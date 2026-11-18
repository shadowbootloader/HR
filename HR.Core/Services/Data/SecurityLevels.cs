namespace HR.Services.Data
{
    /// <summary>رده حفاظتی روی ستون متنی Department.SecurityLevel ذخیره می‌شود، نه جدول lookup.</summary>
    public static class SecurityLevels
    {
        public static readonly string[] All =
        {
            "عادی",
            "محرمانه",
            "خیلی محرمانه",
            "فوق محرمانه",
            "سری",
            "بسیار سری"
        };
    }
}
