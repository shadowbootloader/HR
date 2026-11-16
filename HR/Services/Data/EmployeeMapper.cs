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
                BirthCertificateNumber = string.IsNullOrWhiteSpace(profile.IdNumber)
                    ? "—"
                    : profile.IdNumber.Trim(),
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

            var hasJobInfo =
                !string.IsNullOrWhiteSpace(profile.PersonnelCode) ||
                !string.IsNullOrWhiteSpace(profile.Position) ||
                !string.IsNullOrWhiteSpace(profile.DepartmentName) ||
                profile.HireDate.HasValue ||
                supp != null && (
                    !string.IsNullOrWhiteSpace(supp.Position) ||
                    !string.IsNullOrWhiteSpace(supp.InsuranceType) ||
                    !string.IsNullOrWhiteSpace(supp.CooperationType) ||
                    supp.StartDate.HasValue ||
                    supp.Salary > 0);

            if (hasJobInfo)
            {
                var departmentId = await lookups.IdFromTitleAsync<Department>(profile.DepartmentName);

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
                    DepartmentId = departmentId
                };
            }

            foreach (var mapper in ChildMappers)
                await mapper.MapAsync(profile, entity, lookups);

            if (entity.Supplement != null && entity.BankAccounts.Count > 0)
                entity.Supplement.BankAccountId = entity.BankAccounts.First().BankAccountId;

            return entity;
        }


        /// <summary>
        /// نگاشت معکوس Entity → EmployeeProfile برای بارگذاری فرم ویرایش.
        /// </summary>
        public static EmployeeProfile ToProfile(Employee entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var profile = new EmployeeProfile
            {
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                FatherName = entity.FatherName,
                NationalCode = entity.NationalCode,
                IdNumber = entity.BirthCertificateNumber,
                BirthDate = entity.BirthDate,
                IdIssueDate = entity.IssueDate,
                ChildrenCount = entity.ChildrenCount,
                Gender = MapGenderBack(entity.GenderId, entity.Gender?.Title),
                MaritalStatus = MapMaritalBack(entity.MaritalStatusId, entity.MaritalStatus?.Title),
                MilitaryStatus = MapMilitaryBack(entity.MilitaryStatusId, entity.MilitaryStatus?.Title),
                PersonnelCode = entity.Supplement?.PersonnelCode ?? string.Empty,
                Position = entity.Supplement?.PositionTitle,
                DepartmentName = entity.Supplement?.Department?.Title,
                HireDate = entity.Supplement?.StartDate,
                Status = entity.IsDeleted ? EmployeeStatus.Terminated : EmployeeStatus.Active
            };

            if (entity.Supplement != null)
            {
                profile.Supplementary = new SupplementaryInfo
                {
                    Position = entity.Supplement.PositionTitle,
                    StartDate = entity.Supplement.StartDate,
                    EndDate = entity.Supplement.EndDate,
                    Salary = entity.Supplement.Salary,
                    InsuranceType = entity.Supplement.InsuranceType?.Title,
                    CooperationType = entity.Supplement.CooperationType?.Title
                };
            }

            foreach (var edu in entity.Educations ?? Enumerable.Empty<Education>())
            {
                profile.Educations.Add(new EducationRecord
                {
                    Degree = MapDegreeBack(edu.DegreeId, edu.Degree?.Title),
                    Status = MapEduStatusBack(edu.StatusId, edu.Status?.Title),
                    Field = edu.FieldOfStudy,
                    University = edu.InstitutionName,
                    Gpa = (double)edu.GPA,
                    StartDate = edu.StartDate,
                    EndDate = edu.EndDate,
                    ThesisTitle = edu.ThesisTitle
                });
            }

            foreach (var c in entity.Courses ?? Enumerable.Empty<Course>())
            {
                profile.Courses.Add(new TrainingCourseRecord
                {
                    Title = c.Title,
                    Institute = c.InstitutionName,
                    DurationText = c.DurationHours.ToString(),
                    Note = c.Description
                });
            }

            foreach (var b in entity.BankAccounts ?? Enumerable.Empty<BankAccount>())
            {
                profile.BankAccounts.Add(new BankAccountRecord
                {
                    BankName = b.Bank?.Title ?? string.Empty,
                    AccountNumber = b.AccountNumber,
                    ShebaNumber = b.Sheba,
                    CardNumber = b.CardNumber,
                    AccountType = MapAccountTypeBack(b.AccountTypeId, b.AccountType?.Title)
                });
            }

            foreach (var p in entity.PhoneNumbers ?? Enumerable.Empty<PhoneNumber>())
            {
                profile.Phones.Add(new PhoneRecord
                {
                    Number = p.Number,
                    Kind = MapPhoneKindBack(p.PhoneNumberType, p.Type?.Title),
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Note = p.Description
                });
            }

            foreach (var r in entity.RelatedPersons ?? Enumerable.Empty<RelatedPerson>())
            {
                profile.Family.Add(new FamilyMemberRecord
                {
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    FatherName = r.FatherName,
                    NationalCode = r.NationalCode,
                    Relation = r.Relationship,
                    Note = r.JobTitle
                });
            }

            foreach (var l in entity.Languages ?? Enumerable.Empty<EmployeeLanguage>())
            {
                profile.Languages.Add(new LanguageRecord
                {
                    Language = l.Language?.Title ?? string.Empty,
                    Reading = MapLangLevelBack(l.ReadingLevelId, l.ReadingLevel?.Title),
                    Writing = MapLangLevelBack(l.WritingLevelId, l.WritingLevel?.Title),
                    Comprehension = MapLangLevelBack(l.ComprehensionLevelId, l.ComprehensionLevel?.Title),
                    Speaking = MapLangLevelBack(l.SpeakingLevelId, l.SpeakingLevel?.Title)
                });
            }

            foreach (var s in entity.Softwares ?? Enumerable.Empty<EmployeeSoftware>())
            {
                profile.SoftwareSkills.Add(new SoftwareSkillRecord
                {
                    SoftwareName = s.Software?.Title ?? string.Empty,
                    Level = MapSkillBack(s.SkillLevelId, s.SkillLevel?.Title),
                    Note = s.Description
                });
            }

            foreach (var j in entity.EmploymentHistories ?? Enumerable.Empty<EmploymentHistory>())
            {
                profile.JobHistories.Add(new JobHistoryRecord
                {
                    Company = j.WorkplaceName,
                    JobTitle = j.JobTitle,
                    CompanyPhone = j.PhoneNumber,
                    StartDate = j.StartDate,
                    EndDate = j.EndDate,
                    LeaveReason = j.EndReason?.Title,
                    Note = j.Description
                });
            }

            return profile;
        }

        private static HR.Models.Gender MapGenderBack(byte id, string? title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                if (title.Contains("زن") || title.Contains("خانم")) return HR.Models.Gender.Female;
                if (title.Contains("مرد") || title.Contains("آقا")) return HR.Models.Gender.Male;
            }
            return id switch { 2 => HR.Models.Gender.Female, 1 => HR.Models.Gender.Male, _ => HR.Models.Gender.Unspecified };
        }

        private static HR.Models.MaritalStatus MapMaritalBack(byte id, string? title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                if (title.Contains("مجرد")) return HR.Models.MaritalStatus.Single;
                if (title.Contains("متاهل") || title.Contains("متأهل")) return HR.Models.MaritalStatus.Married;
                if (title.Contains("مطلقه") || title.Contains("طلاق")) return HR.Models.MaritalStatus.Divorced;
                if (title.Contains("همسر فوت")) return HR.Models.MaritalStatus.Widowed;
            }
            return id switch
            {
                1 => HR.Models.MaritalStatus.Single,
                2 => HR.Models.MaritalStatus.Married,
                3 => HR.Models.MaritalStatus.Divorced,
                4 => HR.Models.MaritalStatus.Widowed,
                _ => HR.Models.MaritalStatus.Unspecified
            };
        }

        private static HR.Models.MilitaryStatus MapMilitaryBack(byte? id, string? title)
        {
            if (id == null) return HR.Models.MilitaryStatus.Unspecified;
            if (!string.IsNullOrWhiteSpace(title))
            {
                if (title.Contains("پایان") || title.Contains("کارت پایان")) return HR.Models.MilitaryStatus.Done;
                if (title.Contains("معاف")) return HR.Models.MilitaryStatus.Exempt;
                if (title.Contains("در حال")) return HR.Models.MilitaryStatus.InProgress;
                if (title.Contains("مشمول نیست") || title.Contains("ندارد")) return HR.Models.MilitaryStatus.NotApplicable;
            }
            return id switch
            {
                1 => HR.Models.MilitaryStatus.Done,
                2 => HR.Models.MilitaryStatus.Exempt,
                3 => HR.Models.MilitaryStatus.InProgress,
                4 => HR.Models.MilitaryStatus.NotApplicable,
                _ => HR.Models.MilitaryStatus.Unspecified
            };
        }

        private static DegreeLevel MapDegreeBack(byte id, string? title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                if (title.Contains("دیپلم") && !title.Contains("فوق")) return DegreeLevel.Diploma;
                if (title.Contains("کاردانی") || title.Contains("فوق دیپلم")) return DegreeLevel.Associate;
                if (title.Contains("کارشناسی ارشد") || title.Contains("ارشد")) return DegreeLevel.Master;
                if (title.Contains("دکتری") || title.Contains("دکترا")) return DegreeLevel.Doctorate;
                if (title.Contains("پسادکتری") || title.Contains("پس‌دکتری")) return DegreeLevel.PostDoctorate;
                if (title.Contains("کارشناسی")) return DegreeLevel.Bachelor;
            }
            return id switch
            {
                1 => DegreeLevel.Diploma,
                2 => DegreeLevel.Associate,
                3 => DegreeLevel.Bachelor,
                4 => DegreeLevel.Master,
                5 => DegreeLevel.Doctorate,
                6 => DegreeLevel.PostDoctorate,
                _ => DegreeLevel.Bachelor
            };
        }

        private static EducationStatusKind MapEduStatusBack(byte id, string? title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                if (title.Contains("فارغ")) return EducationStatusKind.Graduated;
                if (title.Contains("تحصیل")) return EducationStatusKind.Studying;
                if (title.Contains("انصراف")) return EducationStatusKind.DroppedOut;
                if (title.Contains("مرخصی")) return EducationStatusKind.OnHold;
            }
            return id switch
            {
                1 => EducationStatusKind.Graduated,
                2 => EducationStatusKind.Studying,
                3 => EducationStatusKind.DroppedOut,
                4 => EducationStatusKind.OnHold,
                _ => EducationStatusKind.Graduated
            };
        }

        private static BankAccountType MapAccountTypeBack(byte id, string? title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                if (title.Contains("جاری")) return BankAccountType.Current;
                if (title.Contains("پس‌انداز") || title.Contains("پس انداز")) return BankAccountType.Savings;
                if (title.Contains("کوتاه")) return BankAccountType.ShortTerm;
                if (title.Contains("بلند")) return BankAccountType.LongTerm;
            }
            return id switch
            {
                1 => BankAccountType.Current,
                2 => BankAccountType.Savings,
                3 => BankAccountType.ShortTerm,
                4 => BankAccountType.LongTerm,
                _ => BankAccountType.Current
            };
        }

        private static PhoneKind MapPhoneKindBack(byte id, string? title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                if (title.Contains("همراه") || title.Contains("موبایل")) return PhoneKind.Mobile;
                if (title.Contains("منزل") || title.Contains("خانه")) return PhoneKind.Home;
                if (title.Contains("کار")) return PhoneKind.Work;
                if (title.Contains("اضطرار")) return PhoneKind.Emergency;
            }
            return id switch
            {
                1 => PhoneKind.Mobile,
                2 => PhoneKind.Home,
                3 => PhoneKind.Work,
                4 => PhoneKind.Emergency,
                _ => PhoneKind.Mobile
            };
        }

        private static LanguageLevel MapLangLevelBack(byte id, string? title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                if (title.Contains("مادری") || title.Contains("native")) return LanguageLevel.Native;
                if (title.Contains("روان") || title.Contains("عالی")) return LanguageLevel.Fluent;
                if (title.Contains("خوب")) return LanguageLevel.Good;
                if (title.Contains("متوسط")) return LanguageLevel.Intermediate;
                if (title.Contains("مبتدی") || title.Contains("ضعیف")) return LanguageLevel.Basic;
            }
            return id switch
            {
                1 => LanguageLevel.None,
                2 => LanguageLevel.Basic,
                3 => LanguageLevel.Intermediate,
                4 => LanguageLevel.Good,
                5 => LanguageLevel.Fluent,
                6 => LanguageLevel.Native,
                _ => LanguageLevel.Basic
            };
        }

        private static HR.Models.SkillLevel MapSkillBack(byte id, string? title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                if (title.Contains("ضعیف")) return HR.Models.SkillLevel.Weak;
                if (title.Contains("متوسط")) return HR.Models.SkillLevel.Average;
                if (title.Contains("خوب")) return HR.Models.SkillLevel.Good;
                if (title.Contains("عالی") || title.Contains("ممتاز")) return HR.Models.SkillLevel.Excellent;
            }
            return id switch
            {
                1 => HR.Models.SkillLevel.Weak,
                2 => HR.Models.SkillLevel.Average,
                3 => HR.Models.SkillLevel.Good,
                4 => HR.Models.SkillLevel.Excellent,
                _ => HR.Models.SkillLevel.Average
            };
        }


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
