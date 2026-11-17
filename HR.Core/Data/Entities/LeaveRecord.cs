using System;

namespace HR.Data.Entities
{
    public class LeaveRecord
    {
        public Guid LeaveId { get; set; }
        public Guid EmployeeId { get; set; }
        public string LeaveType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        public Employee Employee { get; set; } = null!;
    }
}
