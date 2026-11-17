namespace HR.Services.Assessments
{
    /// <summary>
    /// تنها آزمون از پیش موجود. متن سؤال‌ها همان ارزیابی مدیر است و چیز دیگری ساخته نمی‌شود.
    /// </summary>
    public static class ManagerAssessmentSeed
    {
        public const string Code = "manager-eval";

        public static AssessmentRecord Create()
        {
            return new AssessmentRecord
            {
                Code = Code,
                Title = "ارزیابی مدیر",
                Category = "شغلی",
                Description = "ارزیابی مهارت‌های مدیریتی",
                EstimatedMinutes = 15,
                QuestionsPerPage = 2,
                ScoringMode = "dimensionWeighted",
                NormalizeTo100 = true,
                Dimensions =
                {
                    new DimensionRecord { Key = "leadership", Title = "رهبری", Weight = 0.6 },
                    new DimensionRecord { Key = "communication", Title = "ارتباطات", Weight = 0.4 }
                },
                Levels =
                {
                    new LevelRecord { MinScore = 0, MaxScore = 49.99, Label = "نیاز به توسعه", ColorHex = "#FB7185" },
                    new LevelRecord { MinScore = 50, MaxScore = 74.99, Label = "قابل قبول", ColorHex = "#FBBF24" },
                    new LevelRecord { MinScore = 75, MaxScore = 100, Label = "قوی", ColorHex = "#34D399" }
                },
                Questions =
                {
                    new QuestionRecord
                    {
                        Code = "q1", SortOrder = 1, QuestionType = "Likert",
                        Text = "در تصمیم‌گیری‌های تیمی مشارکت فعال دارم",
                        HelpText = "نزدیک‌ترین گزینه را انتخاب کنید",
                        IsRequired = true, MinValue = 1, MaxValue = 5,
                        MinLabel = "کاملاً مخالفم", MaxLabel = "کاملاً موافقم",
                        Weight = 1, DimensionKey = "leadership"
                    },
                    new QuestionRecord
                    {
                        Code = "q2", SortOrder = 2, QuestionType = "SingleChoice",
                        Text = "سبک مدیریتی غالب شما کدام است؟",
                        IsRequired = true, Weight = 1, DimensionKey = "leadership",
                        Options =
                        {
                            new OptionRecord { Code = "a", Text = "دستوری", Score = 1, SortOrder = 1 },
                            new OptionRecord { Code = "b", Text = "مشارکتی", Score = 3, SortOrder = 2 },
                            new OptionRecord { Code = "c", Text = "تفویضی", Score = 2, SortOrder = 3 }
                        }
                    },
                    new QuestionRecord
                    {
                        Code = "q3", SortOrder = 3, QuestionType = "Scale",
                        Text = "توانایی اولویت‌بندی کارهای تیم را چقدر ارزیابی می‌کنید؟",
                        IsRequired = true, MinValue = 0, MaxValue = 10,
                        MinLabel = "ضعیف", MaxLabel = "عالی",
                        Weight = 1.2, DimensionKey = "leadership"
                    },
                    new QuestionRecord
                    {
                        Code = "q4", SortOrder = 4, QuestionType = "Likert",
                        Text = "بازخورد سازنده را شفاف و بدون تنش منتقل می‌کنم",
                        IsRequired = true, MinValue = 1, MaxValue = 5,
                        MinLabel = "کاملاً مخالفم", MaxLabel = "کاملاً موافقم",
                        Weight = 1, DimensionKey = "communication"
                    },
                    new QuestionRecord
                    {
                        Code = "q5", SortOrder = 5, QuestionType = "Scale",
                        Text = "کیفیت ارتباط نوشتاری تیم را چقدر ارزیابی می‌کنید؟",
                        IsRequired = true, MinValue = 0, MaxValue = 10,
                        MinLabel = "ضعیف", MaxLabel = "عالی",
                        Weight = 1, DimensionKey = "communication"
                    },
                    new QuestionRecord
                    {
                        Code = "q6", SortOrder = 6, QuestionType = "TextLong",
                        Text = "یک موقعیت اخیر را شرح دهید که بازخورد سازنده دادید",
                        IsRequired = true, CountsTowardTotal = false, DimensionKey = "communication"
                    }
                }
            };
        }
    }
}
