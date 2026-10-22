using System;
using System.Collections.Generic;

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

    public class OptionDefinition
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public int? Score { get; set; }
    }

    public class QuestionDefinition
    {
        public string Id { get; set; }
        public QuestionType Type { get; set; }
        public string Text { get; set; }
        public string HelpText { get; set; }
        public bool Required { get; set; } = true;
        public int Order { get; set; }
        public List<OptionDefinition> Options { get; set; } = new();
        public int? Min { get; set; }
        public int? Max { get; set; }
        public string MinLabel { get; set; }
        public string MaxLabel { get; set; }
        public double Weight { get; set; } = 1;
        public string DimensionKey { get; set; }
    }

    public class AssessmentScoringConfig
    {
        public string Mode { get; set; } = "weightedSum";
        public bool NormalizeTo100 { get; set; } = true;
        public List<ScoreLevel> Levels { get; set; } = new();
        public List<ScoreDimension> Dimensions { get; set; } = new();
    }

    public class ScoreLevel
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public string Label { get; set; }
        public string ColorHex { get; set; }
    }

    public class ScoreDimension
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public double Weight { get; set; } = 1;
    }

    public class AssessmentDefinition
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public int EstimatedMinutes { get; set; }
        public int TotalScore { get; set; }
        public List<QuestionDefinition> Questions { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public int QuestionsPerPage { get; set; } = 1;
        public AssessmentScoringConfig Scoring { get; set; }
    }

    public class AnswerValue
    {
        public string QuestionId { get; set; }
        public List<string> SelectedOptionIds { get; set; } = new();
        public double? Numeric { get; set; }
        public string Text { get; set; }
    }

    public class AssessmentSession
    {
        public string DefinitionId { get; set; }
        public string EmployeeId { get; set; }
        public AssessmentStatus Status { get; set; } = AssessmentStatus.NotStarted;
        public int CurrentIndex { get; set; }
        public Dictionary<string, AnswerValue> Answers { get; set; } = new();
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public AssessmentResult Result { get; set; }
    }

    public class AssessmentResult
    {
        public string DefinitionId { get; set; }
        public string EmployeeId { get; set; }
        public string Title { get; set; }
        public double TotalScore { get; set; }
        public string LevelLabel { get; set; }
        public string LevelColorHex { get; set; }
        public Dictionary<string, double> Dimensions { get; set; } = new();
        public Dictionary<string, double> QuestionScores { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public string Summary { get; set; }
        public DateTime ComputedAt { get; set; }
    }

    public interface IAssessmentCatalog
    {
        AssessmentDefinition Get(string id);
        IReadOnlyList<AssessmentDefinition> GetAll();
    }
}
