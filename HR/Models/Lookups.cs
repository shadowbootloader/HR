namespace HR.Models
{
    /// <summary>
    /// فقط موقعیت‌های سازمانی (ستون متنی بدون جدول Lookup).
    /// بقیه Lookupها از پایگاه داده خوانده می‌شوند.
    /// </summary>
    public static class Lookups
    {
        public static readonly string[] Positions =
        {
            "کارشناس", "کارشناس ارشد", "مدیر", "معاون", "رئیس", "کارمند", "سایر"
        };
    }
}
