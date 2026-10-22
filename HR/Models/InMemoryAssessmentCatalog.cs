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
                Simple("mbti",            "شخصیت‌شناسی MBTI",       "روانشناسی", QuestionType.Likert),
                Simple("comm-skills",     "تست مهارت‌های ارتباطی",   "مهارتی",    QuestionType.SingleChoice),
                Simple("decision",        "تست تصمیم‌گیری",          "موقعیتی",   QuestionType.Scale),
                Simple("self-efficacy",   "تست خودکارآمدی",          "روانشناسی", QuestionType.Likert),
                Simple("discipline",      "ارزیابی انضباط سازمانی",   "انضباطی",   QuestionType.Likert),
                Simple("problem-solving", "تست حل مسئله",            "موقعیتی",   QuestionType.TextLong),
                Simple("creativity",      "تست خلاقیت",              "توانمندی‌ها", QuestionType.Scale),
                Simple("360",             "ارزیابی ۳۶۰ درجه",        "شغلی",      QuestionType.Likert),
                Simple("motivation",      "تست انگیزش شغلی",         "روانشناسی", QuestionType.Likert),
            };
        }

        public AssessmentDefinition? Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return _items.FirstOrDefault(x =>
                string.Equals(x.Id, id, System.StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyList<AssessmentDefinition> GetAll() => _items;

        // =========================================================

        private static AssessmentScoringConfig StandardScoring() => new()
        {
            Mode = AssessmentScoringConfig.ModeWeightedSum,
            NormalizeTo100 = true,
            Levels = new List<ScoreLevel>
            {
                new() { Min = 0,  Max = 49.99, Label = "نیاز به توسعه", ColorHex = "#FB7185" },
                new() { Min = 50, Max = 74.99, Label = "قابل قبول",     ColorHex = "#FBBF24" },
                new() { Min = 75, Max = 100,   Label = "قوی",           ColorHex = "#34D399" },
            }
        };

        /// <summary>
        /// یک آزمون نمونه با ۵ سؤال امتیازدار می‌سازد.
        /// اگر نوع درخواستی متنی باشد، سؤالات عددی به Likert تبدیل می‌شوند
        /// تا آزمون همیشه نمره قابل محاسبه داشته باشد.
        /// </summary>
        private static AssessmentDefinition Simple(string id, string title, string category, QuestionType type)
        {
            var def = new AssessmentDefinition
            {
                Id = id,
                Title = title,
                Category = category,
                EstimatedMinutes = 10,
                QuestionsPerPage = 2,
                Scoring = StandardScoring()
            };

            var numericType = QuestionTypeInfo.IsText(type) ? QuestionType.Likert : type;
            int min = numericType == QuestionType.Scale ? 0 : 1;
            int max = numericType == QuestionType.Scale ? 10 : 5;

            for (int i = 1; i <= 5; i++)
            {
                var q = new QuestionDefinition
                {
                    Id = $"q{i}",
                    Order = i,
                    Type = numericType,
                    Required = true,
                    Text = $"سؤال {i} از «{title}»",
                    MinLabel = "کم",
                    MaxLabel = "زیاد",
                    Weight = 1,
                    DimensionKey = "overall"
                };

                if (QuestionTypeInfo.IsNumeric(numericType))
                {
                    q.Min = min;
                    q.Max = max;
                }
                else if (QuestionTypeInfo.IsChoice(numericType))
                {
                    q.Options = new List<OptionDefinition>
                    {
                        new() { Id = "a", Text = "گزینه الف", Score = 1 },
                        new() { Id = "b", Text = "گزینه ب", Score = 2 },
                        new() { Id = "c", Text = "گزینه ج", Score = 3 },
                        new() { Id = "d", Text = "گزینه د", Score = 4 },
                    };
                }

                def.Questions.Add(q);
            }

            // سؤال متنی: امتیاز می‌گیرد ولی نمره کل را پایین نمی‌کشد
            def.Questions.Add(new QuestionDefinition
            {
                Id = "qText",
                Order = 6,
                Type = QuestionType.TextLong,
                Text = "توضیح تکمیلی خود را بنویسید",
                Required = false,
                CountsTowardTotal = false
            });

            return def;
        }

        private static AssessmentDefinition ManagerEval()
        {
            var def = new AssessmentDefinition
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
                    Mode = AssessmentScoringConfig.ModeDimensionWeighted,
                    NormalizeTo100 = true,
                    Dimensions = new List<ScoreDimension>
                    {
                        new() { Key = "leadership",    Title = "رهبری",   Weight = 0.6 },
                        new() { Key = "communication", Title = "ارتباطات", Weight = 0.4 },
                    },
                    Levels = new List<ScoreLevel>
                    {
                        new() { Min = 0,  Max = 49.99, Label = "نیاز به توسعه", ColorHex = "#FB7185" },
                        new() { Min = 50, Max = 74.99, Label = "قابل قبول",     ColorHex = "#FBBF24" },
                        new() { Min = 75, Max = 100,   Label = "قوی",           ColorHex = "#34D399" },
                    }
                }
            };

            def.Questions.Add(new QuestionDefinition
            {
                Id = "q1",
                Order = 1,
                Type = QuestionType.Likert,
                Text = "در تصمیم‌گیری‌های تیمی مشارکت فعال دارم",
                HelpText = "نزدیک‌ترین گزینه را انتخاب کنید",
                Required = true,
                Min = 1,
                Max = 5,
                MinLabel = "کاملاً مخالفم",
                MaxLabel = "کاملاً موافقم",
                Weight = 1,
                DimensionKey = "leadership"
            });

            def.Questions.Add(new QuestionDefinition
            {
                Id = "q2",
                Order = 2,
                Type = QuestionType.SingleChoice,
                Text = "سبک مدیریتی غالب شما کدام است؟",
                Required = true,
                Weight = 1,
                DimensionKey = "leadership",
                Options = new List<OptionDefinition>
                {
                    new() { Id = "a", Text = "دستوری",  Score = 1 },
                    new() { Id = "b", Text = "مشارکتی", Score = 3 },
                    new() { Id = "c", Text = "تفویضی",  Score = 2 },
                }
            });

            def.Questions.Add(new QuestionDefinition
            {
                Id = "q3",
                Order = 3,
                Type = QuestionType.Scale,
                Text = "توانایی اولویت‌بندی کارهای تیم را چقدر ارزیابی می‌کنید؟",
                Required = true,
                Min = 0,
                Max = 10,
                MinLabel = "ضعیف",
                MaxLabel = "عالی",
                Weight = 1.2,
                DimensionKey = "leadership"
            });

            // بعد ارتباطات باید سؤال عددی داشته باشد، وگرنه همیشه صفر می‌ماند
            def.Questions.Add(new QuestionDefinition
            {
                Id = "q4",
                Order = 4,
                Type = QuestionType.Likert,
                Text = "بازخورد سازنده را شفاف و بدون تنش منتقل می‌کنم",
                Required = true,
                Min = 1,
                Max = 5,
                MinLabel = "کاملاً مخالفم",
                MaxLabel = "کاملاً موافقم",
                Weight = 1,
                DimensionKey = "communication"
            });

            def.Questions.Add(new QuestionDefinition
            {
                Id = "q5",
                Order = 5,
                Type = QuestionType.Scale,
                Text = "کیفیت ارتباط نوشتاری تیم را چقدر ارزیابی می‌کنید؟",
                Required = true,
                Min = 0,
                Max = 10,
                MinLabel = "ضعیف",
                MaxLabel = "عالی",
                Weight = 1,
                DimensionKey = "communication"
            });

            def.Questions.Add(new QuestionDefinition
            {
                Id = "q6",
                Order = 6,
                Type = QuestionType.TextLong,
                Text = "یک موقعیت اخیر را شرح دهید که بازخورد سازنده دادید",
                Required = true,
                CountsTowardTotal = false,
                DimensionKey = "communication"
            });

            return def;
        }
    }
}