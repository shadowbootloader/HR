using System;
using System.Collections.Generic;
using System.Linq;

namespace HR.Models
{
    public enum AssessmentStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Locked,
        Outlier
    }

    public enum QuestionType
    {
        SingleChoice,
        MultiChoice,
        Likert,
        Scale,
        TextShort,
        TextLong,
        Ranking,
        Matrix
    }

    /// <summary>
    /// پیش‌فرض‌های هر نوع سؤال. موتور امتیازدهی به‌جای صفر فرض کردن Min/Max،
    /// از این مقادیر استفاده می‌کند تا سؤال بدون بازه، اشتباهاً نمره ۱۰۰ نگیرد.
    /// </summary>
    public static class QuestionTypeInfo
    {
        public static int DefaultMin(QuestionType type) => type switch
        {
            QuestionType.Likert => 1,
            QuestionType.Scale => 0,
            _ => 0
        };

        public static int DefaultMax(QuestionType type) => type switch
        {
            QuestionType.Likert => 5,
            QuestionType.Scale => 10,
            _ => 0
        };

        public static bool IsNumeric(QuestionType type) =>
            type is QuestionType.Likert or QuestionType.Scale;

        public static bool IsChoice(QuestionType type) =>
            type is QuestionType.SingleChoice or QuestionType.MultiChoice;

        public static bool IsText(QuestionType type) =>
            type is QuestionType.TextShort or QuestionType.TextLong;
    }

    public class OptionDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public int? Score { get; set; }
    }

    public class QuestionDefinition
    {
        public string Id { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? HelpText { get; set; }
        public bool Required { get; set; } = true;
        public int Order { get; set; }
        public List<OptionDefinition> Options { get; set; } = new();
        public int? Min { get; set; }
        public int? Max { get; set; }
        public string? MinLabel { get; set; }
        public string? MaxLabel { get; set; }
        public double Weight { get; set; } = 1;
        public string? DimensionKey { get; set; }

        /// <summary>
        /// اگر false باشد سؤال امتیاز می‌گیرد ولی در نمره کل شمرده نمی‌شود.
        /// جایگزین ترفند قبلی «Weight = 0» است.
        /// </summary>
        public bool CountsTowardTotal { get; set; } = true;

        /// <summary>حد پایین مؤثر؛ از نوع سؤال مشتق می‌شود اگر Min ست نشده باشد.</summary>
        public int EffectiveMin => Min ?? QuestionTypeInfo.DefaultMin(Type);

        /// <summary>حد بالای مؤثر؛ از نوع سؤال مشتق می‌شود اگر Max ست نشده باشد.</summary>
        public int EffectiveMax => Max ?? QuestionTypeInfo.DefaultMax(Type);

        /// <summary>آیا این سؤال در محاسبه نمره کل وارد می‌شود؟ (سؤال متنی هرگز وارد نمی‌شود)</summary>
        public bool IsScored =>
            CountsTowardTotal &&
            Weight > 0 &&
            !QuestionTypeInfo.IsText(Type);
    }

    public class AssessmentScoringConfig
    {
        public const string ModeWeightedSum = "weightedSum";
        public const string ModeAverage = "average";
        public const string ModeSum = "sum";
        public const string ModeDimensionWeighted = "dimensionWeighted";

        public string Mode { get; set; } = ModeWeightedSum;

        public bool NormalizeTo100 { get; set; } = true;

        /// <summary>اگر true باشد، سؤال بی‌پاسخ صفر حساب می‌شود؛ وگرنه از محاسبه خارج می‌شود.</summary>
        public bool TreatUnansweredAsZero { get; set; }

        public List<ScoreLevel> Levels { get; set; } = new();
        public List<ScoreDimension> Dimensions { get; set; } = new();

        public bool IsSum => string.Equals(Mode, ModeSum, StringComparison.OrdinalIgnoreCase);

        public bool IsDimensionWeighted =>
            string.Equals(Mode, ModeDimensionWeighted, StringComparison.OrdinalIgnoreCase);
    }

    public class ScoreLevel
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public string Label { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#94A3B8";

        public bool Contains(double value) => value >= Min && value <= Max;
    }

    public class ScoreDimension
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public double Weight { get; set; } = 1;
    }

    public class AssessmentDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Description { get; set; }
        public int EstimatedMinutes { get; set; }
        public List<QuestionDefinition> Questions { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public int QuestionsPerPage { get; set; } = 1;
        public AssessmentScoringConfig Scoring { get; set; } = new();

        /// <summary>حداکثر امتیاز ممکن (نمایشی/تشخیصی).</summary>
        public double MaxScore => Questions.Where(q => q.IsScored).Sum(q => q.Weight) * 100;
    }

    public class AnswerValue
    {
        public string QuestionId { get; set; } = string.Empty;
        public List<string> SelectedOptionIds { get; set; } = new();
        public double? Numeric { get; set; }
        public string? Text { get; set; }

        public bool HasAnyValue =>
            SelectedOptionIds.Count > 0 || Numeric.HasValue || !string.IsNullOrWhiteSpace(Text);
    }

    public class AssessmentSession
    {
        public string DefinitionId { get; set; } = string.Empty;
        public string? EmployeeId { get; set; }
        public AssessmentStatus Status { get; set; } = AssessmentStatus.NotStarted;
        public int CurrentIndex { get; set; }

        /// <summary>مقایسه‌ی کلید بدون حساسیت به بزرگی/کوچکی حرف.</summary>
        public Dictionary<string, AnswerValue> Answers { get; set; } =
            new(StringComparer.OrdinalIgnoreCase);

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public AssessmentResult? Result { get; set; }
    }

    public class AssessmentResult
    {
        public string DefinitionId { get; set; } = string.Empty;
        public string? EmployeeId { get; set; }
        public string Title { get; set; } = string.Empty;

        /// <summary>نمره نهایی نرمال‌شده (۰ تا ۱۰۰).</summary>
        public double TotalScore { get; set; }

        /// <summary>مجموع وزنی خام قبل از نرمال‌سازی نهایی.</summary>
        public double RawScore { get; set; }

        /// <summary>تعداد سؤال‌هایی که واقعاً در نمره شمرده شدند.</summary>
        public int CountedQuestions { get; set; }

        public string LevelLabel { get; set; } = "—";
        public string LevelColorHex { get; set; } = "#94A3B8";

        public Dictionary<string, double> Dimensions { get; set; } =
            new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, double> QuestionScores { get; set; } =
            new(StringComparer.OrdinalIgnoreCase);

        public List<string> Tags { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
        public DateTime ComputedAt { get; set; } = DateTime.Now;
    }

    public interface IAssessmentCatalog
    {
        AssessmentDefinition? Get(string id);
        IReadOnlyList<AssessmentDefinition> GetAll();
    }
}