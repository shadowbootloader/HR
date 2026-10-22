namespace HR.Models
{
    /// <summary>
    /// هر بخش از فرم «افزودن کارمند» این قرارداد را پیاده می‌کند
    /// تا صفحه‌ی میزبان بتواند همه‌ی بخش‌ها را یکجا ذخیره و اعتبارسنجی کند.
    /// </summary>
    public interface IEmployeeSection
    {
        /// <summary>عنوان بخش، برای پیام‌های خطا.</summary>
        string SectionTitle { get; }

        /// <summary>بارگذاری مقادیر پرونده در فرم.</summary>
        void LoadFrom(EmployeeProfile profile);

        /// <summary>نوشتن مقادیر فرم در پرونده.</summary>
        void SaveTo(EmployeeProfile profile);

        /// <summary>اعتبارسنجی؛ در صورت خطا false و پیام را پر می‌کند.</summary>
        bool Validate(out string message);
    }
}