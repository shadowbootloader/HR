namespace HR.Services.Assessments
{
    public sealed class AssessmentRecord
    {
        public Guid AssessmentId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public string ScoringMode { get; set; } = "weightedSum";
        public bool NormalizeTo100 { get; set; } = true;
        public int EstimatedMinutes { get; set; }
        public int QuestionsPerPage { get; set; } = 2;
        public List<DimensionRecord> Dimensions { get; set; } = new();
        public List<LevelRecord> Levels { get; set; } = new();
        public List<QuestionRecord> Questions { get; set; } = new();
    }

    public sealed class DimensionRecord
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public double Weight { get; set; }
    }

    public sealed class LevelRecord
    {
        public double MinScore { get; set; }
        public double MaxScore { get; set; }
        public string Label { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#94A3B8";
    }

    public sealed class QuestionRecord
    {
        public string Code { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? HelpText { get; set; }
        public bool IsRequired { get; set; } = true;
        public bool CountsTowardTotal { get; set; } = true;
        public double Weight { get; set; } = 1;
        public string? DimensionKey { get; set; }
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public string? MinLabel { get; set; }
        public string? MaxLabel { get; set; }
        public List<OptionRecord> Options { get; set; } = new();
    }

    public sealed class OptionRecord
    {
        public string Code { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public double Score { get; set; }
        public int SortOrder { get; set; }
    }

    public sealed class AttemptSave
    {
        public string AssessmentCode { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime SubmittedAt { get; set; }
        public double TotalScore { get; set; }
        public string? LevelLabel { get; set; }
        public string? Summary { get; set; }
        public List<AnswerSave> Answers { get; set; } = new();
    }

    public sealed class AnswerSave
    {
        public string QuestionCode { get; set; } = string.Empty;
        public string? OptionCode { get; set; }
        public double? NumericValue { get; set; }
        public string? TextValue { get; set; }
        public double Score { get; set; }
    }

    public sealed class AttemptSummary
    {
        public Guid AttemptId { get; set; }
        public Guid EmployeeId { get; set; }
        public string AssessmentCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double? TotalScore { get; set; }
        public string? LevelLabel { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }
}
