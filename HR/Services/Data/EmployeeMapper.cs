using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;
using HR.Services.Data.Mapping;

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
                BirthCertificateNumber = MapperHelpers.Required(profile.IdNumber, "شماره شناسنامه"),
                BirthDate = MapperHelpers.RequireDate(profile.BirthDate, "تاریخ تولد"),
                IssueDate = MapperHelpers.RequireDate(profile.IdIssueDate, "تاریخ صدور"),
                Email = MapperHelpers.Required(profile.Email, "ایمیل"),
                ChildrenCount = (byte)Math.Clamp(profile.ChildrenCount ?? 0, 0, 255),
                InsuranceNumber = MapperHelpers.Required(profile.InsuranceNumber, "شماره بیمه"),
                Description = null,
                IsDeleted = false,
                GenderId = lookups.MatchTitle<HR.Data.Entities.Gender>(
                    profile.Gender == HR.Models.Gender.Female ? new[] { "زن", "خانم" } : new[] { "مرد", "آقا" },
                    "جنسیت"),
                MaritalStatusId = await lookups.ByteFromTitleAsync<HR.Data.Entities.MaritalStatus>(profile.MaritalStatusTitle),
                MilitaryStatusId = await lookups.ByteFromTitleOrNullAsync<HR.Data.Entities.MilitaryStatus>(profile.MilitaryStatusTitle),
                ExemptionTypeId = await lookups.ByteFromTitleOrNullAsync<ExemptionType>(profile.ExemptionType),
                ExemptionReasonId = await lookups.ByteFromTitleOrNullAsync<ExemptionReason>(profile.ExemptionReason),
                ReligionId = await lookups.ByteFromTitleOrNullAsync<Religion>(profile.Religion),
                DenominationId = await lookups.ByteFromTitleOrNullAsync<Denomination>(profile.Denomination),
                BirthNode = await lookups.LocationByNameAsync(profile.BirthPlace),
                IssueNode = await lookups.LocationByNameAsync(profile.IdIssuePlace),
                ResidenceNode = await lookups.LocationByNameAsync(profile.Residence)
            };

            if (!string.IsNullOrWhiteSpace(profile.AddressText))
            {
                entity.Addresses.Add(new Address
                {
                    AddressId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    Title = MapperHelpers.Required(profile.AddressTitle, "عنوان نشانی"),
                    AddressText = profile.AddressText.Trim(),
                    PostalCode = string.IsNullOrWhiteSpace(profile.PostalCode) ? null : profile.PostalCode.Trim()
                });
            }

            var supp = profile.Supplementary;

            var hasJobInfo =
                Filled(profile.PersonnelCode) ||
                Filled(profile.Position) ||
                Filled(profile.DepartmentName) ||
                profile.HireDate.HasValue ||
                supp != null && (
                    Filled(supp.Post) ||
                    Filled(supp.Position) ||
                    Filled(supp.InsuranceType) ||
                    Filled(supp.CooperationType) ||
                    supp.StartDate.HasValue ||
                    supp.Salary > 0);

            if (hasJobInfo)
            {
                if (string.IsNullOrWhiteSpace(profile.DepartmentName) || profile.DepartmentName.Trim() == "—")
                    throw new InvalidOperationException("دپارتمان را از فهرست انتخاب کنید.");

                var departmentId = await lookups.IdFromTitleAsync<Department>(profile.DepartmentName);

                var positionTitle = FirstText(supp?.Post, profile.Position, supp?.Position);
                entity.Supplement = new EmployeeSupplement
                {
                    EmployeeSupplementId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    PositionTitle = MapperHelpers.Required(positionTitle, "سمت"),
                    PersonnelCode = MapperHelpers.Required(profile.PersonnelCode, "کد پرسنلی"),
                    StartDate = MapperHelpers.RequireDate(supp?.StartDate ?? profile.HireDate, "تاریخ شروع همکاری"),
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
        /// نگاشت معکوس Entity → EmployeeProfile (همزمان؛ بدون Geo نام‌ها).
        /// برای بارگذاری کامل فرم از <see cref="ToProfileAsync"/> استفاده شود.
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
                Email = entity.Email,
                InsuranceNumber = entity.InsuranceNumber,
                Denomination = entity.Denomination?.Title ?? string.Empty,
                MaritalStatusTitle = entity.MaritalStatus?.Title,
                MilitaryStatusTitle = entity.MilitaryStatus?.Title,
                ChildrenCount = entity.ChildrenCount,
                Gender = MapGenderBack(entity.GenderId, entity.Gender?.Title),
                MaritalStatus = MapMaritalBack(entity.MaritalStatusId, entity.MaritalStatus?.Title),
                MilitaryStatus = MapMilitaryBack(entity.MilitaryStatusId, entity.MilitaryStatus?.Title),
                ExemptionType = entity.ExemptionType?.Title ?? string.Empty,
                ExemptionReason = entity.ExemptionReason?.Title ?? string.Empty,
                Religion = entity.Religion?.Title ?? string.Empty,
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
                    DegreeTitle = edu.Degree?.Title ?? string.Empty,
                    Status = MapEduStatusBack(edu.StatusId, edu.Status?.Title),
                    StatusTitle = edu.Status?.Title ?? string.Empty,
                    Field = edu.FieldOfStudy,
                    University = edu.InstitutionName,
                    Gpa = (double)edu.GPA,
                    StartDate = edu.StartDate,
                    EndDate = edu.EndDate,
                    ThesisTitle = edu.ThesisTitle,
                    CertificateImagePath = FilePath(entity, edu.CertificateFileId)
                });
            }

            foreach (var c in entity.Courses ?? Enumerable.Empty<Course>())
            {
                profile.Courses.Add(new TrainingCourseRecord
                {
                    Title = c.Title,
                    Institute = c.InstitutionName,
                    DurationText = c.DurationHours.ToString(),
                    Note = c.Description,
                    CertificateImagePath = FilePath(entity, c.CertificateFileId)
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
                    AccountType = MapAccountTypeBack(b.AccountTypeId, b.AccountType?.Title),
                    AccountTypeTitle = b.AccountType?.Title ?? string.Empty
                });
            }

            foreach (var p in entity.PhoneNumbers ?? Enumerable.Empty<PhoneNumber>())
            {
                profile.Phones.Add(new PhoneRecord
                {
                    Number = p.Number,
                    Kind = MapPhoneKindBack(p.PhoneNumberType, p.Type?.Title),
                    KindTitle = p.Type?.Title ?? string.Empty,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Note = p.Description
                });
            }

            // بستگان (PersonRole == 0)
            foreach (var r in (entity.RelatedPersons ?? Enumerable.Empty<RelatedPerson>()).Where(x => x.PersonRole == 0))
            {
                profile.Family.Add(new FamilyMemberRecord
                {
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    FatherName = r.FatherName,
                    NationalCode = r.NationalCode,
                    Relation = r.Relationship,
                    PhoneNumber = r.PhoneNumber,
                    Address = r.Address,
                    Note = !string.IsNullOrWhiteSpace(r.Description) ? r.Description : r.JobTitle
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
                    Speaking = MapLangLevelBack(l.SpeakingLevelId, l.SpeakingLevel?.Title),
                    ReadingTitle = l.ReadingLevel?.Title ?? string.Empty,
                    WritingTitle = l.WritingLevel?.Title ?? string.Empty,
                    ComprehensionTitle = l.ComprehensionLevel?.Title ?? string.Empty,
                    SpeakingTitle = l.SpeakingLevel?.Title ?? string.Empty
                });
            }

            foreach (var s in entity.Softwares ?? Enumerable.Empty<EmployeeSoftware>())
            {
                profile.SoftwareSkills.Add(new SoftwareSkillRecord
                {
                    SoftwareName = s.Software?.Title ?? string.Empty,
                    Level = MapSkillBack(s.SkillLevelId, s.SkillLevel?.Title),
                    LevelTitle = s.SkillLevel?.Title ?? string.Empty,
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

            // —— مجموعه‌های تکمیل‌شده ——

            foreach (var c in entity.Citizenships ?? Enumerable.Empty<Citizenship>())
            {
                profile.Citizenships.Add(new CitizenshipRecord
                {
                    Country = string.Empty, // در ToProfileAsync پر می‌شود
                    Reason = c.Reason?.Title ?? string.Empty,
                    StartDate = c.StartDate,
                    Note = c.Description
                });
            }

            foreach (var b in entity.BackgroundRecords ?? Enumerable.Empty<BackgroundRecord>())
            {
                var (person, note) = MapperHelpers.SplitJoined(b.Description);
                profile.CriminalRecords.Add(new CriminalRecord
                {
                    RecordType = b.RecordType?.Title ?? string.Empty,
                    Title = b.Title,
                    PersonName = person ?? string.Empty,
                    Note = note
                });
            }

            foreach (var t in entity.ForeignTravels ?? Enumerable.Empty<ForeignTravel>())
            {
                var (companions, note) = MapperHelpers.SplitJoined(t.Description);
                profile.TripsAbroad.Add(new TripAbroadRecord
                {
                    Reason = t.TravelReason?.Title ?? string.Empty,
                    Transport = t.TransportType?.Title ?? string.Empty,
                    DepartDate = t.DepartureDate,
                    ReturnDate = t.ReturnDate,
                    OriginCountry = string.Empty,
                    DestinationCountry = string.Empty,
                    Companions = companions,
                    Note = note
                });
            }

            foreach (var s in entity.SacrificeRecords ?? Enumerable.Empty<HR.Data.Entities.SacrificeRecord>())
            {
                var (fullName, note) = MapperHelpers.SplitJoined(s.Description);
                profile.Sacrifices.Add(new HR.Models.SacrificeRecord
                {
                    SacrificeRelationship = s.Relationship?.Title ?? string.Empty,
                    SacrificeType = s.Type?.Title ?? string.Empty,
                    Amount = s.Amount.ToString(CultureInfo.InvariantCulture),
                    AmountUnit = s.AmountUnit?.Title,
                    FullName = fullName ?? string.Empty,
                    Note = note
                });
            }

            foreach (var h in entity.HiringRecords ?? Enumerable.Empty<HiringRecord>())
            {
                profile.Recruitments.Add(new RecruitmentRecord
                {
                    InstitutionType = h.SectorType?.Title ?? string.Empty,
                    InstitutionName = h.OrganizationName,
                    Result = h.Result?.Title,
                    Note = h.Description
                });
            }

            foreach (var o in entity.OrganizationCooperations ?? Enumerable.Empty<OrganizationCooperation>())
            {
                profile.OrganCollaborations.Add(new OrganCollaborationRecord
                {
                    OrganName = o.OrganizationName,
                    OrganType = o.SectorType?.Title ?? string.Empty,
                    Note = o.Description
                });
            }

            foreach (var o in entity.OtherActivities ?? Enumerable.Empty<HR.Data.Entities.OtherActivity>())
            {
                profile.OtherActivities.Add(new OtherActivityRecord
                {
                    Title = o.Title,
                    Place = o.ExecutionPlace,
                    Note = o.Description
                });
            }

            foreach (var f in entity.ForeignRelatives ?? Enumerable.Empty<ForeignRelative>())
            {
                profile.ForeignRelatives.Add(new ForeignRelativeRecord
                {
                    FullName = $"{f.FirstName} {f.LastName}".Trim(),
                    Reason = f.AbroadReason?.Title,
                    DepartureDate = f.StartDate,
                    Country = string.Empty,
                    Note = f.Description
                });
            }

            // همکاری خارجی — از RelatedPerson با نقش ۱
            var coopPeople = (entity.RelatedPersons ?? Enumerable.Empty<RelatedPerson>())
                .Where(r => r.PersonRole == 1)
                .ToDictionary(r => r.RelatedPersonId);

            var foreignCooperations = (entity.ForeignCooperations ?? Enumerable.Empty<HR.Data.Entities.ForeignCooperation>())
                .Concat((entity.RelatedPersons ?? Enumerable.Empty<RelatedPerson>())
                    .SelectMany(r => r.ForeignCooperations ?? Enumerable.Empty<HR.Data.Entities.ForeignCooperation>()))
                .GroupBy(c => c.ForeignCooperationId)
                .Select(g => g.First());

            foreach (var fc in foreignCooperations)
            {
                RelatedPerson? person = null;
                if (fc.RelatedPersonId is Guid personId)
                    coopPeople.TryGetValue(personId, out person);
                var (country, note) = MapperHelpers.SplitJoined(fc.Description);
                profile.ForeignCooperations.Add(new ForeignCooperationRecord
                {
                    PersonName = person != null ? $"{person.FirstName} {person.LastName}".Trim() : string.Empty,
                    InstitutionName = fc.CenterName,
                    InstitutionType = fc.CenterType?.Title ?? string.Empty,
                    StartDate = fc.StartDate,
                    EndDate = fc.EndDate,
                    Country = country ?? string.Empty,
                    Note = note
                });
            }

            // PersonFile paths
            foreach (var pf in entity.PersonFiles ?? Enumerable.Empty<PersonFile>())
            {
                var title = pf.FileType?.Title ?? string.Empty;
                var path = pf.FileAddress ?? string.Empty;
                if (title.Contains("عکس")) profile.PhotoPath = path;
                else if (title.Contains("کارت ملی")) profile.NationalCardImagePath = path;
                else if (title.Contains("شناسنامه")) profile.IdCardImagePath = path;
                else if (title.Contains("پایان خدمت") || title.Contains("خدمت")) profile.MilitaryImagePath = path;
                else if (title.Contains("رزومه") && profile.Supplementary != null)
                    profile.Supplementary.ResumePath = path;
            }

            // Abilities
            foreach (var a in entity.Abilities ?? Enumerable.Empty<EmployeeAbility>())
            {
                profile.Abilities.Add(new AbilityRecord
                {
                    Title = a.Ability?.Title ?? string.Empty,
                    Level = MapSkillBack(a.SkillLevelId, a.SkillLevel?.Title),
                    LevelTitle = a.SkillLevel?.Title ?? string.Empty
                });
            }

            // Work interests
            foreach (var w in entity.WorkInterests ?? Enumerable.Empty<EmployeeWorkInterest>())
            {
                profile.WorkInterests.Add(new WorkInterestRecord
                {
                    Title = w.Title,
                    Level = MapInterestBack(w.InterestLevelId, w.InterestLevel?.Title),
                    LevelTitle = w.InterestLevel?.Title ?? string.Empty
                });
            }

            // Self assessment
            if (entity.SelfAssessment != null)
            {
                var sa = entity.SelfAssessment;
                profile.SelfAssessment = new SelfAssessmentRecord
                {
                    Discipline = sa.Discipline,
                    Speed = sa.Speed,
                    Effort = sa.Effort,
                    Negotiation = sa.Negotiation,
                    Patience = sa.Patience,
                    Creativity = sa.Creativity,
                    Punctuality = sa.Punctuality,
                    Teamwork = sa.Teamwork,
                    Accuracy = sa.Accuracy,
                    Planning = sa.Planning
                };
            }

            return profile;
        }

        /// <summary>
        /// نگاشت معکوس کامل با پر کردن نام‌های Geo.
        /// </summary>
        public static async Task<EmployeeProfile> ToProfileAsync(Employee entity, LookupResolver lookups)
        {
            var profile = ToProfile(entity);

            profile.BirthPlace = await lookups.LocationNameByNodeAsync(entity.BirthNode) ?? string.Empty;
            profile.IdIssuePlace = await lookups.LocationNameByNodeAsync(entity.IssueNode) ?? string.Empty;
            profile.Residence = await lookups.LocationNameByNodeAsync(entity.ResidenceNode) ?? string.Empty;

            var address = (entity.Addresses ?? Enumerable.Empty<Address>())
                .FirstOrDefault(a => a.RelatedPersonId == null);
            if (address != null)
            {
                profile.AddressTitle = address.Title;
                profile.AddressText = address.AddressText;
                profile.PostalCode = address.PostalCode;
            }

            // Citizenship countries
            var citizenships = (entity.Citizenships ?? Enumerable.Empty<Citizenship>()).ToList();
            for (int i = 0; i < citizenships.Count && i < profile.Citizenships.Count; i++)
            {
                profile.Citizenships[i].Country =
                    await lookups.CountryNameByIdAsync(citizenships[i].CountryId) ?? string.Empty;
            }

            // Foreign travels origin/destination
            var travels = (entity.ForeignTravels ?? Enumerable.Empty<ForeignTravel>()).ToList();
            for (int i = 0; i < travels.Count && i < profile.TripsAbroad.Count; i++)
            {
                profile.TripsAbroad[i].OriginCountry =
                    await lookups.LocationNameByNodeAsync(travels[i].GeoOrigin) ?? string.Empty;
                profile.TripsAbroad[i].DestinationCountry =
                    await lookups.LocationNameByNodeAsync(travels[i].GeoDestination) ?? string.Empty;
            }

            // Foreign relatives country
            var relatives = (entity.ForeignRelatives ?? Enumerable.Empty<ForeignRelative>()).ToList();
            for (int i = 0; i < relatives.Count && i < profile.ForeignRelatives.Count; i++)
            {
                profile.ForeignRelatives[i].Country =
                    await lookups.LocationNameByNodeAsync(relatives[i].GeoLocation) ?? string.Empty;
            }

            return profile;
        }

        private static HR.Models.InterestLevel MapInterestBack(byte id, string? title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                if (title.Contains("کم")) return HR.Models.InterestLevel.Low;
                if (title.Contains("زیاد") || title.Contains("بالا")) return HR.Models.InterestLevel.High;
            }
            return id switch
            {
                1 => HR.Models.InterestLevel.Low,
                2 => HR.Models.InterestLevel.Medium,
                3 => HR.Models.InterestLevel.High,
                _ => HR.Models.InterestLevel.Medium
            };
        }

        private static bool Filled(string? value)
            => !string.IsNullOrWhiteSpace(value) && value.Trim() != "—";

        private static string FilePath(Employee entity, Guid? fileId)
        {
            if (fileId == null) return string.Empty;
            return entity.PersonFiles?.FirstOrDefault(p => p.PersonFileId == fileId)?.FileAddress ?? string.Empty;
        }

        private static string? FirstText(params string?[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value) && value.Trim() != "—")
                    return value.Trim();
            }
            return null;
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

    }
}
