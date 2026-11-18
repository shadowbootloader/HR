using System.Globalization;
using HR.Services.Assessments;

namespace HR.Services.Data
{
    public sealed class SelfAssessmentScores
    {
        public byte Discipline { get; init; }
        public byte Speed { get; init; }
        public byte Effort { get; init; }
        public byte Negotiation { get; init; }
        public byte Patience { get; init; }
        public byte Creativity { get; init; }
        public byte Punctuality { get; init; }
        public byte Teamwork { get; init; }
        public byte Accuracy { get; init; }
        public byte Planning { get; init; }
    }

    public sealed class SkillBar
    {
        public string Title { get; init; } = string.Empty;
        public string? LevelTitle { get; init; }
    }

    public sealed class CourseCertificate
    {
        public string Title { get; init; } = string.Empty;
        public string? Institution { get; init; }
        public bool HasCertificate { get; init; }
    }

    public sealed class ProfileSnapshot
    {
        public bool HasSelfAssessment { get; init; }
        public IReadOnlyList<NamedCount> Competencies { get; init; } = Array.Empty<NamedCount>();
        public IReadOnlyList<string> Strengths { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> Improvements { get; init; } = Array.Empty<string>();
        public IReadOnlyList<NamedCount> Skills { get; init; } = Array.Empty<NamedCount>();
        public IReadOnlyList<MonthCount> Performance { get; init; } = Array.Empty<MonthCount>();
        public IReadOnlyList<AttemptSummary> Achievements { get; init; } = Array.Empty<AttemptSummary>();
        public IReadOnlyList<CourseCertificate> Certificates { get; init; } = Array.Empty<CourseCertificate>();
    }

    public static class ProfileInsights
    {
        public const int StrengthAt = 75;
        public const int ImproveAt = 40;

        public static readonly (string Label, Func<SelfAssessmentScores, byte> Value)[] Dimensions =
        {
            ("نظم", scores => scores.Discipline),
            ("سرعت انجام امور", scores => scores.Speed),
            ("تلاش و پشتکار", scores => scores.Effort),
            ("قدرت مذاکره", scores => scores.Negotiation),
            ("صبر و شکیبایی", scores => scores.Patience),
            ("خلاقیت", scores => scores.Creativity),
            ("وقت‌شناسی", scores => scores.Punctuality),
            ("کار گروهی", scores => scores.Teamwork),
            ("دقت و ریزبینی", scores => scores.Accuracy),
            ("برنامه‌ریزی", scores => scores.Planning)
        };

        public static int? PercentForLevel(string? title) => title switch
        {
            "مقدماتی" => 25,
            "متوسط" => 50,
            "پیشرفته" => 75,
            "حرفه‌ای" => 100,
            _ => null
        };

        public static ProfileSnapshot Build(
            SelfAssessmentScores? scores,
            IEnumerable<AttemptSummary> attempts,
            IEnumerable<SkillBar> skills,
            IEnumerable<CourseCertificate> courses,
            DateTime today)
        {
            IReadOnlyList<NamedCount> competencies = scores == null
                ? Array.Empty<NamedCount>()
                : Dimensions.Select(item => new NamedCount { Name = item.Label, Count = item.Value(scores) }).ToList();

            var strengths = competencies.Where(item => item.Count >= StrengthAt).Select(item => item.Name).ToList();
            var improvements = competencies.Where(item => item.Count <= ImproveAt).Select(item => item.Name).ToList();

            var skillBars = new List<NamedCount>();
            foreach (var skill in skills)
            {
                var percent = PercentForLevel(skill.LevelTitle);
                if (percent == null) continue;
                var name = string.IsNullOrWhiteSpace(skill.Title) ? skill.LevelTitle! : skill.Title.Trim();
                skillBars.Add(new NamedCount { Name = name, Count = percent.Value });
            }

            var history = attempts
                .Where(item => item.AssessmentCode != SelfAssessmentCatalog.Code)
                .Where(item => item.SubmittedAt != null && item.TotalScore != null)
                .OrderBy(item => item.SubmittedAt)
                .ToList();

            return new ProfileSnapshot
            {
                HasSelfAssessment = scores != null,
                Competencies = competencies,
                Strengths = strengths,
                Improvements = improvements,
                Skills = skillBars,
                Performance = AverageByMonth(history, today),
                Achievements = history.OrderByDescending(item => item.SubmittedAt).ToList(),
                Certificates = courses.Where(item => item.HasCertificate && !string.IsNullOrWhiteSpace(item.Title)).ToList()
            };
        }

        private static IReadOnlyList<MonthCount> AverageByMonth(IReadOnlyList<AttemptSummary> history, DateTime today)
        {
            var calendar = new PersianCalendar();
            var buckets = DashboardAggregator.HireMonths(Array.Empty<DateTime>(), today)
                .Select(item => new MonthCount { Year = item.Year, Month = item.Month, Label = item.Label, Count = 0 })
                .ToList();
            if (history.Count == 0) return Array.Empty<MonthCount>();

            var grouped = history.GroupBy(item => (calendar.GetYear(item.SubmittedAt!.Value), calendar.GetMonth(item.SubmittedAt.Value)))
                .ToDictionary(group => group.Key, group => (int)Math.Round(group.Average(item => item.TotalScore!.Value)));

            return buckets.Select(bucket => new MonthCount
            {
                Year = bucket.Year,
                Month = bucket.Month,
                Label = bucket.Label,
                Count = grouped.TryGetValue((bucket.Year, bucket.Month), out var score) ? score : 0
            }).ToList();
        }
    }
}
