using System;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace HR.Models
{
    public enum Gender { Unspecified, Male, Female }
    public enum MaritalStatus { Unspecified, Single, Married, Divorced, Widowed }
    public enum MilitaryStatus { Unspecified, Done, Exempt, InProgress, NotApplicable }

    public enum EmployeeStatus { Active, OnLeave, Terminated, Retired }
    public enum PhoneKind { Mobile, Home, Work, Emergency, Other }
    public enum DegreeLevel { Diploma, Associate, Bachelor, Master, Doctorate, PostDoctorate, Other }
    public enum LanguageLevel { None, Basic, Intermediate, Good, Fluent, Native }
    public enum SkillLevel { Weak, Average, Good, Excellent }
    public enum BankAccountType { Current, Savings, ShortTerm, LongTerm, Other }
    public enum EducationStatusKind { Graduated, Studying, DroppedOut, OnHold }
    public enum InterestLevel { Low, Medium, High }

    /// <summary>
    /// پرونده کامل کارمند — هسته‌ی داده‌ی برنامه.
    /// بعداً مستقیماً به SQL Server نگاشت می‌شود.
    /// </summary>
    public class EmployeeProfile
    {
        public int Id { get; set; }
        public string PersonnelCode { get; set; } = string.Empty;

        // ---------- مشخصات فردی ----------
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string NationalCode { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public string BirthPlace { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public MaritalStatus MaritalStatus { get; set; }
        public DateTime? IdIssueDate { get; set; }
        public string IdIssuePlace { get; set; } = string.Empty;
        public string ExemptionReason { get; set; } = string.Empty;
        public int? ChildrenCount { get; set; }
        public MilitaryStatus MilitaryStatus { get; set; }

        public string PhotoPath { get; set; } = string.Empty;
        public string NationalCardImagePath { get; set; } = string.Empty;
        public string IdCardImagePath { get; set; } = string.Empty;
        public string MilitaryImagePath { get; set; } = string.Empty;

        // ---------- اطلاعات شغلی ----------
        public string? DepartmentName { get; set; }
        public string? Position { get; set; }
        public DateTime? HireDate { get; set; }
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

        // ---------- مجموعه‌ها ----------
        public ObservableCollection<PhoneRecord> Phones { get; set; } = new();
        public ObservableCollection<EducationRecord> Educations { get; set; } = new();
        public ObservableCollection<TrainingCourseRecord> Courses { get; set; } = new();
        public ObservableCollection<JobHistoryRecord> JobHistories { get; set; } = new();
        public ObservableCollection<BankAccountRecord> BankAccounts { get; set; } = new();
        public ObservableCollection<FamilyMemberRecord> Family { get; set; } = new();
        public ObservableCollection<LanguageRecord> Languages { get; set; } = new();
        public ObservableCollection<SoftwareSkillRecord> SoftwareSkills { get; set; } = new();
        public ObservableCollection<AbilityRecord> Abilities { get; set; } = new();
        public ObservableCollection<WorkInterestRecord> WorkInterests { get; set; } = new();
        public ObservableCollection<TripAbroadRecord> TripsAbroad { get; set; } = new();
        public ObservableCollection<ForeignCooperationRecord> ForeignCooperations { get; set; } = new();
        public ObservableCollection<CitizenshipRecord> Citizenships { get; set; } = new();
        public ObservableCollection<CriminalRecord> CriminalRecords { get; set; } = new();
        public ObservableCollection<ForeignRelativeRecord> ForeignRelatives { get; set; } = new();
        public ObservableCollection<RecruitmentRecord> Recruitments { get; set; } = new();
        public ObservableCollection<OrganCollaborationRecord> OrganCollaborations { get; set; } = new();
        public ObservableCollection<OtherActivityRecord> OtherActivities { get; set; } = new();
        public ObservableCollection<SacrificeRecord> Sacrifices { get; set; } = new();

        public SupplementaryInfo Supplementary { get; set; } = new();
        public SelfAssessmentRecord SelfAssessment { get; set; } = new();

        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    // ==================== رکوردهای فرزند ====================

    public class PhoneRecord : ObservableRow
    {
        public int Id { get; set; }
        public PhoneKind Kind { get; set; }
        public string Number { get; set; } = string.Empty;
        public string? Owner { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Note { get; set; }

        private bool _isPrimary;
        public bool IsPrimary
        {
            get => _isPrimary;
            set
            {
                if (_isPrimary == value) return;
                _isPrimary = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PrimaryText));
            }
        }

        public string PrimaryText => IsPrimary ? "اصلی" : string.Empty;

        public string KindText => Kind switch
        {
            PhoneKind.Mobile => "همراه",
            PhoneKind.Home => "منزل",
            PhoneKind.Work => "کاری",
            PhoneKind.Emergency => "اضطراری",
            _ => "سایر"
        };
    }

    public class EducationRecord : ObservableRow
    {
        public int Id { get; set; }
        public DegreeLevel Degree { get; set; }
        public string Field { get; set; } = string.Empty;
        public string University { get; set; } = string.Empty;
        public string? State { get; set; }
        public string? City { get; set; }
        public EducationStatusKind Status { get; set; }
        public double? Gpa { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ThesisTitle { get; set; }
        public string CertificateImagePath { get; set; } = string.Empty;

        public string DegreeText => Degree switch
        {
            DegreeLevel.Diploma => "دیپلم",
            DegreeLevel.Associate => "کاردانی",
            DegreeLevel.Bachelor => "کارشناسی",
            DegreeLevel.Master => "کارشناسی ارشد",
            DegreeLevel.Doctorate => "دکتری",
            DegreeLevel.PostDoctorate => "پس‌دکتری",
            _ => "سایر"
        };

        public string StatusText => Status switch
        {
            EducationStatusKind.Graduated => "فارغ‌التحصیل",
            EducationStatusKind.Studying => "در حال تحصیل",
            EducationStatusKind.DroppedOut => "انصراف",
            _ => "مرخصی تحصیلی"
        };
    }

    public class TrainingCourseRecord : ObservableRow
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Institute { get; set; } = string.Empty;
        public string? DurationText { get; set; }
        public string? Note { get; set; }
        public string CertificateImagePath { get; set; } = string.Empty;
    }

    public class JobHistoryRecord : ObservableRow
    {
        public int Id { get; set; }
        public string Company { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string? CompanyPhone { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? LeaveReason { get; set; }
        public string? Note { get; set; }
    }

    public class BankAccountRecord : ObservableRow
    {
        public int Id { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string ShebaNumber { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public BankAccountType AccountType { get; set; }

        public string AccountTypeText => AccountType switch
        {
            BankAccountType.Current => "جاری",
            BankAccountType.Savings => "پس‌انداز",
            BankAccountType.ShortTerm => "کوتاه‌مدت",
            BankAccountType.LongTerm => "بلندمدت",
            _ => "سایر"
        };
    }

    public class FamilyMemberRecord : ObservableRow
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Relation { get; set; } = string.Empty;
        public string? FatherName { get; set; }
        public string? NationalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Note { get; set; }
    }

    public class LanguageRecord : ObservableRow
    {
        public int Id { get; set; }
        public string Language { get; set; } = string.Empty;
        public LanguageLevel Reading { get; set; }
        public LanguageLevel Writing { get; set; }
        public LanguageLevel Comprehension { get; set; }
        public LanguageLevel Speaking { get; set; }

        public string ReadingText => LanguageLevelText(Reading);
        public string WritingText => LanguageLevelText(Writing);
        public string ComprehensionText => LanguageLevelText(Comprehension);
        public string SpeakingText => LanguageLevelText(Speaking);

        public static string LanguageLevelText(LanguageLevel level) => level switch
        {
            LanguageLevel.None => "—",
            LanguageLevel.Basic => "مبتدی",
            LanguageLevel.Intermediate => "متوسط",
            LanguageLevel.Good => "خوب",
            LanguageLevel.Fluent => "روان",
            LanguageLevel.Native => "زبان مادری",
            _ => "—"
        };
    }

    public class SoftwareSkillRecord : ObservableRow
    {
        public int Id { get; set; }
        public string SoftwareName { get; set; } = string.Empty;
        public SkillLevel Level { get; set; }
        public string? Note { get; set; }

        public string LevelText => Level switch
        {
            SkillLevel.Weak => "ضعیف",
            SkillLevel.Average => "متوسط",
            SkillLevel.Good => "خوب",
            _ => "عالی"
        };

        public string DisplayText => $"{SoftwareName} · {LevelText}";

        public Brush SkillLevelBrush => Level switch
        {
            SkillLevel.Weak => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F1D1D")),
            SkillLevel.Average => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E3A5F")),
            SkillLevel.Good => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#14532D")),
            _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#065F46"))
        };
    }

    public class SkillTagRecord : ObservableRow
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public SkillLevel Level { get; set; }
    }

    public class TripAbroadRecord : ObservableRow
    {
        public int Id { get; set; }
        public string OriginCountry { get; set; } = string.Empty;
        public string DestinationCountry { get; set; } = string.Empty;
        public string Transport { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime? DepartDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string? Companions { get; set; }
        public string? Note { get; set; }
    }
    public class ForeignCooperationRecord : ObservableRow
    {
        public int Id { get; set; }
        public string PersonName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string InstitutionName { get; set; } = string.Empty;
        public string InstitutionType { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Note { get; set; }
    }
    public class CitizenshipRecord : ObservableRow
    {
        public int Id { get; set; }
        public string Country { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public string? Note { get; set; }
    }
    public class CriminalRecord : ObservableRow
    {
        public int Id { get; set; }
        public string PersonName { get; set; } = string.Empty;
        public string RecordType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class ForeignRelativeRecord : ObservableRow
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public DateTime? DepartureDate { get; set; }
        public string? Reason { get; set; }
        public string? Note { get; set; }
    }

    public class RecruitmentRecord : ObservableRow
    {
        public int Id { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public string InstitutionType { get; set; } = string.Empty;
        public string? Result { get; set; }
        public string? Note { get; set; }
    }

    public class OrganCollaborationRecord : ObservableRow
    {
        public int Id { get; set; }
        public string OrganName { get; set; } = string.Empty;
        public string OrganType { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class OtherActivityRecord : ObservableRow
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Place { get; set; }
        public string? Note { get; set; }
    }

    public class SacrificeRecord : ObservableRow
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string SacrificeType { get; set; } = string.Empty;
        public string? Amount { get; set; }
        public string? Note { get; set; }
    }

    public class SupplementaryInfo : ObservableRow
    {
        public int Id { get; set; }
        public string? Post { get; set; }
        public string? Position { get; set; }
        public string? InsuranceType { get; set; }
        public string? CooperationType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? Salary { get; set; }
        public string ResumePath { get; set; } = string.Empty;
    }

    public class SelfAssessmentRecord : ObservableRow
    {
        public int Id { get; set; }
        public int Discipline { get; set; } = 50;
        public int Speed { get; set; } = 50;
        public int Effort { get; set; } = 50;
        public int Negotiation { get; set; } = 50;
        public int Patience { get; set; } = 50;
        public int Creativity { get; set; } = 50;
        public int Punctuality { get; set; } = 50;
        public int Teamwork { get; set; } = 50;
        public int Accuracy { get; set; } = 50;
        public int Planning { get; set; } = 50;

        public double Average =>
            (Discipline + Speed + Effort + Negotiation + Patience +
             Creativity + Punctuality + Teamwork + Accuracy + Planning) / 10.0;
    }
    public class AbilityRecord : ObservableRow
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public SkillLevel Level { get; set; }

        public string DisplayText => $"{Title} · {LevelText}";

        public string LevelText => Level switch
        {
            SkillLevel.Weak => "ضعیف",
            SkillLevel.Average => "متوسط",
            SkillLevel.Good => "خوب",
            _ => "عالی"
        };

        public Brush AbilityLevelBrush => Level switch
        {
            SkillLevel.Weak => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F1D1D")),
            SkillLevel.Average => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E3A5F")),
            SkillLevel.Good => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#14532D")),
            _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#065F46"))
        };
    }


    public class WorkInterestRecord : ObservableRow
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public InterestLevel Level { get; set; }

        public string DisplayText => $"{Title} · {LevelText}";

        public string LevelText => Level switch
        {
            InterestLevel.Low => "کم",
            InterestLevel.Medium => "متوسط",
            _ => "زیاد"
        };

        public Brush InterestLevelBrush => Level switch
        {
            InterestLevel.Low => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F1D1D")),
            InterestLevel.Medium => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E3A5F")),
            _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#14532D"))
        };
    }
}