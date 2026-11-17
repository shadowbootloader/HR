using HR.Services.Data;

namespace HR.Services.Assessments
{
    /// <summary>خودارزیابی اسلایدری. متن سؤال‌ها همان برچسب‌های فرم است.</summary>
    public static class SelfAssessmentCatalog
    {
        public const string Code = "self-sliders";

        public static AssessmentRecord Create()
        {
            var record = new AssessmentRecord
            {
                Code = Code,
                Title = "خودارزیابی",
                Category = "خودارزیابی",
                Description = "امتیاز هر مورد از ۰ تا ۱۰۰",
                ScoringMode = "weightedSum",
                NormalizeTo100 = true,
                EstimatedMinutes = 5,
                QuestionsPerPage = 2
            };

            var order = 1;
            foreach (var dimension in ProfileInsights.Dimensions)
            {
                record.Questions.Add(new QuestionRecord
                {
                    Code = "s" + order,
                    SortOrder = order,
                    QuestionType = "Scale",
                    Text = dimension.Label,
                    IsRequired = true,
                    CountsTowardTotal = true,
                    Weight = 1,
                    MinValue = 0,
                    MaxValue = 100,
                    MinLabel = "۰",
                    MaxLabel = "۱۰۰"
                });
                order++;
            }

            return record;
        }

        public static AttemptSave ToAttempt(Guid employeeId, SelfAssessmentScores scores, DateTime when)
        {
            var values = ProfileInsights.Dimensions.Select(item => (double)item.Value(scores)).ToList();
            var average = values.Count == 0 ? 0 : values.Average();
            return new AttemptSave
            {
                AssessmentCode = Code,
                EmployeeId = employeeId,
                StartedAt = when,
                SubmittedAt = when,
                TotalScore = Math.Round(average, 2),
                LevelLabel = average >= 75 ? "قوی" : average >= 50 ? "قابل قبول" : "نیاز به توسعه",
                Summary = "میانگین خودارزیابی",
                Answers = values.Select((value, index) => new AnswerSave
                {
                    QuestionCode = "s" + (index + 1),
                    NumericValue = value,
                    Score = value
                }).ToList()
            };
        }
    }
}
