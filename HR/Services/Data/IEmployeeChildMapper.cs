using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data
{
    /// <summary>
    /// مپر یک مجموعه‌ی فرزند (تحصیلات، حساب بانکی، سفر و…).
    /// با افزودن یک پیاده‌سازی جدید، بدون تغییر EmployeeMapper به چرخه‌ی ذخیره اضافه می‌شود.
    /// </summary>
    public interface IEmployeeChildMapper
    {
        Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups);
    }
}