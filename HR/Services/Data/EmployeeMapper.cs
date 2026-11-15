using System;
using System.Linq;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data
{
    /// <summary>
    /// نگاشت بین مدل UI (EmployeeProfile) و Entityهای دیتابیس
    /// </summary>
    public static class EmployeeMapper
    {
        public static Employee ToEntity(EmployeeProfile profile, Guid? existingId = null)
        {
            var entity = new Employee
            {
                EmployeeId = existingId ?? Guid.NewGuid(),
                FirstName = profile.FirstName?.Trim() ?? string.Empty,
                LastName = profile.LastName?.Trim() ?? string.Empty,
                FatherName = profile.FatherName?.Trim() ?? string.Empty,
                NationalCode = profile.NationalCode?.Trim() ?? string.Empty,
                BirthCertificateNumber = profile.IdNumber?.Trim() ?? string.Empty,
                BirthDate = profile.BirthDate ?? DateTime.Today,
                IssueDate = profile.IdIssueDate ?? DateTime.Today,
                Email = $"{profile.NationalCode}@temp.local",
                ChildrenCount = (byte)Math.Clamp(profile.ChildrenCount ?? 0, 0, 255),
                InsuranceNumber = "0000000000",
                Description = null,
                IsDeleted = false,
                GenderId = MapGender(profile.Gender),
                MaritalStatusId = MapMaritalStatus(profile.MaritalStatus),
                MilitaryStatusId = MapMilitaryStatus(profile.MilitaryStatus)
            };

            // Supplement (اطلاعات شغلی)
            if (!string.IsNullOrWhiteSpace(profile.PersonnelCode) ||
                !string.IsNullOrWhiteSpace(profile.Position) ||
                profile.HireDate.HasValue)
            {
                entity.Supplement = new EmployeeSupplement
                {
                    EmployeeSupplementId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    PositionTitle = profile.Position?.Trim() ?? "—",
                    PersonnelCode = string.IsNullOrWhiteSpace(profile.PersonnelCode)
                        ? Guid.NewGuid().ToString("N")[..10]
                        : profile.PersonnelCode.Trim(),
                    StartDate = profile.HireDate ?? DateTime.Today,
                    EndDate = null,
                    Salary = 0,
                    InsuranceTypeId = 1,
                    CooperationTypeId = 1,
                    DepartmentId = 1 // TODO: از فرم انتخاب شود
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
                        DegreeId = MapDegree(edu.Degree),
                        StatusId = MapEducationStatus(edu.Status),
                        FieldOfStudy = edu.Field?.Trim() ?? "—",
                        InstitutionName = edu.University?.Trim() ?? "—",
                        GPA = (decimal)(edu.Gpa ?? 0),
                        StartDate = edu.StartDate ?? DateTime.Today,
                        EndDate = edu.EndDate,
                        ThesisTitle = edu.ThesisTitle,
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
                        BankId = 1, // TODO: نگاشت نام بانک به Id
                        AccountNumber = b.AccountNumber?.Trim() ?? string.Empty,
                        Sheba = (b.ShebaNumber ?? string.Empty).PadRight(26).Substring(0, 26),
                        CardNumber = (b.CardNumber ?? string.Empty).PadRight(16).Substring(0, 16),
                        AccountTypeId = MapAccountType(b.AccountType),
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
                        PhoneNumberType = MapPhoneKind(p.Kind),
                        PhoneNumberStatus = 1, // فعال
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
                        PhoneNumber = j.CompanyPhone?.Trim() ?? "00000000000",
                        StartDate = j.StartDate ?? DateTime.Today,
                        EndDate = j.EndDate,
                        Description = j.Note ?? j.LeaveReason ?? "—"
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
                        LanguageId = 1, // TODO: نگاشت نام زبان
                        ReadingLevelId = MapLanguageLevel(l.Reading),
                        WritingLevelId = MapLanguageLevel(l.Writing),
                        ComprehensionLevelId = MapLanguageLevel(l.Comprehension),
                        SpeakingLevelId = MapLanguageLevel(l.Speaking),
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
                        SoftwareId = 1, // TODO: نگاشت نام نرم‌افزار
                        SkillLevelId = MapSkillLevel(s.Level),
                        Description = s.Note
                    });
                }
            }

            return entity;
        }

        // ==================== Helper Mappings ====================

        private static byte MapGender(Models.Gender g) => g switch
        {
            Models.Gender.Male => 1,
            Models.Gender.Female => 2,
            _ => 1
        };

        private static byte MapMaritalStatus(Models.MaritalStatus m) => m switch
        {
            Models.MaritalStatus.Single => 1,
            Models.MaritalStatus.Married => 2,
            Models.MaritalStatus.Divorced => 3,
            Models.MaritalStatus.Widowed => 4,
            _ => 1
        };

        private static byte? MapMilitaryStatus(Models.MilitaryStatus m) => m switch
        {
            Models.MilitaryStatus.Done => 1,
            Models.MilitaryStatus.Exempt => 2,
            Models.MilitaryStatus.InProgress => 3,
            Models.MilitaryStatus.NotApplicable => 4,
            _ => null
        };

        private static byte MapDegree(DegreeLevel d) => d switch
        {
            DegreeLevel.Diploma => 1,
            DegreeLevel.Associate => 2,
            DegreeLevel.Bachelor => 3,
            DegreeLevel.Master => 4,
            DegreeLevel.Doctorate => 5,
            DegreeLevel.PostDoctorate => 6,
            _ => 3
        };

        private static byte MapEducationStatus(EducationStatusKind s) => s switch
        {
            EducationStatusKind.Graduated => 1,
            EducationStatusKind.Studying => 2,
            EducationStatusKind.DroppedOut => 3,
            EducationStatusKind.OnHold => 4,
            _ => 1
        };

        private static byte MapAccountType(BankAccountType t) => t switch
        {
            BankAccountType.Current => 1,
            BankAccountType.Savings => 2,
            BankAccountType.ShortTerm => 3,
            BankAccountType.LongTerm => 4,
            _ => 1
        };

        private static byte MapPhoneKind(PhoneKind k) => k switch
        {
            PhoneKind.Mobile => 1,
            PhoneKind.Home => 2,
            PhoneKind.Work => 3,
            PhoneKind.Emergency => 4,
            _ => 1
        };

        private static byte MapLanguageLevel(LanguageLevel l) => l switch
        {
            LanguageLevel.None => 1,
            LanguageLevel.Basic => 2,
            LanguageLevel.Intermediate => 3,
            LanguageLevel.Good => 4,
            LanguageLevel.Fluent => 5,
            LanguageLevel.Native => 6,
            _ => 1
        };

        private static byte MapSkillLevel(Models.SkillLevel s) => s switch
        {
            Models.SkillLevel.Weak => 1,
            Models.SkillLevel.Average => 2,
            Models.SkillLevel.Good => 3,
            Models.SkillLevel.Excellent => 4,
            _ => 2
        };

        private static int ParseDuration(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            var digits = new string(text.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out var h) ? h : 0;
        }
    }
}
