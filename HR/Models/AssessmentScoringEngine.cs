using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HR.Models
{
    /// <summary>
    /// موتور پردازش خروجی آزمون.
    /// قوانین را از AssessmentDefinition.Scoring و وزن/امتیاز سوال‌ها می‌خواند.
    /// AI اینجا نیست؛ فقط محاسبه قطعی.
    /// </summary>
    public static class AssessmentScoringEngine
    {
        public static AssessmentResult Compute(AssessmentDefinition def, AssessmentSession session)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (session == null) throw new ArgumentNullException(nameof(session));

            var scoring = def.Scoring ?? DefaultScoring();
            var questionScores = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var dimRaw = new Dictionary<string, List<(double value, double weight)>>(StringComparer.OrdinalIgnoreCase);

            double totalWeighted = 0;
            double totalWeight = 0;

            foreach (var q in def.Questions ?? Enumerable.Empty<QuestionDefinition>())
            {
                session.Answers.TryGetValue(q.Id, out var answer);
                var raw = ExtractRawScore(q, answer);
                if (raw == null) continue;

                double normalized = NormalizeQuestion(q, raw.Value, scoring.NormalizeTo100);
                double w = q.Weight <= 0 ? 1 : q.Weight;
                questionScores[q.Id] = Math.Round(normalized, 2);

                totalWeighted += normalized * w;
                totalWeight += w;

                var dimKey = string.IsNullOrWhiteSpace(q.DimensionKey) ? null : q.DimensionKey.Trim();
                if (!string.IsNullOrEmpty(dimKey))
                {
                    if (!dimRaw.TryGetValue(dimKey, out var list))
                    {
                        list = new List<(double, double)>();
                        dimRaw[dimKey] = list;
                    }
                    list.Add((normalized, w));
                }
            }

            double total = totalWeight > 0
                ? (scoring.Mode == "sum" ? questionScores.Values.Sum() : totalWeighted / totalWeight)
                : 0;

            if (scoring.NormalizeTo100 && scoring.Mode == "sum" && questionScores.Count > 0)
                total = total / questionScores.Count;

            total = Math.Max(0, Math.Min(100, Math.Round(total, 1)));

            var dimensions = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in dimRaw)
            {
                var sumW = kv.Value.Sum(x => x.weight);
                var avg = sumW > 0 ? kv.Value.Sum(x => x.value * x.weight) / sumW : 0;
                dimensions[kv.Key] = Math.Round(avg, 1);
            }

            // وزن ابعاد از config (اختیاری برای total جایگزین)
            if (scoring.Dimensions != null && scoring.Dimensions.Count > 0 && dimensions.Count > 0)
            {
                double dw = 0, ds = 0;
                foreach (var d in scoring.Dimensions)
                {
                    if (!dimensions.TryGetValue(d.Key, out var v)) continue;
                    var w = d.Weight <= 0 ? 1 : d.Weight;
                    ds += v * w;
                    dw += w;
                }
                if (dw > 0 && string.Equals(scoring.Mode, "weightedSum", StringComparison.OrdinalIgnoreCase))
                    total = Math.Round(ds / dw, 1);
            }

            var level = ResolveLevel(scoring.Levels, total);
            var summary = BuildSummary(def, total, level?.Label, dimensions);

            var result = new AssessmentResult
            {
                DefinitionId = def.Id,
                EmployeeId = session.EmployeeId,
                Title = def.Title,
                TotalScore = total,
                LevelLabel = level?.Label ?? "—",
                LevelColorHex = level?.ColorHex ?? "#94A3B8",
                Dimensions = dimensions,
                QuestionScores = questionScores,
                Tags = def.Tags != null ? new List<string>(def.Tags) : new List<string>(),
                Summary = summary,
                ComputedAt = DateTime.Now
            };

            session.Result = result;
            return result;
        }

        private static AssessmentScoringConfig DefaultScoring() => new AssessmentScoringConfig
        {
            Mode = "weightedSum",
            NormalizeTo100 = true,
            Levels = new List<ScoreLevel>
            {
                new() { Min = 0, Max = 49.99, Label = "نیاز به توسعه", ColorHex = "#FB7185" },
                new() { Min = 50, Max = 74.99, Label = "قابل قبول", ColorHex = "#FBBF24" },
                new() { Min = 75, Max = 100, Label = "قوی", ColorHex = "#34D399" },
            }
        };

        /// <summary>از جواب خام یک عدد استخراج می‌کند.</summary>
        public static double? ExtractRawScore(QuestionDefinition q, AnswerValue answer)
        {
            if (answer == null) return null;

            switch (q.Type)
            {
                case QuestionType.SingleChoice:
                    if (answer.SelectedOptionIds == null || answer.SelectedOptionIds.Count == 0)
                        return null;
                    var opt = q.Options?.FirstOrDefault(o => o.Id == answer.SelectedOptionIds[0]);
                    return opt?.Score ?? IndexFallback(q, answer.SelectedOptionIds[0]);

                case QuestionType.MultiChoice:
                    if (answer.SelectedOptionIds == null || answer.SelectedOptionIds.Count == 0)
                        return null;
                    double sum = 0;
                    int n = 0;
                    foreach (var id in answer.SelectedOptionIds)
                    {
                        var o = q.Options?.FirstOrDefault(x => x.Id == id);
                        if (o?.Score != null) { sum += o.Score.Value; n++; }
                        else
                        {
                            var fb = IndexFallback(q, id);
                            if (fb != null) { sum += fb.Value; n++; }
                        }
                    }
                    return n > 0 ? sum / n : (double?)null;

                case QuestionType.Likert:
                case QuestionType.Scale:
                    return answer.Numeric;

                case QuestionType.TextShort:
                case QuestionType.TextLong:
                    // متن امتیاز عددی ندارد؛ بعداً AI می‌تواند برچسب بزند
                    return string.IsNullOrWhiteSpace(answer.Text) ? (double?)null : 0;

                default:
                    return answer.Numeric;
            }
        }

        private static double? IndexFallback(QuestionDefinition q, string optionId)
        {
            if (q.Options == null || q.Options.Count == 0) return null;
            int idx = q.Options.FindIndex(o => o.Id == optionId);
            if (idx < 0) return null;
            // ۱-based در نبود Score
            return idx + 1;
        }

        private static double NormalizeQuestion(QuestionDefinition q, double raw, bool to100)
        {
            if (!to100) return raw;

            double min = q.Min ?? 0;
            double max = q.Max ?? 0;

            if (q.Type == QuestionType.SingleChoice || q.Type == QuestionType.MultiChoice)
            {
                if (q.Options != null && q.Options.Count > 0)
                {
                    var scores = q.Options.Where(o => o.Score.HasValue).Select(o => (double)o.Score.Value).ToList();
                    if (scores.Count > 0)
                    {
                        min = scores.Min();
                        max = scores.Max();
                    }
                    else
                    {
                        min = 1;
                        max = q.Options.Count;
                    }
                }
            }

            if (max <= min) return raw <= min ? 0 : 100;
            var t = (raw - min) / (max - min);
            return Math.Max(0, Math.Min(100, t * 100));
        }

        private static ScoreLevel ResolveLevel(List<ScoreLevel> levels, double total)
        {
            if (levels == null || levels.Count == 0) return null;
            return levels.FirstOrDefault(l => total >= l.Min && total <= l.Max)
                   ?? levels.OrderBy(l => l.Min).LastOrDefault();
        }

        private static string BuildSummary(
            AssessmentDefinition def,
            double total,
            string level,
            Dictionary<string, double> dimensions)
        {
            var sb = new StringBuilder();
            sb.Append($"آزمون «{def.Title}» با امتیاز {total:0.#} در سطح «{level ?? "—"}» قرار گرفت.");
            if (dimensions != null && dimensions.Count > 0)
            {
                sb.Append(" ابعاد: ");
                sb.Append(string.Join("، ", dimensions.Select(kv =>
                {
                    var title = def.Scoring?.Dimensions?.FirstOrDefault(d => d.Key == kv.Key)?.Title ?? kv.Key;
                    return $"{title} {kv.Value:0.#}";
                })));
                sb.Append('.');
            }
            return sb.ToString();
        }
    }
}
