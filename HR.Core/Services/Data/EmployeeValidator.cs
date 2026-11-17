using System.Net.Mail;
using HR.Data.Entities;

namespace HR.Services.Data
{
    /// <summary>
    /// قواعد اعتبارسنجی کارمند که به پایگاه داده نیاز ندارند (قابل تست واحد).
    /// حدود طول با پیکربندی ستون‌ها در <c>PersonnelDbContext</c> یکی است؛
    /// تا کاربر به‌جای خطای مبهم SQL ("String or binary data would be truncated") پیام فارسی ببیند.
    /// </summary>
    public static class EmployeeValidator
    {
        public const int NameMaxLength = 50;
        public const int BirthCertificateMaxLength = 10;
        public const int EmailMaxLength = 254;
        public const int InsuranceNumberMaxLength = 10;
        public const int DescriptionMaxLength = 250;
        public const int PositionTitleMaxLength = 100;
        public const int PersonnelCodeMaxLength = 20;

        /// <summary>اولین خطای یافت‌شده را برمی‌گرداند؛ اگر مشکلی نبود null.</summary>
        public static string? Validate(Employee employee, DateTime today)
        {
            if (employee == null) throw new ArgumentNullException(nameof(employee));

            if (Length(employee.FirstName) > NameMaxLength)
                return $"نام نباید بیش از {NameMaxLength} نویسه باشد.";
            if (Length(employee.LastName) > NameMaxLength)
                return $"نام خانوادگی نباید بیش از {NameMaxLength} نویسه باشد.";
            if (Length(employee.FatherName) > NameMaxLength)
                return $"نام پدر نباید بیش از {NameMaxLength} نویسه باشد.";
            if (Length(employee.BirthCertificateNumber) > BirthCertificateMaxLength)
                return $"شماره شناسنامه نباید بیش از {BirthCertificateMaxLength} رقم باشد.";
            if (Length(employee.InsuranceNumber) > InsuranceNumberMaxLength)
                return $"شماره بیمه نباید بیش از {InsuranceNumberMaxLength} رقم باشد.";
            if (Length(employee.Description) > DescriptionMaxLength)
                return $"توضیحات نباید بیش از {DescriptionMaxLength} نویسه باشد.";

            var email = employee.Email?.Trim() ?? string.Empty;
            if (email.Length == 0)
                return "ایمیل الزامی است.";
            if (email.Length > EmailMaxLength || !IsEmail(email))
                return "قالب ایمیل نامعتبر است.";

            if (employee.BirthDate.Year < 1900)
                return "تاریخ تولد نامعتبر است.";
            if (employee.BirthDate.Date > today.Date)
                return "تاریخ تولد نمی‌تواند در آینده باشد.";
            if (employee.IssueDate.Date > today.Date)
                return "تاریخ صدور شناسنامه نمی‌تواند در آینده باشد.";
            if (employee.IssueDate.Date < employee.BirthDate.Date)
                return "تاریخ صدور شناسنامه نمی‌تواند قبل از تاریخ تولد باشد.";

            var supplement = employee.Supplement;
            if (supplement != null)
            {
                if (string.IsNullOrWhiteSpace(supplement.PositionTitle))
                    return "سمت الزامی است.";
                if (supplement.PositionTitle.Length > PositionTitleMaxLength)
                    return $"سمت نباید بیش از {PositionTitleMaxLength} نویسه باشد.";
                if (Length(supplement.PersonnelCode) > PersonnelCodeMaxLength)
                    return $"کد پرسنلی نباید بیش از {PersonnelCodeMaxLength} نویسه باشد.";
                if (supplement.Salary < 0)
                    return "حقوق نمی‌تواند منفی باشد.";
                if (supplement.StartDate.Year < 1900)
                    return "تاریخ شروع همکاری نامعتبر است.";
                if (supplement.EndDate.HasValue && supplement.EndDate.Value.Date < supplement.StartDate.Date)
                    return "تاریخ پایان همکاری نمی‌تواند قبل از تاریخ شروع باشد.";
            }

            return null;
        }

        private static int Length(string? value) => value?.Length ?? 0;

        private static bool IsEmail(string value)
            => MailAddress.TryCreate(value, out var address) && address.Address == value;
    }
}
