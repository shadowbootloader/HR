using System;
using Microsoft.EntityFrameworkCore;

namespace HR.Data.Entities
{
    public class Education
    {
        public Guid EducationId { get; set; }
        public Guid EmployeeId { get; set; }
        public byte StatusId { get; set; }
        public byte DegreeId { get; set; }
        public string FieldOfStudy { get; set; } = string.Empty;
        public string InstitutionName { get; set; } = string.Empty;
        public decimal GPA { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ThesisTitle { get; set; }
        public string? Description { get; set; }
        public Microsoft.EntityFrameworkCore.HierarchyId? EducationLocationNode { get; set; }
        public Guid? CertificateFileId { get; set; }

        public Employee Employee { get; set; } = null!;
        public EducationStatus Status { get; set; } = null!;
        public EducationDegree Degree { get; set; } = null!;
        public PersonFile? CertificateFile { get; set; }
    }

    public class Course
    {
        public Guid CourseId { get; set; }
        public Guid EmployeeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string InstitutionName { get; set; } = string.Empty;
        public int DurationHours { get; set; }
        public string? Description { get; set; }
        public Guid? CertificateFileId { get; set; }

        public Employee Employee { get; set; } = null!;
        public PersonFile? CertificateFile { get; set; }
    }

    public class BankAccount
    {
        public Guid BankAccountId { get; set; }
        public Guid EmployeeId { get; set; }
        public short BankId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string Sheba { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public byte AccountTypeId { get; set; }
        public string? Description { get; set; }

        public Employee Employee { get; set; } = null!;
        public Bank Bank { get; set; } = null!;
        public AccountTypeBank AccountType { get; set; } = null!;
    }

    public class PhoneNumber
    {
        public Guid PhoneNumberId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? RelatedPersonId { get; set; }
        public string Number { get; set; } = string.Empty;
        public string OwnerFirstName { get; set; } = string.Empty;
        public string OwnerLastName { get; set; } = string.Empty;
        public byte PhoneNumberStatus { get; set; }
        public byte PhoneNumberType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }

        public Employee? Employee { get; set; }
        public RelatedPerson? RelatedPerson { get; set; }
        public PhoneStatus Status { get; set; } = null!;
        public PhoneType Type { get; set; } = null!;
    }

