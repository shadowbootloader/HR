using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HR.Models
{
    /// <summary>
    /// موتور پردازش خروجی آزمون. قوانین را از AssessmentDefinition.Scoring می‌خواند.
    /// AI اینجا نیست؛ فقط محاسبه قطعی و قابل تست.
    /// </summary>
    public static class AssessmentScoringEngine
    {
        public static AssessmentResult Compute(AssessmentDefinition def, AssessmentSession session)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (session == null) throw new ArgumentNullException(nameof(session));

            var scoring = def.Scoring ?? DefaultScoring();
            var questions = def.Questions ?? new List<QuestionDefinition>();

            var questionScores = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var dimRaw = new Dictionary<string, List<(double Value, double Weight)>>(StringComparer.OrdinalIgnoreCase);

            double weightedSum = 0;
            double weightTotal = 0;
            int counted = 0;

            foreach (var q in questions)
            {
                if (q == null || string.IsNullOrWhiteSpace(q.Id)) continue;
                if (!q.IsScored) continue;

                var answer = ResolveAnswer(session, q.Id);
                var raw = ExtractRawScore(q, answer);

                if (raw == null && !scoring.TreatUnansweredAsZero) continue;

                double normalized = raw == null ? 0 : NormalizeQuestion(q, raw.Value, scoring.NormalizeTo100);
                double weight = q.Weight;

                questionScores[q.Id] = Math.Round(normalized, 2);
                weightedSum += normalized * weight;
                weightTotal += weight;
                counted++;

                var dimKey = string.IsNullOrWhiteSpace(q.DimensionKey) ? null : q.DimensionKey.Trim();
                if (dimKey != null)
                {
                    if (!dimRaw.TryGetValue(dimKey, out var list))
                    {
                        list = new List<(double, double)>();
                        dimRaw[dimKey] = list;
                    }
                    list.Add((normalized, weight));
                }
            }

            // ---- نمره کل: سه رفتار مستقل، دیگر همه به یک جا ختم نمی‌شوند ----
            double total;
            if (scoring.IsSum)
            {
                // میانگین ساده: وزن‌ها نادیده گرفته می‌شوند
                total = questionScores.Count > 0
                    ? questionScores.Values.Sum() / questionScores.Count
                    : 0;
            }
            else
            {
                // میانگین وزنی بر اساس وزن سؤال‌ها
                total = weightTotal > 0 ? weightedSum / weightTotal : 0;
            }

            // ---- نمره‌ی ابعاد ----
            var dimensions = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in dimRaw)
            {
                var sumW = kv.Value.Sum(x => x.Weight);
                var avg = sumW > 0 ? kv.Value.Sum(x => x.Value * x.Weight) / sumW : 0;
                dimensions[kv.Key] = Math.Round(avg, 1);
            }

            // فقط حالت صریحاً dimensionWeighted نمره کل را از ابعاد می‌سازد
            if (scoring.IsDimensionWeighted && dimensions.Count > 0 && scoring.Dimensions.Count > 0)
            {
                double dw = 0, ds = 0;
                foreach (var d in scoring.Dimensions)
                {
                    if (d == null || string.IsNullOrWhiteSpace(d.Key)) continue;
                    if (!dimensions.TryGetValue(d.Key.Trim(), out var v)) continue;
                    var w = d.Weight <= 0 ? 1 : d.Weight;
                    ds += v * w;
                    dw += w;
                }
                if (dw > 0) total = ds / dw;
            }

            if (scoring.NormalizeTo100) total = Math.Clamp(total, 0, 100);
            total = Math.Round(total, 1);

            var level = ResolveLevel(scoring.Levels, total);

            var result = new AssessmentResult
            {
                DefinitionId = def.Id,
                EmployeeId = session.EmployeeId,
                Title = def.Title,
                TotalScore = total,
                RawScore = Math.Round(weightedSum, 2),
                CountedQuestions = counted,
                LevelLabel = level?.Label ?? "—",
                LevelColorHex = level?.ColorHex ?? "#94A3B8",
                Dimensions = dimensions,
                QuestionScores = questionScores,
                Tags = def.Tags != null ? new List<string>(def.Tags) : new List<string>(),
                Summary = BuildSummary(def, total, level?.Label, dimensions),
                ComputedAt = DateTime.Now
            };

            session.Result = result;
            return result;
        }

        // =========================================================
        //  استخراج امتیاز خام
        // =========================================================

        public static double? ExtractRawScore(QuestionDefinition q, AnswerValue? answer)
        {
            if (q == null || answer == null) return null;

            switch (q.Type)
            {
                case QuestionType.SingleChoice:
                    {
                        var id = answer.SelectedOptionIds.FirstOrDefault();
                        return string.IsNullOrEmpty(id) ? null : ScoreForOption(q, id);
                    }

                case QuestionType.MultiChoice:
                    {
                        if (answer.SelectedOptionIds.Count == 0) return null;
                        double sum = 0;
                        int n = 0;
                        foreach (var id in answer.SelectedOptionIds)
                        {
                            var s = ScoreForOption(q, id);
                            if (s.HasValue) { sum += s.Value; n++; }
                        }
                        // میانگین گزینه‌های انتخاب‌شده (نه مجموع)
                        return n > 0 ? sum / n : (double?)null;
                    }

                case QuestionType.Likert:
                case QuestionType.Scale:
                case QuestionType.Ranking:
                case QuestionType.Matrix:
                    return answer.Numeric;

                default: // سؤال متنی: مقدار عددی ندارد
                    return string.IsNullOrWhiteSpace(answer.Text) ? null : 0d;
            }
        }

        private static double? ScoreForOption(QuestionDefinition q, string optionId)
        {
            if (q.Options == null || q.Options.Count == 0) return null;

            int index = q.Options.FindIndex(o =>
                string.Equals(o.Id, optionId, StringComparison.OrdinalIgnoreCase));

            if (index < 0) return null;

            // اگر Score نبود، رتبه گزینه (۱-based) استفاده می‌شود
            return q.Options[index].Score ?? index + 1;
        }

        // =========================================================
        //  نرمال‌سازی
        // =========================================================

        private static double NormalizeQuestion(QuestionDefinition q, double raw, bool to100)
        {
            if (!to100) return raw;

            double min = q.EffectiveMin;
            double max = q.EffectiveMax;

            if (QuestionTypeInfo.IsChoice(q.Type) && q.Options.Count > 0)
            {
                var scores = q.Options
                    .Where(o => o.Score.HasValue)
                    .Select(o => (double)o.Score!.Value)
                    .ToList();

                if (scores.Count > 0) { min = scores.Min(); max = scores.Max(); }
                else { min = 1; max = q.Options.Count; }
            }

            // فقط انواع بدون بازه (مثل Ranking/Matrix بدون Min/Max) به اینجا می‌رسند
            if (max <= min) return raw <= min ? 0 : 100;

            var t = (raw - min) / (max - min);
            return Math.Clamp(t * 100, 0, 100);
        }

        // =========================================================
        //  سطح‌بندی
        // =========================================================

        private static ScoreLevel? ResolveLevel(List<ScoreLevel>? levels, double total)
        {
            if (levels == null || levels.Count == 0) return null;

            var exact = levels.FirstOrDefault(l => l.Contains(total));
            if (exact != null) return exact;

            // نزدیک‌ترین بازه به نمره، به‌جای «همیشه بالاترین سطح»
            return levels
                .OrderBy(l => total < l.Min ? l.Min - total
                            : total > l.Max ? total - l.Max
                            : 0)
                .First();
        }

        // =========================================================
        //  کمکی‌ها
        // =========================================================

        private static AnswerValue? ResolveAnswer(AssessmentSession session, string questionId)
        {
            if (session.Answers == null) return null;

            if (session.Answers.TryGetValue(questionId, out var exact)) return exact;

            // پشتیبان: اگر سشن با دیکشنری پیش‌فرض ساخته شده باشد
            foreach (var kv in session.Answers)
                if (string.Equals(kv.Key, questionId, StringComparison.OrdinalIgnoreCase))
                    return kv.Value;

            return null;
        }

        public static AssessmentScoringConfig DefaultScoring() => new()
        {
            Mode = AssessmentScoringConfig.ModeWeightedSum,
            NormalizeTo100 = true,
            Levels = new List<ScoreLevel>
            {
                new() { Min = 0,    Max = 49.99, Label = "نیاز به توسعه", ColorHex = "#FB7185" },
                new() { Min = 50,   Max = 74.99, Label = "قابل قبول",     ColorHex = "#FBBF24" },
                new() { Min = 75,   Max = 100,   Label = "قوی",           ColorHex = "#34D399" },
            }
        };

        private static string BuildSummary(
            AssessmentDefinition def,
            double total,
            string? level,
            Dictionary<string, double> dimensions)
        {
            var sb = new StringBuilder();
            sb.Append($"آزمون «{def.Title}» با امتیاز {total:0.#} در سطح «{level ?? "—"}» قرار گرفت.");

            if (dimensions.Count > 0)
            {
                sb.Append(" ابعاد: ");
                sb.Append(string.Join("، ", dimensions.Select(kv =>
                {
                    var title = def.Scoring?.Dimensions?
                        .FirstOrDefault(d => string.Equals(d.Key, kv.Key, StringComparison.OrdinalIgnoreCase))?
                        .Title ?? kv.Key;
                    return $"{title} {kv.Value:0.#}";
                })));
                sb.Append('.');
            }

            return sb.ToString();
        }

        /// <summary>
        /// اعتبارسنجی تعریف آزمون پیش از اجرا. لیست خطاها را برمی‌گرداند (خالی = سالم).
        /// </summary>
        public static IReadOnlyList<string> Validate(AssessmentDefinition? def)
        {
            var errors = new List<string>();
            if (def == null) { errors.Add("تعریف آزمون null است."); return errors; }

            if (string.IsNullOrWhiteSpace(def.Id)) errors.Add("Id آزمون خالی است.");
            if (string.IsNullOrWhiteSpace(def.Title)) errors.Add("عنوان آزمون خالی است.");
            if (def.Questions == null || def.Questions.Count == 0) errors.Add("آزمون هیچ سؤالی ندارد.");

            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var q in def.Questions ?? new List<QuestionDefinition>())
            {
                if (q == null) continue;

                if (string.IsNullOrWhiteSpace(q.Id)) { errors.Add("یک سؤال بدون Id وجود دارد."); continue; }
                if (!ids.Add(q.Id)) errors.Add($"Id سؤال تکراری: {q.Id}");

                if (QuestionTypeInfo.IsChoice(q.Type) && q.Options.Count == 0)
                    errors.Add($"سؤال گزینه‌ای «{q.Id}» گزینه ندارد.");

                if (q.IsScored && q.EffectiveMax <= q.EffectiveMin)
                    errors.Add($"بازه سؤال «{q.Id}» نامعتبر است.");

                if (q.Weight < 0) errors.Add($"وزن سؤال «{q.Id}» منفی است.");
            }

            var levels = def.Scoring?.Levels;
            if (levels == null || levels.Count == 0) errors.Add("سطح‌بندی (Levels) تعریف نشده است.");
            else if (levels.Any(l => l.Min > l.Max)) errors.Add("یک سطح، Min بزرگ‌تر از Max دارد.");

            return errors;
        }
    }
}