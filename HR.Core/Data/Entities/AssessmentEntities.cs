using System;
using System.Collections.Generic;

namespace HR.Data.Entities
{
    public class Assessment
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
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        public ICollection<AssessmentDimension> Dimensions { get; set; } = new List<AssessmentDimension>();
        public ICollection<AssessmentLevel> Levels { get; set; } = new List<AssessmentLevel>();
        public ICollection<AssessmentQuestion> Questions { get; set; } = new List<AssessmentQuestion>();
        public ICollection<AssessmentAttempt> Attempts { get; set; } = new List<AssessmentAttempt>();
    }

    public class AssessmentDimension
    {
        public Guid DimensionId { get; set; }
        public Guid AssessmentId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public double Weight { get; set; }
        public Assessment Assessment { get; set; } = null!;
    }

    public class AssessmentLevel
    {
        public Guid LevelId { get; set; }
        public Guid AssessmentId { get; set; }
        public double MinScore { get; set; }
        public double MaxScore { get; set; }
        public string Label { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#94A3B8";
        public Assessment Assessment { get; set; } = null!;
    }

    public class AssessmentQuestion
    {
        public Guid QuestionId { get; set; }
        public Guid AssessmentId { get; set; }
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
        public Assessment Assessment { get; set; } = null!;
        public ICollection<AssessmentOption> Options { get; set; } = new List<AssessmentOption>();
    }

    public class AssessmentOption
    {
        public Guid OptionId { get; set; }
        public Guid QuestionId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public double Score { get; set; }
        public int SortOrder { get; set; }
        public AssessmentQuestion Question { get; set; } = null!;
    }

    public class AssessmentAttempt
    {
        public Guid AttemptId { get; set; }
        public Guid AssessmentId { get; set; }
        public Guid EmployeeId { get; set; }
        public string Status { get; set; } = "Completed";
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public double? TotalScore { get; set; }
        public string? LevelLabel { get; set; }
        public string? Summary { get; set; }
        public Assessment Assessment { get; set; } = null!;
        public Employee Employee { get; set; } = null!;
        public ICollection<AssessmentAnswer> Answers { get; set; } = new List<AssessmentAnswer>();
    }

    public class AssessmentAnswer
    {
        public Guid AnswerId { get; set; }
        public Guid AttemptId { get; set; }
        public Guid QuestionId { get; set; }
        public Guid? OptionId { get; set; }
        public double? NumericValue { get; set; }
        public string? TextValue { get; set; }
        public double Score { get; set; }
        public AssessmentAttempt Attempt { get; set; } = null!;
        public AssessmentQuestion Question { get; set; } = null!;
        public AssessmentOption? Option { get; set; }
    }
}
