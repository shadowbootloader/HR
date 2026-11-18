namespace HR.Services.Assessments
{
    /// <summary>
    /// فرم‌های کوتاه شخصیتی با گویه‌های بازنویسی‌شده (الگوی IPIP برای پنج عامل بزرگ، بدون متن ابزارهای تجاری).
    /// </summary>
    public static class PersonalityAssessmentCatalog
    {
        public static IReadOnlyList<AssessmentRecord> All() => new[]
        {
            Likert("big-five-short", "پنج عامل بزرگ (کوتاه)", "شخصیتی", "فرم کوتاه بر اساس گویه‌های عمومی IPIP",
                new[] { ("E", "برون‌گرایی"), ("A", "سازگاری"), ("C", "وظیفه‌شناسی"), ("N", "روان‌رنجوری"), ("O", "گشودگی") },
                new[]
                {
                    ("E", "در جمع به‌راحتی گفتگو را شروع می‌کنم"),
                    ("E", "بودن کنار دیگران به من انرژی می‌دهد"),
                    ("A", "به احساس دیگران توجه می‌کنم"),
                    ("A", "در اختلاف، دنبال راه‌حل منصفانه می‌گردم"),
                    ("C", "کارها را تا پایان منظم پیش می‌برم"),
                    ("C", "به قول‌های زمانی خود پایبندم"),
                    ("N", "در فشار، زود مضطرب می‌شوم"),
                    ("N", "نگرانی را سخت از ذهنم بیرون می‌کنم"),
                    ("O", "ایده‌های تازه برایم جذاب است"),
                    ("O", "از تجربه راه‌های ناآشنا استقبال می‌کنم")
                }),
            Choice("mbti-style", "ترجیح‌های شخصیتی", "شخصیتی", "چهار دوگانگی؛ نتیجه کد تیپ نیست و فقط قطب غالب را نشان می‌دهد",
                new[] { ("EI", "انرژی"), ("SN", "اطلاعات"), ("TF", "تصمیم"), ("JP", "سبک زندگی") },
                new (string Key, string Text, string A, string B)[]
                {
                    ("EI", "انرژی خود را بیشتر از کجا می‌گیرید؟", "از تنهایی و تمرکز", "از معاشرت و گفتگو"),
                    ("SN", "به چه نوع اطلاعات بیشتر تکیه می‌کنید؟", "واقعیت‌های ملموس", "الگو و امکان‌های آینده"),
                    ("TF", "در تصمیم مهم کدام وزن بیشتری دارد؟", "منطق و معیار", "اثر تصمیم بر افراد"),
                    ("JP", "برنامه‌ریزی روز را چطور دوست دارید؟", "فهرست و موعد مشخص", "انعطاف و تصمیم لحظه‌ای")
                }),
            Likert("disc-short", "سبک رفتاری DISC", "شخصیتی", "فرم کوتاه چهار سبک رفتاری",
                new[] { ("D", "سلطه‌گری"), ("I", "تأثیرگذاری"), ("S", "ثبات"), ("C", "دقت") },
                new[]
                {
                    ("D", "در بحث، سریع موضع می‌گیرم و نتیجه را جلو می‌برم"),
                    ("I", "با اشتیاق دیگران را همراه می‌کنم"),
                    ("S", "ثبات و هماهنگی گروه برایم مهم است"),
                    ("C", "قبل از اقدام، جزئیات و کیفیت را چک می‌کنم")
                }),
            Likert("riasec-short", "علاقه شغلی هالند", "رغبت شغلی", "شش تیپ علاقه؛ نمره هر تیپ جداست",
                new[] { ("R", "واقع‌گرا"), ("I", "جستجوگر"), ("A", "هنری"), ("S", "اجتماعی"), ("E", "متهور"), ("C", "قراردادی") },
                new[]
                {
                    ("R", "کار با ابزار و ساختن چیز ملموس را دوست دارم"),
                    ("I", "تحلیل مسئله و پیدا کردن علت برایم جذاب است"),
                    ("A", "خلق کردن و بیان شخصی به من انگیزه می‌دهد"),
                    ("S", "کمک به یادگیری یا حال دیگران را ترجیح می‌دهم"),
                    ("E", "متقاعد کردن و راهبری یک کار را دوست دارم"),
                    ("C", "نظم، داده و رویه مشخص برایم راحت است")
                }),
            Likert("eq-short", "هوش هیجانی (کوتاه)", "شخصیتی", "فرم کوتاه خودسنجی هیجان",
                new[] { ("EQ", "هوش هیجانی") },
                new[]
                {
                    ("EQ", "هیجان خود را همان لحظه درست نام می‌برم"),
                    ("EQ", "وقتی ناراحتم، واکنش را به تعویق می‌اندازم"),
                    ("EQ", "حال دیگران را از لحن و رفتارشان می‌فهمم"),
                    ("EQ", "در تنش، رابطه را بدون تحقیر نگه می‌دارم")
                })
        };

        private static AssessmentRecord Likert(string code, string title, string category, string description,
            (string Key, string Title)[] dimensions, (string Key, string Text)[] questions)
        {
            var record = Shell(code, title, category, description, dimensions);
            var order = 1;
            foreach (var question in questions)
            {
                record.Questions.Add(new QuestionRecord
                {
                    Code = "q" + order,
                    SortOrder = order,
                    QuestionType = "Likert",
                    Text = question.Text,
                    IsRequired = true,
                    CountsTowardTotal = true,
                    Weight = 1,
                    MinValue = 1,
                    MaxValue = 5,
                    MinLabel = "کاملاً مخالفم",
                    MaxLabel = "کاملاً موافقم",
                    DimensionKey = question.Key
                });
                order++;
            }
            return record;
        }

        private static AssessmentRecord Choice(string code, string title, string category, string description,
            (string Key, string Title)[] dimensions, (string Key, string Text, string A, string B)[] questions)
        {
            var record = Shell(code, title, category, description, dimensions);
            var order = 1;
            foreach (var question in questions)
            {
                record.Questions.Add(new QuestionRecord
                {
                    Code = "q" + order,
                    SortOrder = order,
                    QuestionType = "SingleChoice",
                    Text = question.Text,
                    IsRequired = true,
                    CountsTowardTotal = true,
                    Weight = 1,
                    DimensionKey = question.Key,
                    Options =
                    {
                        new OptionRecord { Code = "a", Text = question.A, Score = 20, SortOrder = 1 },
                        new OptionRecord { Code = "b", Text = question.B, Score = 80, SortOrder = 2 }
                    }
                });
                order++;
            }
            return record;
        }

        private static AssessmentRecord Shell(string code, string title, string category, string description, (string Key, string Title)[] dimensions)
        {
            var record = new AssessmentRecord
            {
                Code = code,
                Title = title,
                Category = category,
                Description = description,
                ScoringMode = "dimensionWeighted",
                NormalizeTo100 = true,
                EstimatedMinutes = 8,
                QuestionsPerPage = 2
            };
            foreach (var dimension in dimensions)
                record.Dimensions.Add(new DimensionRecord { Key = dimension.Key, Title = dimension.Title, Weight = 1 });
            record.Levels.Add(new LevelRecord { MinScore = 0, MaxScore = 39.99, Label = "پایین", ColorHex = "#FB7185" });
            record.Levels.Add(new LevelRecord { MinScore = 40, MaxScore = 69.99, Label = "متوسط", ColorHex = "#FBBF24" });
            record.Levels.Add(new LevelRecord { MinScore = 70, MaxScore = 100, Label = "بالا", ColorHex = "#34D399" });
            return record;
        }
    }

    public static class AttemptStatus
    {
        public static string Of(AttemptSummary? latest)
        {
            if (latest == null || latest.SubmittedAt == null && !string.Equals(latest.Status, "InProgress", StringComparison.OrdinalIgnoreCase))
                return "none";
            if (latest.SubmittedAt == null)
                return "in-progress";
            return "completed";
        }
    }
}
