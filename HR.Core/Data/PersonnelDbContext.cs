using HR.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HR.Data
{
    public class PersonnelDbContext : DbContext
    {
        public PersonnelDbContext()
        {
        }

        public PersonnelDbContext(DbContextOptions<PersonnelDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(PersonnelConnection.Resolve(), sqlOptions =>
                {
                    sqlOptions.UseHierarchyId(); // پشتیبانی hierarchyid
                });
            }
        }

        // ==================== DbSets ====================

        // Core
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<EmployeeSupplement> EmployeeSupplements => Set<EmployeeSupplement>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<DepartmentOffice> DepartmentOffices => Set<DepartmentOffice>();
        public DbSet<Geo> Geos => Set<Geo>();

        // Child tables
        public DbSet<Education> Educations => Set<Education>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
        public DbSet<PhoneNumber> PhoneNumbers => Set<PhoneNumber>();
        public DbSet<RelatedPerson> RelatedPersons => Set<RelatedPerson>();
        public DbSet<EmployeeLanguage> EmployeeLanguages => Set<EmployeeLanguage>();
        public DbSet<EmployeeSoftware> EmployeeSoftwares => Set<EmployeeSoftware>();
        public DbSet<EmploymentHistory> EmploymentHistories => Set<EmploymentHistory>();
        public DbSet<Citizenship> Citizenships => Set<Citizenship>();
        public DbSet<ForeignTravel> ForeignTravels => Set<ForeignTravel>();
        public DbSet<ForeignRelative> ForeignRelatives => Set<ForeignRelative>();
        public DbSet<ForeignCooperation> ForeignCooperations => Set<ForeignCooperation>();
        public DbSet<OrganizationCooperation> OrganizationCooperations => Set<OrganizationCooperation>();
        public DbSet<OtherActivity> OtherActivities => Set<OtherActivity>();
        public DbSet<SacrificeRecord> SacrificeRecords => Set<SacrificeRecord>();
        public DbSet<HiringRecord> HiringRecords => Set<HiringRecord>();
        public DbSet<PersonFile> PersonFiles => Set<PersonFile>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<BackgroundRecord> BackgroundRecords => Set<BackgroundRecord>();
        public DbSet<Ability> AbilitiesLookup => Set<Ability>();
        public DbSet<InterestLevel> InterestLevels => Set<InterestLevel>();
        public DbSet<EmployeeAbility> EmployeeAbilities => Set<EmployeeAbility>();
        public DbSet<EmployeeWorkInterest> EmployeeWorkInterests => Set<EmployeeWorkInterest>();
        public DbSet<EmployeeSelfAssessment> EmployeeSelfAssessments => Set<EmployeeSelfAssessment>();

        // Lookups
        public DbSet<Gender> Genders => Set<Gender>();
        public DbSet<MaritalStatus> MaritalStatuses => Set<MaritalStatus>();
        public DbSet<MilitaryStatus> MilitaryStatuses => Set<MilitaryStatus>();
        public DbSet<Religion> Religions => Set<Religion>();
        public DbSet<Denomination> Denominations => Set<Denomination>();
        public DbSet<EducationDegree> EducationDegrees => Set<EducationDegree>();
        public DbSet<EducationStatus> EducationStatuses => Set<EducationStatus>();
        public DbSet<AccountTypeBank> AccountTypeBanks => Set<AccountTypeBank>();
        public DbSet<Bank> Banks => Set<Bank>();
        public DbSet<InsuranceType> InsuranceTypes => Set<InsuranceType>();
        public DbSet<CooperationType> CooperationTypes => Set<CooperationType>();
        public DbSet<SkillLevel> SkillLevels => Set<SkillLevel>();
        public DbSet<Language> Languages => Set<Language>();
        public DbSet<Software> Softwares => Set<Software>();
        public DbSet<PhoneType> PhoneTypes => Set<PhoneType>();
        public DbSet<PhoneStatus> PhoneStatuses => Set<PhoneStatus>();
        public DbSet<FileType> FileTypes => Set<FileType>();
        public DbSet<EmploymentEndReason> EmploymentEndReasons => Set<EmploymentEndReason>();
        public DbSet<ExemptionType> ExemptionTypes => Set<ExemptionType>();
        public DbSet<ExemptionReason> ExemptionReasons => Set<ExemptionReason>();
        public DbSet<CitizenshipReason> CitizenshipReasons => Set<CitizenshipReason>();
        public DbSet<TransportType> TransportTypes => Set<TransportType>();
        public DbSet<TravelReason> TravelReasons => Set<TravelReason>();
        public DbSet<AbroadReason> AbroadReasons => Set<AbroadReason>();
        public DbSet<CenterType> CenterTypes => Set<CenterType>();
        public DbSet<SectorType> SectorTypes => Set<SectorType>();
        public DbSet<HiringResult> HiringResults => Set<HiringResult>();
        public DbSet<RecordType> RecordTypes => Set<RecordType>();
        public DbSet<SacrificeType> SacrificeTypes => Set<SacrificeType>();
        public DbSet<SacrificeRelationship> SacrificeRelationships => Set<SacrificeRelationship>();
        public DbSet<AmountUnit> AmountUnits => Set<AmountUnit>();
        public DbSet<GeoType> GeoTypes => Set<GeoType>();
        public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<Assessment> Assessments => Set<Assessment>();
        public DbSet<AssessmentDimension> AssessmentDimensions => Set<AssessmentDimension>();
        public DbSet<AssessmentLevel> AssessmentLevels => Set<AssessmentLevel>();
        public DbSet<AssessmentQuestion> AssessmentQuestions => Set<AssessmentQuestion>();
        public DbSet<AssessmentOption> AssessmentOptions => Set<AssessmentOption>();
        public DbSet<AssessmentAttempt> AssessmentAttempts => Set<AssessmentAttempt>();
        public DbSet<AssessmentAnswer> AssessmentAnswers => Set<AssessmentAnswer>();
        public DbSet<LeaveRecord> LeaveRecords => Set<LeaveRecord>();
        public DbSet<AppUser> AppUsers => Set<AppUser>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // ==================== Ability / WorkInterest / SelfAssessment ====================
            modelBuilder.Entity<Ability>(entity =>
            {
                entity.ToTable("Ability");
                entity.HasKey(e => e.AbilityId);
                entity.Property(e => e.Title).HasMaxLength(100).IsRequired();
                entity.HasIndex(e => e.Title).IsUnique();
            });

            modelBuilder.Entity<InterestLevel>(entity =>
            {
                entity.ToTable("InterestLevel");
                entity.HasKey(e => e.InterestLevelId);
                entity.Property(e => e.Title).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Title).IsUnique();
            });

            modelBuilder.Entity<EmployeeAbility>(entity =>
            {
                entity.ToTable("EmployeeAbility");
                entity.HasKey(e => e.EmployeeAbilityId);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasOne(e => e.Employee)
                      .WithMany(emp => emp.Abilities)
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Ability)
                      .WithMany()
                      .HasForeignKey(e => e.AbilityId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.SkillLevel)
                      .WithMany()
                      .HasForeignKey(e => e.SkillLevelId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EmployeeWorkInterest>(entity =>
            {
                entity.ToTable("EmployeeWorkInterest");
                entity.HasKey(e => e.EmployeeWorkInterestId);
                entity.Property(e => e.Title).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasOne(e => e.Employee)
                      .WithMany(emp => emp.WorkInterests)
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.InterestLevel)
                      .WithMany()
                      .HasForeignKey(e => e.InterestLevelId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EmployeeSelfAssessment>(entity =>
            {
                entity.ToTable("EmployeeSelfAssessment");
                entity.HasKey(e => e.EmployeeSelfAssessmentId);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.EmployeeId).IsUnique();
            });

            modelBuilder.Entity<RelatedPerson>(entity =>
            {
                entity.Property(e => e.PersonRole).HasDefaultValue((byte)0);
            });

            base.OnModelCreating(modelBuilder);

            // ==================== Employee ====================
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employee");
                entity.HasKey(e => e.EmployeeId);

                entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.FatherName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.NationalCode).HasMaxLength(10).IsRequired();
                entity.Property(e => e.BirthCertificateNumber).HasMaxLength(10).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(254).IsRequired();
                entity.Property(e => e.InsuranceNumber).HasMaxLength(10).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(250);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasOne(e => e.Gender).WithMany().HasForeignKey(e => e.GenderId);
                entity.HasOne(e => e.MaritalStatus).WithMany().HasForeignKey(e => e.MaritalStatusId);
                entity.HasOne(e => e.MilitaryStatus).WithMany().HasForeignKey(e => e.MilitaryStatusId);
                entity.HasOne(e => e.Religion).WithMany().HasForeignKey(e => e.ReligionId);
                entity.HasOne(e => e.Denomination).WithMany().HasForeignKey(e => e.DenominationId).OnDelete(DeleteBehavior.NoAction);
                entity.HasIndex(e => e.NationalCode).IsUnique();

                entity.HasOne(e => e.Supplement)
                      .WithOne(s => s.Employee)
                      .HasForeignKey<EmployeeSupplement>(s => s.EmployeeId);

                entity.HasOne(e => e.ExemptionType)
                      .WithMany()
                      .HasForeignKey(e => e.ExemptionTypeId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.ExemptionReason)
                      .WithMany()
                      .HasForeignKey(e => e.ExemptionReasonId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.SelfAssessment)
                      .WithOne(s => s.Employee)
                      .HasForeignKey<EmployeeSelfAssessment>(s => s.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

            });

            // ==================== EmployeeSupplement ====================
            modelBuilder.Entity<EmployeeSupplement>(entity =>
            {
                entity.ToTable("EmployeeSupplement");
                entity.HasKey(e => e.EmployeeSupplementId);

                entity.Property(e => e.PositionTitle).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PersonnelCode).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Salary).HasColumnType("decimal(18,0)");

                entity.HasIndex(e => e.EmployeeId).IsUnique();
                entity.HasIndex(e => e.PersonnelCode).IsUnique();

                entity.HasOne(e => e.Department).WithMany(d => d.Employees).HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.DirectManager).WithMany().HasForeignKey(e => e.DirectManagerEmployeeId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.InsuranceType).WithMany().HasForeignKey(e => e.InsuranceTypeId);
                entity.HasOne(e => e.CooperationType).WithMany().HasForeignKey(e => e.CooperationTypeId);
            });

            // ==================== Department ====================
            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("Department");
                entity.HasKey(e => e.DepartmentId);

                entity.Property(e => e.Title).HasMaxLength(100).IsRequired();
                entity.Property(e => e.SecurityLevel).HasMaxLength(50);
                entity.Property(e => e.LogoPath).HasMaxLength(500);
                entity.Property(e => e.CostCenterCode).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.AnnualBudget).HasColumnType("decimal(18,0)");
                entity.Property(e => e.PettyCashLimit).HasColumnType("decimal(18,0)");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasIndex(e => new { e.Title, e.ParentDepartmentId }).IsUnique();

                entity.HasOne(e => e.Parent)
                      .WithMany(d => d.Children)
                      .HasForeignKey(e => e.ParentDepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Manager)
                      .WithMany()
                      .HasForeignKey(e => e.ManagerEmployeeId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Deputy)
                      .WithMany()
                      .HasForeignKey(e => e.DeputyEmployeeId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.CashCustodian)
                      .WithMany()
                      .HasForeignKey(e => e.CashCustodianEmployeeId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<DepartmentOffice>(entity =>
            {
                entity.ToTable("DepartmentOffice");
                entity.HasKey(e => e.OfficeId);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(500);

                entity.HasOne(e => e.Department)
                      .WithMany(d => d.Offices)
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================== Geo ====================
            modelBuilder.Entity<Geo>(entity =>
            {
                entity.ToTable("Geo");
                entity.HasKey(e => e.GeoId);
                entity.Property(e => e.Node).IsRequired();
                entity.HasIndex(e => e.Node).IsUnique();
                entity.Property(e => e.Level).HasComputedColumnSql("[Node].[GetLevel]()", stored: true);
            });

            // ==================== Education ====================
            modelBuilder.Entity<Education>(entity =>
            {
                entity.ToTable("Education");
                entity.HasKey(e => e.EducationId);
                entity.Property(e => e.FieldOfStudy).HasMaxLength(150).IsRequired();
                entity.Property(e => e.InstitutionName).HasMaxLength(150).IsRequired();
                entity.Property(e => e.GPA).HasColumnType("decimal(20,0)");
                entity.Property(e => e.ThesisTitle).HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(250);

                entity.HasOne(e => e.Employee).WithMany(emp => emp.Educations).HasForeignKey(e => e.EmployeeId);
                entity.HasOne(e => e.Status).WithMany().HasForeignKey(e => e.StatusId);
                entity.HasOne(e => e.Degree).WithMany().HasForeignKey(e => e.DegreeId);
            });

            // ==================== Course ====================
            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Course");
                entity.HasKey(e => e.CourseId);
                entity.Property(e => e.Title).HasMaxLength(150).IsRequired();
                entity.Property(e => e.InstitutionName).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(250);

                entity.HasOne(e => e.Employee).WithMany(emp => emp.Courses).HasForeignKey(e => e.EmployeeId);
            });

            // ==================== BankAccount ====================
            modelBuilder.Entity<BankAccount>(entity =>
            {
                entity.ToTable("BankAccount");
                entity.HasKey(e => e.BankAccountId);
                entity.Property(e => e.AccountNumber).HasMaxLength(10).IsRequired();
                entity.Property(e => e.Sheba).HasMaxLength(26).IsRequired().IsFixedLength();
                entity.Property(e => e.CardNumber).HasMaxLength(16).IsRequired().IsFixedLength();
                entity.Property(e => e.Description).HasMaxLength(250);

                entity.HasIndex(e => new { e.AccountNumber, e.BankId }).IsUnique();
                entity.HasIndex(e => e.Sheba).IsUnique();

                entity.HasOne(e => e.Employee).WithMany(emp => emp.BankAccounts).HasForeignKey(e => e.EmployeeId);
                entity.HasOne(e => e.Bank).WithMany().HasForeignKey(e => e.BankId);
                entity.HasOne(e => e.AccountType).WithMany().HasForeignKey(e => e.AccountTypeId);
            });

            // ==================== PhoneNumber ====================
            modelBuilder.Entity<PhoneNumber>(entity =>
            {
                entity.ToTable("PhoneNumber");
                entity.HasKey(e => e.PhoneNumberId);
                entity.Property(e => e.Number).HasMaxLength(11).IsRequired();
                entity.Property(e => e.OwnerFirstName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.OwnerLastName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(250);

                entity.HasOne(e => e.Employee).WithMany(emp => emp.PhoneNumbers).HasForeignKey(e => e.EmployeeId);
                entity.HasOne(e => e.Status).WithMany().HasForeignKey(e => e.PhoneNumberStatus);
                entity.HasOne(e => e.Type).WithMany().HasForeignKey(e => e.PhoneNumberType);
            });

            // ==================== RelatedPerson ====================
            modelBuilder.Entity<RelatedPerson>(entity =>
            {
                entity.ToTable("RelatedPerson");
                entity.HasKey(e => e.RelatedPersonId);
                entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.FatherName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.NationalCode).HasMaxLength(10).IsRequired();
                entity.Property(e => e.BirthCertificateNumber).HasMaxLength(10).IsRequired();
                entity.Property(e => e.JobTitle).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Relationship).HasMaxLength(50).IsRequired();

                entity.HasOne(e => e.Employee).WithMany(emp => emp.RelatedPersons).HasForeignKey(e => e.EmployeeId);
            });

            // ==================== EmployeeLanguage ====================
            modelBuilder.Entity<EmployeeLanguage>(entity =>
            {
                entity.ToTable("EmployeeLanguage");
                entity.HasKey(e => e.EmployeeLanguageId);
                entity.Property(e => e.Description).HasMaxLength(250);

                entity.HasIndex(e => new { e.EmployeeId, e.LanguageId }).IsUnique();

                entity.HasOne(e => e.Employee).WithMany(emp => emp.Languages).HasForeignKey(e => e.EmployeeId);
                entity.HasOne(e => e.Language).WithMany().HasForeignKey(e => e.LanguageId);
                entity.HasOne(e => e.ReadingLevel).WithMany().HasForeignKey(e => e.ReadingLevelId);
                entity.HasOne(e => e.WritingLevel).WithMany().HasForeignKey(e => e.WritingLevelId);
                entity.HasOne(e => e.ComprehensionLevel).WithMany().HasForeignKey(e => e.ComprehensionLevelId);
                entity.HasOne(e => e.SpeakingLevel).WithMany().HasForeignKey(e => e.SpeakingLevelId);
            });

            // ==================== EmployeeSoftware ====================
            modelBuilder.Entity<EmployeeSoftware>(entity =>
            {
                entity.ToTable("EmployeeSoftware");
                entity.HasKey(e => e.EmployeeSoftwareId);
                entity.Property(e => e.Description).HasMaxLength(250);

                entity.HasIndex(e => new { e.EmployeeId, e.SoftwareId }).IsUnique();

                entity.HasOne(e => e.Employee).WithMany(emp => emp.Softwares).HasForeignKey(e => e.EmployeeId);
                entity.HasOne(e => e.Software).WithMany().HasForeignKey(e => e.SoftwareId);
                entity.HasOne(e => e.SkillLevel).WithMany().HasForeignKey(e => e.SkillLevelId);
            });

            // ==================== EmploymentHistory ====================
            modelBuilder.Entity<EmploymentHistory>(entity =>
            {
                entity.ToTable("EmploymentHistory");
                entity.HasKey(e => e.EmploymentHistoryId);
                entity.Property(e => e.WorkplaceName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.JobTitle).HasMaxLength(150).IsRequired();
                entity.Property(e => e.PhoneNumber).HasMaxLength(11).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(250).IsRequired();

                entity.HasOne(e => e.Employee).WithMany(emp => emp.EmploymentHistories).HasForeignKey(e => e.EmployeeId);
                entity.HasOne(e => e.EndReason).WithMany().HasForeignKey(e => e.EndReasonId);
            });

            // ==================== Lookups - simple tables ====================
            modelBuilder.Entity<Gender>().ToTable("Gender").HasKey(e => e.GenderId);
            modelBuilder.Entity<MaritalStatus>().ToTable("MaritalStatus").HasKey(e => e.MaritalStatusId);
            modelBuilder.Entity<MilitaryStatus>().ToTable("MilitaryStatus").HasKey(e => e.MilitaryStatusId);
            modelBuilder.Entity<Religion>().ToTable("Religion").HasKey(e => e.ReligionId);
            modelBuilder.Entity<Denomination>().ToTable("Denomination").HasKey(e => e.DenominationId);
            modelBuilder.Entity<EducationDegree>().ToTable("EducationDegree").HasKey(e => e.DegreeId);
            modelBuilder.Entity<EducationStatus>().ToTable("EducationStatus").HasKey(e => e.StatusId);
            modelBuilder.Entity<AccountTypeBank>().ToTable("AccountTypeBank").HasKey(e => e.AccountTypeId);
            modelBuilder.Entity<Bank>().ToTable("Bank").HasKey(e => e.BankId);
            modelBuilder.Entity<InsuranceType>().ToTable("InsuranceType").HasKey(e => e.InsuranceTypeId);
            modelBuilder.Entity<CooperationType>().ToTable("CooperationType").HasKey(e => e.CooperationTypeId);
            modelBuilder.Entity<SkillLevel>().ToTable("SkillLevel").HasKey(e => e.SkillLevelId);
            modelBuilder.Entity<Language>().ToTable("Language").HasKey(e => e.LanguageId);
            modelBuilder.Entity<Software>().ToTable("Software").HasKey(e => e.SoftwareId);
            modelBuilder.Entity<PhoneType>().ToTable("PhoneType").HasKey(e => e.PhoneTypeId);
            modelBuilder.Entity<PhoneStatus>().ToTable("PhoneStatus").HasKey(e => e.PhoneStatusId);
            modelBuilder.Entity<FileType>().ToTable("FileType").HasKey(e => e.FileTypeId);
            modelBuilder.Entity<EmploymentEndReason>().ToTable("EmploymentEndReason").HasKey(e => e.EndReasonId);
            modelBuilder.Entity<ExemptionType>().ToTable("ExemptionType").HasKey(e => e.ExemptionTypeId);
            modelBuilder.Entity<ExemptionReason>().ToTable("ExemptionReason").HasKey(e => e.ExemptionReasonId);
            modelBuilder.Entity<CitizenshipReason>().ToTable("CitizenshipReason").HasKey(e => e.CitizenshipReasonId);
            modelBuilder.Entity<TransportType>().ToTable("TransportType").HasKey(e => e.TransportTypeId);
            modelBuilder.Entity<TravelReason>().ToTable("TravelReason").HasKey(e => e.TravelReasonId);
            modelBuilder.Entity<AbroadReason>().ToTable("AbroadReason").HasKey(e => e.AbroadReasonId);
            modelBuilder.Entity<CenterType>().ToTable("CenterType").HasKey(e => e.CenterTypeId);
            modelBuilder.Entity<SectorType>().ToTable("SectorType").HasKey(e => e.SectorTypeId);
            modelBuilder.Entity<HiringResult>().ToTable("HiringResult").HasKey(e => e.HiringResultId);
            modelBuilder.Entity<RecordType>().ToTable("RecordType").HasKey(e => e.RecordTypeId);
            modelBuilder.Entity<SacrificeType>().ToTable("SacrificeType").HasKey(e => e.SacrificeTypeId);
            modelBuilder.Entity<SacrificeRelationship>().ToTable("SacrificeRelationship").HasKey(e => e.SacrificeRelationshipId);
            modelBuilder.Entity<AmountUnit>().ToTable("AmountUnit").HasKey(e => e.AmountUnitId);
            modelBuilder.Entity<GeoType>().ToTable("GeoType").HasKey(e => e.GeoTypeId);

            // جداولی که نام مفرد SQL دارند؛ بدون ToTable، EF نام جمع DbSet را می‌خواهد.
            modelBuilder.Entity<Citizenship>(entity =>
            {
                entity.ToTable("Citizenship");
                entity.HasKey(e => e.CitizenshipId);
                entity.Property(e => e.Description).HasMaxLength(250);

                entity.HasOne(e => e.Employee).WithMany(emp => emp.Citizenships).HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.RelatedPerson).WithMany().HasForeignKey(e => e.RelatedPersonId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Reason).WithMany().HasForeignKey(e => e.CitizenshipReasonId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<ForeignTravel>(entity =>
            {
                entity.ToTable("ForeignTravel");
                entity.HasKey(e => e.ForeignTravelId);
                entity.Property(e => e.Description).HasMaxLength(250);

                entity.HasOne(e => e.Employee).WithMany(emp => emp.ForeignTravels).HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.TravelReason).WithMany().HasForeignKey(e => e.TravelReasonId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.TransportType).WithMany().HasForeignKey(e => e.TransportTypeId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<ForeignRelative>(entity =>
            {
                entity.ToTable("ForeignRelative");
                entity.HasKey(e => e.ForeignRelativeId);
                entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(250);

                entity.HasOne(e => e.Employee).WithMany(emp => emp.ForeignRelatives).HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.AbroadReason).WithMany().HasForeignKey(e => e.AbroadReasonId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<ForeignCooperation>(entity =>
            {
                entity.ToTable("ForeignCooperation");
                entity.HasKey(e => e.ForeignCooperationId);
                entity.Property(e => e.CenterName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(250);

                entity.HasOne(e => e.Employee).WithMany(emp => emp.ForeignCooperations).HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.RelatedPerson).WithMany(p => p.ForeignCooperations).HasForeignKey(e => e.RelatedPersonId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.CenterType).WithMany().HasForeignKey(e => e.CenterTypeId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<OrganizationCooperation>(entity =>
            {
                entity.ToTable("OrganizationCooperation");
                entity.HasKey(e => e.OrganizationCooperationId);
                entity.Property(e => e.OrganizationName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(250);

                entity.HasOne(e => e.Employee).WithMany(emp => emp.OrganizationCooperations).HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.SectorType).WithMany().HasForeignKey(e => e.SectorTypeId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<OtherActivity>(entity =>
            {
                entity.ToTable("OtherActivity");
                entity.HasKey(e => e.OtherActivityId);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.ExecutionPlace).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(250);

                entity.HasOne(e => e.Employee).WithMany(emp => emp.OtherActivities).HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<SacrificeRecord>(entity =>
            {
                entity.ToTable("SacrificeRecord");
                entity.HasKey(e => e.SacrificeRecordId);
                entity.Property(e => e.Amount).HasColumnType("decimal(6,2)");
                entity.Property(e => e.Description).HasMaxLength(250);
                entity.Property(e => e.RelatedPersonId).HasColumnName("RelatedPersonID");

                entity.HasOne(e => e.Employee).WithMany(emp => emp.SacrificeRecords).HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Relationship).WithMany().HasForeignKey(e => e.SacrificeRelationshipId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Type).WithMany().HasForeignKey(e => e.SacrificeTypeId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.AmountUnit).WithMany().HasForeignKey(e => e.AmountUnitId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<HiringRecord>(entity =>
            {
                entity.ToTable("HiringRecord");
                entity.HasKey(e => e.HiringRecordId);
                entity.Property(e => e.OrganizationName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasOne(e => e.Employee).WithMany(emp => emp.HiringRecords).HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.SectorType).WithMany().HasForeignKey(e => e.SectorTypeId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Result).WithMany().HasForeignKey(e => e.HiringResultId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<BackgroundRecord>(entity =>
            {
                entity.ToTable("BackgroundRecord");
                entity.HasKey(e => e.BackgroundRecordId);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(250);

                entity.HasOne(e => e.Employee).WithMany(emp => emp.BackgroundRecords).HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.RelatedPerson).WithMany().HasForeignKey(e => e.RelatedPersonId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.RecordType).WithMany().HasForeignKey(e => e.RecordTypeId).OnDelete(DeleteBehavior.NoAction);
            });

            // Address mapping (column name difference)
            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("Address");
                entity.HasKey(e => e.AddressId);
                entity.Property(e => e.AddressText).HasColumnName("Address").HasMaxLength(500).IsRequired();
                entity.Property(e => e.Title).HasMaxLength(50).IsRequired();
                entity.Property(e => e.PostalCode).HasMaxLength(10);

                entity.HasOne(e => e.Employee).WithMany(emp => emp.Addresses).HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.RelatedPerson).WithMany().HasForeignKey(e => e.RelatedPersonId).OnDelete(DeleteBehavior.NoAction);
            });

            // PersonFile
            modelBuilder.Entity<PersonFile>(entity =>
            {
                entity.ToTable("PersonFile");
                entity.HasKey(e => e.PersonFileId);
                entity.Property(e => e.FileAddress).HasMaxLength(500).IsRequired();
            });

            modelBuilder.Entity<DocumentType>(entity =>
            {
                entity.ToTable("DocumentType");
                entity.HasKey(e => e.DocumentTypeId);
                entity.Property(e => e.Title).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Title).IsUnique();
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.ToTable("Document");
                entity.HasKey(e => e.DocumentId);
                entity.Property(e => e.OriginalFileName).HasMaxLength(260).IsRequired();
                entity.Property(e => e.StoredRelativePath).HasMaxLength(500).IsRequired();
                entity.Property(e => e.ContentType).HasMaxLength(127).IsRequired();
                entity.Property(e => e.Sha256).HasMaxLength(64).IsFixedLength().IsRequired();
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Department).WithMany().HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Type).WithMany().HasForeignKey(e => e.DocumentTypeId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Assessment>(entity =>
            {
                entity.ToTable("Assessment");
                entity.HasKey(e => e.AssessmentId);
                entity.Property(e => e.Code).HasMaxLength(64).IsRequired();
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ScoringMode).HasMaxLength(32).IsRequired();
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            modelBuilder.Entity<AssessmentDimension>(entity =>
            {
                entity.ToTable("AssessmentDimension");
                entity.HasKey(e => e.DimensionId);
                entity.Property(e => e.Key).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Title).HasMaxLength(100).IsRequired();
                entity.HasOne(e => e.Assessment).WithMany(a => a.Dimensions).HasForeignKey(e => e.AssessmentId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AssessmentLevel>(entity =>
            {
                entity.ToTable("AssessmentLevel");
                entity.HasKey(e => e.LevelId);
                entity.Property(e => e.Label).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ColorHex).HasMaxLength(16).IsRequired();
                entity.HasOne(e => e.Assessment).WithMany(a => a.Levels).HasForeignKey(e => e.AssessmentId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AssessmentQuestion>(entity =>
            {
                entity.ToTable("AssessmentQuestion");
                entity.HasKey(e => e.QuestionId);
                entity.Property(e => e.Code).HasMaxLength(32).IsRequired();
                entity.Property(e => e.QuestionType).HasMaxLength(32).IsRequired();
                entity.Property(e => e.Text).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.HelpText).HasMaxLength(500);
                entity.Property(e => e.DimensionKey).HasMaxLength(50);
                entity.Property(e => e.MinLabel).HasMaxLength(50);
                entity.Property(e => e.MaxLabel).HasMaxLength(50);
                entity.HasOne(e => e.Assessment).WithMany(a => a.Questions).HasForeignKey(e => e.AssessmentId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AssessmentOption>(entity =>
            {
                entity.ToTable("AssessmentOption");
                entity.HasKey(e => e.OptionId);
                entity.Property(e => e.Code).HasMaxLength(16).IsRequired();
                entity.Property(e => e.Text).HasMaxLength(400).IsRequired();
                entity.HasOne(e => e.Question).WithMany(q => q.Options).HasForeignKey(e => e.QuestionId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AssessmentAttempt>(entity =>
            {
                entity.ToTable("AssessmentAttempt");
                entity.HasKey(e => e.AttemptId);
                entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
                entity.Property(e => e.LevelLabel).HasMaxLength(50);
                entity.Property(e => e.Summary).HasMaxLength(500);
                entity.HasOne(e => e.Assessment).WithMany(a => a.Attempts).HasForeignKey(e => e.AssessmentId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AssessmentAnswer>(entity =>
            {
                entity.ToTable("AssessmentAnswer");
                entity.HasKey(e => e.AnswerId);
                entity.Property(e => e.TextValue).HasMaxLength(2000);
                entity.HasOne(e => e.Attempt).WithMany(a => a.Answers).HasForeignKey(e => e.AttemptId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Question).WithMany().HasForeignKey(e => e.QuestionId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Option).WithMany().HasForeignKey(e => e.OptionId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<LeaveRecord>(entity =>
            {
                entity.ToTable("LeaveRecord");
                entity.HasKey(e => e.LeaveId);
                entity.Property(e => e.LeaveType).HasMaxLength(30).IsRequired();
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.HasOne(e => e.Employee).WithMany(employee => employee.Leaves).HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");
                entity.HasKey(e => e.RoleId);
                entity.Property(e => e.RoleId).ValueGeneratedNever();
                entity.Property(e => e.Name).HasMaxLength(32).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique();
            });

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("AppUser");
                entity.HasKey(e => e.AppUserId);
                entity.Property(e => e.Username).HasMaxLength(64).IsRequired();
                entity.Property(e => e.DisplayName).HasMaxLength(100);
                entity.Property(e => e.PasswordHash).HasMaxLength(64).IsRequired();
                entity.Property(e => e.PasswordSalt).HasMaxLength(32).IsRequired();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("UserRole");
                entity.HasKey(e => new { e.AppUserId, e.RoleId });
                entity.HasOne(e => e.User).WithMany(u => u.Roles).HasForeignKey(e => e.AppUserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Role).WithMany(r => r.Users).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
