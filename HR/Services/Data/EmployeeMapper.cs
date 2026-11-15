using System;
using System.Linq;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data
{
    /// <summary>
    /// نگاشت بین مدل UI (EmployeeProfile) و Entityهای دیتابیس.
    /// همه‌ی مقادیر Lookup از دیتابیس حل می‌شوند (بدون Id هاردکد).
    /// </summary>
    public static class EmployeeMapper
    {
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
                GenderId = await lookups.GenderAsync(profile.Gender),
                MaritalStatusId = await lookups.MaritalStatusAsync(profile.MaritalStatus),
                MilitaryStatusId = await lookups.MilitaryStatusAsync(profile.MilitaryStatus)
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
                    InsuranceTypeId = await lookups.InsuranceTypeAsync(supp?.InsuranceType),
                    CooperationTypeId = await lookups.CooperationTypeAsync(supp?.CooperationType),
                    DepartmentId = await lookups.DepartmentAsync(profile.DepartmentName)
                };
            }

            // تحصیلات
            if (profile.Educations?.Count > 0)
            {
                foreach (var edu in profile.Educations)
                {
                    entity.Educations.Add(new Education
                    {
                        EducationId = Guid.NewGuid(),
                        EmployeeId = entity.EmployeeId,
                        DegreeId = await lookups.DegreeAsync(edu.Degree),
                        StatusId = await lookups.EducationStatusAsync(edu.Status),
                        FieldOfStudy = edu.Field?.Trim() ?? "—",
                        InstitutionName = edu.University?.Trim() ?? "—",
                        GPA = (decimal)(edu.Gpa ?? 0),
                        StartDate = edu.StartDate ?? DateTime.Today,
                        EndDate = edu.EndDate,
                        ThesisTitle = edu.ThesisTitle,
                        EducationLocationNode = await lookups.EducationLocationAsync(edu.State, edu.City),
                        Description = null
                    });
                }
            }

            // دوره‌های آموزشی
            if (profile.Courses?.Count > 0)
            {
                foreach (var c in profile.Courses)
                {
                    entity.Courses.Add(new Course
                    {
                        CourseId = Guid.NewGuid(),
                        EmployeeId = entity.EmployeeId,
                        Title = c.Title?.Trim() ?? "—",
                        InstitutionName = c.Institute?.Trim() ?? "—",
                        DurationHours = ParseDuration(c.DurationText),
                        Description = c.Note
                    });
                }
            }

            // حساب‌های بانکی
            if (profile.BankAccounts?.Count > 0)
            {
                foreach (var b in profile.BankAccounts)
                {
                    entity.BankAccounts.Add(new BankAccount
                    {
                        BankAccountId = Guid.NewGuid(),
                        EmployeeId = entity.EmployeeId,
                        BankId = await lookups.BankAsync(b.BankName),
                        AccountNumber = b.AccountNumber?.Trim() ?? string.Empty,
                        Sheba = NormalizeSheba(b.ShebaNumber),
                        CardNumber = NormalizeCard(b.CardNumber),
                        AccountTypeId = await lookups.AccountTypeAsync(b.AccountType),
                        Description = null
                    });
                }
            }

            // تلفن‌ها
            if (profile.Phones?.Count > 0)
            {
                foreach (var p in profile.Phones)
                {
                    entity.PhoneNumbers.Add(new PhoneNumber
                    {
                        PhoneNumberId = Guid.NewGuid(),
                        EmployeeId = entity.EmployeeId,
                        Number = p.Number?.Trim() ?? string.Empty,
                        OwnerFirstName = profile.FirstName ?? "—",
                        OwnerLastName = profile.LastName ?? "—",
                        PhoneNumberType = await lookups.PhoneKindAsync(p.Kind),
                        PhoneNumberStatus = await lookups.PhoneStatusAsync("فعال"),
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        Description = p.Note
                    });
                }
            }

            // سابقه شغلی
            if (profile.JobHistories?.Count > 0)
            {
                foreach (var j in profile.JobHistories)
                {
                    entity.EmploymentHistories.Add(new EmploymentHistory
                    {
                        EmploymentHistoryId = Guid.NewGuid(),
                        EmployeeId = entity.EmployeeId,
                        WorkplaceName = j.Company?.Trim() ?? "—",
                        JobTitle = j.JobTitle?.Trim() ?? "—",
                        PhoneNumber = j.CompanyPhone?.Trim() ?? "—",
                        StartDate = j.StartDate ?? DateTime.Today,
                        EndDate = j.EndDate,
                        Description = j.Note ?? j.LeaveReason ?? string.Empty
                    });
                }
            }

            // زبان‌ها
            if (profile.Languages?.Count > 0)
            {
                foreach (var l in profile.Languages)
                {
                    entity.Languages.Add(new EmployeeLanguage
                    {
                        EmployeeLanguageId = Guid.NewGuid(),
                        EmployeeId = entity.EmployeeId,
                        LanguageId = await lookups.LanguageAsync(l.Language),
                        ReadingLevelId = await lookups.LanguageLevelAsync(l.Reading),
                        WritingLevelId = await lookups.LanguageLevelAsync(l.Writing),
                        ComprehensionLevelId = await lookups.LanguageLevelAsync(l.Comprehension),
                        SpeakingLevelId = await lookups.LanguageLevelAsync(l.Speaking),
                        Description = null
                    });
                }
            }

            // مهارت‌های نرم‌افزاری
            if (profile.SoftwareSkills?.Count > 0)
            {
                foreach (var s in profile.SoftwareSkills)
                {
                    entity.Softwares.Add(new EmployeeSoftware
                    {
                        EmployeeSoftwareId = Guid.NewGuid(),
                        EmployeeId = entity.EmployeeId,
                        SoftwareId = await lookups.SoftwareAsync(s.SoftwareName),
                        SkillLevelId = await lookups.SkillLevelAsync(s.Level),
                        Description = s.Note
                    });
                }
            }

            // تابعیت
            if (profile.Citizenships?.Count > 0)
            {
                foreach (var c in profile.Citizenships)
                {
                    entity.Citizenships.Add(new Citizenship
                    {
                        CitizenshipId = Guid.NewGuid(),
                        EmployeeId = entity.EmployeeId,
                        CountryId = await lookups.CountryAsync(c.Country),
                        CitizenshipReasonId = await lookups.CitizenshipReasonAsync(c.Reason),
                        StartDate = c.StartDate ?? DateTime.Today,
                        Description = c.Note
                    });
                }
            }

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

        private static string DigitsOnly(string? value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : new string(value.Where(char.IsDigit).ToArray());

        private static string NormalizeSheba(string? value)
        {
            var digits = DigitsOnly(value);
            if (digits.Length == 0) return string.Empty;
            if (digits.Length > 24) digits = digits[..24];
            return "IR" + digits.PadLeft(24, '0');
        }

        private static string NormalizeCard(string? value)
        {
            var digits = DigitsOnly(value);
            if (digits.Length == 0) return string.Empty;
            if (digits.Length > 16) digits = digits[..16];
            return digits.PadLeft(16, '0');
        }

        private static int ParseDuration(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            var digits = new string(text.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out var h) ? h : 0;
        }
    }
}