using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HR.Data.Entities
{
    public class Employee
    {
        public Guid EmployeeId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string NationalCode { get; set; } = string.Empty;
        public string BirthCertificateNumber { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public DateTime IssueDate { get; set; }
        public string Email { get; set; } = string.Empty;
        public byte ChildrenCount { get; set; }
        public string InsuranceNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDeleted { get; set; }

        public byte GenderId { get; set; }
        public byte MaritalStatusId { get; set; }
        public byte? MilitaryStatusId { get; set; }
        public byte? ExemptionTypeId { get; set; }
        public byte? ExemptionReasonId { get; set; }
        public byte? ReligionId { get; set; }
        public byte? DenominationId { get; set; }

        public HierarchyId? IssueNode { get; set; }
        public HierarchyId? BirthNode { get; set; }
        public HierarchyId? ResidenceNode { get; set; }

        public Gender Gender { get; set; } = null!;
        public MaritalStatus MaritalStatus { get; set; } = null!;
        public MilitaryStatus? MilitaryStatus { get; set; }
        public Religion? Religion { get; set; }
        public ExemptionType? ExemptionType { get; set; }
        public ExemptionReason? ExemptionReason { get; set; }
        public EmployeeSupplement? Supplement { get; set; }
        public EmployeeSelfAssessment? SelfAssessment { get; set; }

        public ICollection<Education> Educations { get; set; } = new List<Education>();
        public ICollection<Course> Courses { get; set; } = new List<Course>();
        public ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
        public ICollection<PhoneNumber> PhoneNumbers { get; set; } = new List<PhoneNumber>();
        public ICollection<RelatedPerson> RelatedPersons { get; set; } = new List<RelatedPerson>();
        public ICollection<EmployeeLanguage> Languages { get; set; } = new List<EmployeeLanguage>();
        public ICollection<EmployeeSoftware> Softwares { get; set; } = new List<EmployeeSoftware>();
        public ICollection<EmploymentHistory> EmploymentHistories { get; set; } = new List<EmploymentHistory>();
        public ICollection<Citizenship> Citizenships { get; set; } = new List<Citizenship>();
        public ICollection<ForeignTravel> ForeignTravels { get; set; } = new List<ForeignTravel>();
        public ICollection<ForeignRelative> ForeignRelatives { get; set; } = new List<ForeignRelative>();
        public ICollection<ForeignCooperation> ForeignCooperations { get; set; } = new List<ForeignCooperation>();
        public ICollection<OrganizationCooperation> OrganizationCooperations { get; set; } = new List<OrganizationCooperation>();
        public ICollection<OtherActivity> OtherActivities { get; set; } = new List<OtherActivity>();
        public ICollection<SacrificeRecord> SacrificeRecords { get; set; } = new List<SacrificeRecord>();
        public ICollection<HiringRecord> HiringRecords { get; set; } = new List<HiringRecord>();
        public ICollection<PersonFile> PersonFiles { get; set; } = new List<PersonFile>();
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<BackgroundRecord> BackgroundRecords { get; set; } = new List<BackgroundRecord>();
        public ICollection<EmployeeAbility> Abilities { get; set; } = new List<EmployeeAbility>();
        public ICollection<EmployeeWorkInterest> WorkInterests { get; set; } = new List<EmployeeWorkInterest>();
    }

    public class Ability
    {
        public int AbilityId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class InterestLevel
    {
        public byte InterestLevelId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class EmployeeAbility
    {
        public Guid EmployeeAbilityId { get; set; }
        public Guid EmployeeId { get; set; }
        public int AbilityId { get; set; }
        public byte SkillLevelId { get; set; }
        public string? Description { get; set; }

        public Employee Employee { get; set; } = null!;
        public Ability Ability { get; set; } = null!;
        public SkillLevel SkillLevel { get; set; } = null!;
    }

    public class EmployeeWorkInterest
    {
        public Guid EmployeeWorkInterestId { get; set; }
        public Guid EmployeeId { get; set; }
        public byte InterestLevelId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public Employee Employee { get; set; } = null!;
        public InterestLevel InterestLevel { get; set; } = null!;
    }

    public class EmployeeSelfAssessment
    {
        public Guid EmployeeSelfAssessmentId { get; set; }
        public Guid EmployeeId { get; set; }
        public byte Discipline { get; set; }
        public byte Speed { get; set; }
        public byte Effort { get; set; }
        public byte Negotiation { get; set; }
        public byte Patience { get; set; }
        public byte Creativity { get; set; }
        public byte Punctuality { get; set; }
        public byte Teamwork { get; set; }
        public byte Accuracy { get; set; }
        public byte Planning { get; set; }
        public string? Description { get; set; }

        public Employee Employee { get; set; } = null!;
    }
}