    public class RelatedPerson
    {
        public Guid RelatedPersonId { get; set; }
        public Guid EmployeeId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string NationalCode { get; set; } = string.Empty;
        public string BirthCertificateNumber { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public DateTime IssueDate { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        /// <summary>۰ = بستگان، ۱ = همکاری خارجی</summary>
        public byte PersonRole { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public HierarchyId? IssueCity { get; set; }
        public HierarchyId? BirthCity { get; set; }

        public Employee Employee { get; set; } = null!;
    }

    public class EmployeeLanguage
    {
        public Guid EmployeeLanguageId { get; set; }
        public Guid EmployeeId { get; set; }
        public short LanguageId { get; set; }
        public byte ReadingLevelId { get; set; }
        public byte WritingLevelId { get; set; }
        public byte ComprehensionLevelId { get; set; }
        public byte SpeakingLevelId { get; set; }
        public string? Description { get; set; }
        public Guid? CertificateFileId { get; set; }

        public Employee Employee { get; set; } = null!;
        public Language Language { get; set; } = null!;
        public SkillLevel ReadingLevel { get; set; } = null!;
        public SkillLevel WritingLevel { get; set; } = null!;
        public SkillLevel ComprehensionLevel { get; set; } = null!;
        public SkillLevel SpeakingLevel { get; set; } = null!;
        public PersonFile? CertificateFile { get; set; }
    }

    public class EmployeeSoftware
    {
        public Guid EmployeeSoftwareId { get; set; }
        public Guid EmployeeId { get; set; }
        public short SoftwareId { get; set; }
        public byte SkillLevelId { get; set; }
        public string? Description { get; set; }
        public Guid? CertificateFileId { get; set; }

        public Employee Employee { get; set; } = null!;
        public Software Software { get; set; } = null!;
        public SkillLevel SkillLevel { get; set; } = null!;
        public PersonFile? CertificateFile { get; set; }
    }

    public class EmploymentHistory
    {
        public Guid EmploymentHistoryId { get; set; }
        public Guid EmployeeId { get; set; }
        public string WorkplaceName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public byte? EndReasonId { get; set; }
        public string Description { get; set; } = string.Empty;

        public Employee Employee { get; set; } = null!;
        public EmploymentEndReason? EndReason { get; set; }
    }

    public class Citizenship
    {
        public Guid CitizenshipId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? RelatedPersonId { get; set; }
        public int CountryId { get; set; }
        public byte CitizenshipReasonId { get; set; }
        public DateTime StartDate { get; set; }
        public string? Description { get; set; }

        public Employee? Employee { get; set; }
        public RelatedPerson? RelatedPerson { get; set; }
        public CitizenshipReason Reason { get; set; } = null!;
    }

    public class ForeignTravel
    {
        public Guid ForeignTravelId { get; set; }
        public Guid EmployeeId { get; set; }
        public byte TravelReasonId { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public byte TransportTypeId { get; set; }
        public string? Description { get; set; }
        public HierarchyId? GeoOrigin { get; set; }
        public HierarchyId? GeoDestination { get; set; }

        public Employee Employee { get; set; } = null!;
        public TravelReason TravelReason { get; set; } = null!;
        public TransportType TransportType { get; set; } = null!;
    }

    public class ForeignRelative
    {
        public Guid ForeignRelativeId { get; set; }
        public Guid EmployeeId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public byte AbroadReasonId { get; set; }
        public DateTime StartDate { get; set; }
        public string? Description { get; set; }
        public HierarchyId? GeoLocation { get; set; }

        public Employee Employee { get; set; } = null!;
        public AbroadReason AbroadReason { get; set; } = null!;
    }

    public class ForeignCooperation
    {
        public Guid ForeignCooperationId { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid RelatedPersonId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public byte CenterTypeId { get; set; }
        public string? Description { get; set; }

        public Employee Employee { get; set; } = null!;
        public RelatedPerson RelatedPerson { get; set; } = null!;
        public CenterType CenterType { get; set; } = null!;
    }

    public class OrganizationCooperation
    {
        public Guid OrganizationCooperationId { get; set; }
        public Guid EmployeeId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public byte SectorTypeId { get; set; }
        public string? Description { get; set; }

        public Employee Employee { get; set; } = null!;
        public SectorType SectorType { get; set; } = null!;
    }

    public class OtherActivity
    {
        public Guid OtherActivityId { get; set; }
        public Guid EmployeeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ExecutionPlace { get; set; } = string.Empty;
        public string? Description { get; set; }

        public Employee Employee { get; set; } = null!;
    }

    public class SacrificeRecord
    {
        public Guid SacrificeRecordId { get; set; }
        public Guid EmployeeId { get; set; }
        public byte SacrificeRelationshipId { get; set; }
        public byte SacrificeTypeId { get; set; }
        public decimal Amount { get; set; }
        public byte AmountUnitId { get; set; }
        public string? Description { get; set; }
        public Guid? RelatedPersonId { get; set; }

        public Employee Employee { get; set; } = null!;
        public SacrificeRelationship Relationship { get; set; } = null!;
        public SacrificeType Type { get; set; } = null!;
        public AmountUnit AmountUnit { get; set; } = null!;
    }

    public class HiringRecord
    {
        public Guid HiringRecordId { get; set; }
        public Guid EmployeeId { get; set; }
        public byte SectorTypeId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public byte? HiringResultId { get; set; }
        public string? Description { get; set; }

        public Employee Employee { get; set; } = null!;
        public SectorType SectorType { get; set; } = null!;
        public HiringResult? Result { get; set; }
    }

    public class PersonFile
    {
        public Guid PersonFileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? RelatedPersonId { get; set; }
        public byte FileTypeId { get; set; }
        public string FileAddress { get; set; } = string.Empty;
        public long FileSize { get; set; }

        public Employee? Employee { get; set; }
        public RelatedPerson? RelatedPerson { get; set; }
        public FileType FileType { get; set; } = null!;
    }

    public class Address
    {
        public Guid AddressId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? RelatedPersonId { get; set; }
        public string? PostalCode { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AddressText { get; set; } = string.Empty;

        public Employee? Employee { get; set; }
        public RelatedPerson? RelatedPerson { get; set; }
    }

    public class BackgroundRecord
    {
        public Guid BackgroundRecordId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? RelatedPersonId { get; set; }
        public byte RecordTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public Employee? Employee { get; set; }
        public RelatedPerson? RelatedPerson { get; set; }
        public RecordType RecordType { get; set; } = null!;
    }
}
