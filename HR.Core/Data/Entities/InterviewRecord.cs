using System;

namespace HR.Data.Entities
{
    public class InterviewRecord
    {
        public Guid InterviewId { get; set; }
        public Guid EmployeeId { get; set; }
        public DateTime HeldOn { get; set; }
        public string Interviewer { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? TranscriptPath { get; set; }
        public string? AudioPath { get; set; }

        public Employee Employee { get; set; } = null!;
    }
}
