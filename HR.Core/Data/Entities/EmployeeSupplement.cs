using System;

namespace HR.Data.Entities
{
    public class EmployeeSupplement
    {
        public Guid EmployeeSupplementId { get; set; }
        public Guid EmployeeId { get; set; }

        public string PositionTitle { get; set; } = string.Empty;
        public byte InsuranceTypeId { get; set; }
        public byte CooperationTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Salary { get; set; }
        public Guid? BankAccountId { get; set; }
        public Guid? ResumeFileId { get; set; }
        public string PersonnelCode { get; set; } = string.Empty;
        public short? DepartmentId { get; set; }
        public Guid? DirectManagerEmployeeId { get; set; }

        // Navigation
        public Employee Employee { get; set; } = null!;
        public Department? Department { get; set; }
        public Employee? DirectManager { get; set; }
        public InsuranceType InsuranceType { get; set; } = null!;
        public CooperationType CooperationType { get; set; } = null!;
        public BankAccount? BankAccount { get; set; }
        public PersonFile? ResumeFile { get; set; }
    }
}
