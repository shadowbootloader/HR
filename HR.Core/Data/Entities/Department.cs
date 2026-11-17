using System;
using System.Collections.Generic;

namespace HR.Data.Entities
{
    public class Department
    {
        public short DepartmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public short? ParentDepartmentId { get; set; }

        public bool IsActive { get; set; } = true;
        public string? SecurityLevel { get; set; }
        public DateTime? EstablishedDate { get; set; }
        public int? ApprovedCapacity { get; set; }
        public string? LogoPath { get; set; }

        public Guid? ManagerEmployeeId { get; set; }
        public Guid? DeputyEmployeeId { get; set; }
        public Guid? CashCustodianEmployeeId { get; set; }

        public string? CostCenterCode { get; set; }
        public decimal? AnnualBudget { get; set; }
        public decimal? PettyCashLimit { get; set; }
        public string? Description { get; set; }

        // Navigation
        public Department? Parent { get; set; }
        public ICollection<Department> Children { get; set; } = new List<Department>();
        public ICollection<EmployeeSupplement> Employees { get; set; } = new List<EmployeeSupplement>();
        public ICollection<DepartmentOffice> Offices { get; set; } = new List<DepartmentOffice>();

        public Employee? Manager { get; set; }
        public Employee? Deputy { get; set; }
        public Employee? CashCustodian { get; set; }
    }

    public class DepartmentOffice
    {
        public Guid OfficeId { get; set; }
        public short DepartmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Address { get; set; }

        public Department Department { get; set; } = null!;
    }
}
