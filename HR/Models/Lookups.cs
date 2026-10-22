namespace HR.Models
{
    /// <summary>لیست‌های ثابت مشترک بین فرم‌ها.</summary>
    public static class Lookups
    {
        public static readonly string[] Countries =
        {
            "ایران", "ترکیه", "عراق", "امارات", "قطر", "عربستان", "عمان", "کویت",
            "آلمان", "فرانسه", "انگلیس", "ایتالیا", "اسپانیا", "هلند", "سوئد",
            "کانادا", "آمریکا", "استرالیا", "ژاپن", "چین", "کره جنوبی",
            "مالزی", "روسیه", "اتریش", "سوئیس", "بلژیک", "دانمارک", "نروژ", "سایر"
        };

        public static readonly string[] InstitutionTypes =
        {
            "دولتی", "خصوصی", "دانشگاهی", "تحقیقاتی", "سازمان مردم‌نهاد", "بین‌المللی", "سایر"
        };

        public static readonly string[] TransportModes =
        {
            "هوایی", "زمینی", "دریایی", "ریلی", "سایر"
        };

        public static readonly string[] SacrificeTypes =
        {
            "آزاده", "جانباز", "رزمنده", "خانواده شهید", "فرزند جانباز", "سایر"
        };

        public static readonly string[] SacrificeAmounts =
        {
            "کمتر از ۶ ماه", "۶ تا ۱۲ ماه", "۱ تا ۳ سال", "بیش از ۳ سال"
        };

        public static readonly string[] InsuranceTypes =
        {
            "تأمین اجتماعی", "بیمه تکمیلی", "بیمه آزاد", "بدون بیمه"
        };

        public static readonly string[] CooperationTypes =
        {
            "تمام‌وقت", "پاره‌وقت", "پیمانکاری", "مشاوره‌ای", "کارآموزی"
        };

        public static readonly string[] Positions =
        {
            "مدیر", "معاون", "سرپرست", "کارشناس ارشد", "کارشناس", "کارمند", "کارآموز", "سایر"
        };

        public static readonly string[] CriminalRecordTypes =
        {
            "کیفری", "سیاسی", "انتظامی", "مالی", "سایر"
        };
    }
}