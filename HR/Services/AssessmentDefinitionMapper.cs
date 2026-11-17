using HR.Models;
using HR.Services.Assessments;

namespace HR.Services
{
    public static class AssessmentDefinitionMapper
    {
        public static AssessmentDefinition ToDefinition(AssessmentRecord record)
        {
            return new AssessmentDefinition
            {
                Id = record.Code,
                Title = record.Title,
                Category = record.Category,
                Description = record.Description,
                EstimatedMinutes = record.EstimatedMinutes,
                QuestionsPerPage = record.QuestionsPerPage,
                Scoring = new AssessmentScoringConfig
                {
                    Mode = record.ScoringMode,
                    NormalizeTo100 = record.NormalizeTo100,
                    Dimensions = record.Dimensions.Select(item => new ScoreDimension
                    {
                        Key = item.Key,
                        Title = item.Title,
                        Weight = item.Weight
                    }).ToList(),
                    Levels = record.Levels.Select(item => new ScoreLevel
                    {
                        Min = item.MinScore,
                        Max = item.MaxScore,
                        Label = item.Label,
                        ColorHex = item.ColorHex
                    }).ToList()
                },
                Questions = record.Questions.OrderBy(item => item.SortOrder).Select(item => new QuestionDefinition
                {
                    Id = item.Code,
                    Order = item.SortOrder,
                    Type = Enum.Parse<QuestionType>(item.QuestionType, ignoreCase: true),
                    Text = item.Text,
                    HelpText = item.HelpText,
                    Required = item.IsRequired,
                    CountsTowardTotal = item.CountsTowardTotal,
                    Weight = item.Weight,
                    DimensionKey = item.DimensionKey,
                    Min = item.MinValue,
                    Max = item.MaxValue,
                    MinLabel = item.MinLabel,
                    MaxLabel = item.MaxLabel,
                    Options = item.Options.Select(option => new OptionDefinition
                    {
                        Id = option.Code,
                        Text = option.Text,
                        Score = (int)Math.Round(option.Score)
                    }).ToList()
                }).ToList()
            };
        }
    }
}
