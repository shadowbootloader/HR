using System;

namespace HR.Data.Entities
{
    public class AssessmentAssignment
    {
        public Guid AssignmentId { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid AssessmentId { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime AssignedAt { get; set; }
        public string? AssignedBy { get; set; }

        public Employee Employee { get; set; } = null!;
        public Assessment Assessment { get; set; } = null!;
    }
}
