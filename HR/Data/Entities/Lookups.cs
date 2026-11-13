namespace HR.Data.Entities
{
    public class Gender
    {
        public byte GenderId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class MaritalStatus
    {
        public byte MaritalStatusId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class MilitaryStatus
    {
        public byte MilitaryStatusId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class Religion
    {
        public byte ReligionId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class Denomination
    {
        public byte DenominationId { get; set; }
        public byte ReligionId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class EducationDegree
    {
        public byte DegreeId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class EducationStatus
    {
        public byte StatusId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class AccountTypeBank
    {
        public byte AccountTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class Bank
    {
        public short BankId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class InsuranceType
    {
        public byte InsuranceTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class CooperationType
    {
        public byte CooperationTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class SkillLevel
    {
        public byte SkillLevelId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class Language
    {
        public short LanguageId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class Software
    {
        public short SoftwareId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class PhoneType
    {
        public byte PhoneTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class PhoneStatus
    {
        public byte PhoneStatusId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class FileType
    {
        public byte FileTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class EmploymentEndReason
    {
        public byte EndReasonId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class ExemptionType
    {
        public byte ExemptionTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class ExemptionReason
    {
        public byte ExemptionReasonId { get; set; }
        public byte ExemptionTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class CitizenshipReason
    {
        public byte CitizenshipReasonId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class TransportType
    {
        public byte TransportTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class TravelReason
    {
        public byte TravelReasonId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class AbroadReason
    {
        public byte AbroadReasonId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class CenterType
    {
        public byte CenterTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class SectorType
    {
        public byte SectorTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class HiringResult
    {
        public byte HiringResultId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class RecordType
    {
        public byte RecordTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class SacrificeType
    {
        public byte SacrificeTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class SacrificeRelationship
    {
        public byte SacrificeRelationshipId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class AmountUnit
    {
        public byte AmountUnitId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class GeoType
    {
        public byte GeoTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
