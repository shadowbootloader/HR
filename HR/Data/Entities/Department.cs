using System.Collections.Generic;

namespace HR.Data.Entities
{
    public class Department
    {
        public short DepartmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public short? ParentDepartment { get; set; }

        // Navigation
        public Department? Parent { get; set; }
        public ICollection<Department> Children { get; set; } = new List<Department>();
        public ICollection<EmployeeSupplement> Employees { get; set; } = new List<EmployeeSupplement>();
    }
}
