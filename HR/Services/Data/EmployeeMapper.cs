using System;
using System.Linq;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data
{
    /// <summary>
    /// نگاشت هسته‌ی Employee و اطلاعات شغلی.
    /// مجموعه‌های فرزند از طریق <see cref="IEmployeeChildMapper"/>های کشف‌شده نگاشت می‌شوند.
    /// </summary>
    public static class EmployeeMapper
    {
        private static readonly IEmployeeChildMapper[] ChildMappers = DiscoverChildMappers();

        private static IEmployeeChildMapper[] DiscoverChildMappers()
            => typeof(IEmployeeChildMapper).Assembly.GetTypes()
                .Where(t => typeof(IEmployeeChildMapper).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .OrderBy(t => t.Name, StringComparer.Ordinal)
                .Select(t => (IEmployeeChildMapper)Activator.CreateInstance(t)!)
                .ToArray();

        public static async Task<Employee> ToEntityAsync(
            EmployeeProfile profile, LookupResolver lookups, Guid? existingId = null)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (lookups == null) throw new ArgumentNullException(nameof(lookups));

            var nationalCode = profile.NationalCode?.Trim() ?? string.Empty;

            var entity = new Employee
            {
                EmployeeId = existingId ?? Guid.NewGuid(),
                FirstName = profile.FirstName?.Trim() ?? string.Empty,
                LastName = profile.LastName?.Trim() ?? string.Empty,
                FatherName = profile.FatherName?.Trim() ?? string.Empty,
                NationalCode = nationalCode,
                BirthCertificateNumber = profile.IdNumber?.Trim() ?? string.Empty,
                BirthDate = profile.BirthDate ?? DateTime.Today,
                IssueDate = profile.IdIssueDate ?? DateTime.Today,
                Email = BuildEmail(nationalCode),
                ChildrenCount = (byte)Math.Clamp(profile.ChildrenCount ?? 0, 0, 255),
                InsuranceNumber = BuildInsuranceNumber(),
                Description = null,
                IsDeleted = false,
                GenderId = await lookups.FromEnumAsync<HR.Data.Entities.Gender, HR.Models.Gender>(profile.Gender, offset: 1),
                MaritalStatusId = await lookups.FromEnumAsync<HR.Data.Entities.MaritalStatus, HR.Models.MaritalStatus>(profile.MaritalStatus, offset: 1),
                MilitaryStatusId = profile.MilitaryStatus == HR.Models.MilitaryStatus.Unspecified
                    ? null
                    : await lookups.FromEnumAsync<HR.Data.Entities.MilitaryStatus, HR.Models.MilitaryStatus>(profile.MilitaryStatus, offset: 1)
            };

            var supp = profile.Supplementary;

            // اطلاعات شغلی
            if (!string.IsNullOrWhiteSpace(profile.PersonnelCode) ||
                !string.IsNullOrWhiteSpace(profile.Position) ||
                !string.IsNullOrWhiteSpace(profile.DepartmentName) ||
                profile.HireDate.HasValue)
            {
                entity.Supplement = new EmployeeSupplement
                {
                    EmployeeSupplementId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    PositionTitle = (profile.Position ?? supp?.Position)?.Trim() ?? "—",
                    PersonnelCode = string.IsNullOrWhiteSpace(profile.PersonnelCode)
                        ? $"P{Guid.NewGuid():N}"[..10]
                        : profile.PersonnelCode.Trim(),
                    StartDate = supp?.StartDate ?? profile.HireDate ?? DateTime.Today,
                    EndDate = supp?.EndDate,
                    Salary = supp?.Salary ?? 0,
                    InsuranceTypeId = await lookups.ByteFromTitleAsync<InsuranceType>(supp?.InsuranceType),
                    CooperationTypeId = await lookups.ByteFromTitleAsync<CooperationType>(supp?.CooperationType),
                    DepartmentId = await lookups.IdFromTitleAsync<Department>(profile.DepartmentName)
                };
            }

            // مجموعه‌های فرزند
            foreach (var mapper in ChildMappers)
                await mapper.MapAsync(profile, entity, lookups);

            // اتصال حساب بانکی اصلی به اطلاعات شغلی
            if (entity.Supplement != null && entity.BankAccounts.Count > 0)
                entity.Supplement.BankAccountId = entity.BankAccounts.First().BankAccountId;

            return entity;
        }

        // ==================== کمکی‌ها ====================

        private static string BuildEmail(string nationalCode)
            => string.IsNullOrWhiteSpace(nationalCode)
                ? $"{Guid.NewGuid():N}@personnel.local"
                : $"{nationalCode}@personnel.local";

        private static string BuildInsuranceNumber()
        {
            var digits = new string(Guid.NewGuid().ToByteArray()
                .Take(6)
                .Select(b => (char)('0' + (b % 10)))
                .ToArray());

            return digits.PadRight(10, '0')[..10];
        }
    }
}