using System.Collections.Generic;
using System.Linq;

namespace HR.Models
{
    public class InMemoryAssessmentCatalog : IAssessmentCatalog
    {
        private readonly List<AssessmentDefinition> _items;

        public InMemoryAssessmentCatalog()
        {
            _items = new List<AssessmentDefinition>
            {
                ManagerEval(),
                Simple("mbti", "شخصیت‌شناسی MBTI", "روانشناسی", QuestionType.Likert),
                Simple("comm-skills", "تست مهارت‌های ارتباطی", "مهارتی", QuestionType.SingleChoice),
                Simple("decision", "تست تصمیم‌گیری", "موقعیتی", QuestionType.Scale),
                Simple("self-efficacy", "تست خودکارآمدی", "روانشناسی", QuestionType.Likert),
                Simple("discipline", "ارزیابی انضباط سازمانی", "انضباطی", QuestionType.Likert),
                Simple("problem-solving", "تست حل مسئله", "موقعیتی", QuestionType.TextLong),
                Simple("creativity", "تست خلاقیت", "توانمندی‌ها", QuestionType.Scale),
                Simple("360", "ارزیابی ۳۶۰ درجه", "شغلی", QuestionType.Likert),
                Simple("motivation", "تست انگیزش شغلی", "روانشناسی", QuestionType.Likert),
            };
        }

        public AssessmentDefinition Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return _items.FirstOrDefault(x => x.Id == id);
        }

        public IReadOnlyList<AssessmentDefinition> GetAll() => _items;

        private static AssessmentScoringConfig StandardScoring() => new AssessmentScoringConfig
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

        private static AssessmentDefinition Simple(string id, string title, string category, QuestionType type)
        {
            return new AssessmentDefinition
            {
                Id = id,
                Title = title,
                Category = category,
                EstimatedMinutes = 10,
                QuestionsPerPage = 2,
                Scoring = StandardScoring(),
                Questions = new List<QuestionDefinition>
                {
                    new QuestionDefinition
                    {
                        Id = "q1", Order = 1, Type = type,
                        Text = $"سوال نمونه برای «{title}»",
                        Required = true,
                        Min = 1, Max = type == QuestionType.Scale ? 10 : 5,
                        MinLabel = "کم", MaxLabel = "زیاد",
                        Weight = 1, DimensionKey = "overall",
                        Options = new List<OptionDefinition>
                        {
                            new() { Id = "a", Text = "گزینه الف", Score = 1 },
                            new() { Id = "b", Text = "گزینه ب", Score = 2 },
                            new() { Id = "c", Text = "گزینه ج", Score = 3 },
                        }
                    },
                    new QuestionDefinition
                    {
                        Id = "q2", Order = 2, Type = QuestionType.TextShort,
                        Text = "توضیح کوتاه خود را بنویسید",
                        Required = false, Weight = 0
                    }
                }
            };
        }

        private static AssessmentDefinition ManagerEval()
        {
            return new AssessmentDefinition
            {
                Id = "manager-eval",
                Title = "ارزیابی مدیر",
                Category = "شغلی",
                Description = "ارزیابی مهارت‌های مدیریتی",
                EstimatedMinutes = 15,
                QuestionsPerPage = 2,
                Tags = new List<string> { "leadership", "job" },
                Scoring = new AssessmentScoringConfig
                {
                    Mode = "weightedSum",
                    NormalizeTo100 = true,
                    Dimensions = new List<ScoreDimension>
                    {
                        new() { Key = "leadership", Title = "رهبری", Weight = 0.6 },
                        new() { Key = "communication", Title = "ارتباطات", Weight = 0.4 },
                    },
                    Levels = new List<ScoreLevel>
                    {
                        new() { Min = 0, Max = 49.99, Label = "نیاز به توسعه", ColorHex = "#FB7185" },
                        new() { Min = 50, Max = 74.99, Label = "قابل قبول", ColorHex = "#FBBF24" },
                        new() { Min = 75, Max = 100, Label = "قوی", ColorHex = "#34D399" },
                    }
                },
                Questions = new List<QuestionDefinition>
                {
                    new QuestionDefinition
                    {
                        Id = "q1", Order = 1, Type = QuestionType.Likert,
                        Text = "در تصمیم‌گیری‌های تیمی مشارکت فعال دارم",
                        HelpText = "نزدیک‌ترین گزینه را انتخاب کنید",
                        Required = true, Min = 1, Max = 5,
                        MinLabel = "کاملاً مخالفم", MaxLabel = "کاملاً موافقم",
                        Weight = 1, DimensionKey = "leadership"
                    },
                    new QuestionDefinition
                    {
                        Id = "q2", Order = 2, Type = QuestionType.SingleChoice,
                        Text = "سبک مدیریتی غالب شما کدام است؟",
                        Required = true, Weight = 1, DimensionKey = "leadership",
                        Options = new List<OptionDefinition>
                        {
                            new() { Id = "a", Text = "دستوری", Score = 1 },
                            new() { Id = "b", Text = "مشارکتی", Score = 3 },
                            new() { Id = "c", Text = "تفویضی", Score = 2 },
                        }
                    },
                    new QuestionDefinition
                    {
                        Id = "q3", Order = 3, Type = QuestionType.Scale,
                        Text = "توانایی اولویت‌بندی کارهای تیم را چقدر ارزیابی می‌کنید؟",
                        Required = true, Min = 0, Max = 10,
                        MinLabel = "ضعیف", MaxLabel = "عالی",
                        Weight = 1.2, DimensionKey = "leadership"
                    },
                    new QuestionDefinition
                    {
                        Id = "q4", Order = 4, Type = QuestionType.TextLong,
                        Text = "یک موقعیت اخیر را شرح دهید که بازخورد سازنده دادید",
                        Required = true, Weight = 0, DimensionKey = "communication"
                    }
                }
            };
        }
    }
}
