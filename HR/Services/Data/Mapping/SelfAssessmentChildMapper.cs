using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class SelfAssessmentChildMapper : IEmployeeChildMapper
    {
        public Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            var s = profile.SelfAssessment;
            if (s == null) return Task.CompletedTask;

            entity.SelfAssessment = new EmployeeSelfAssessment
            {
                EmployeeSelfAssessmentId = Guid.NewGuid(),
                EmployeeId = entity.EmployeeId,
                Discipline = Clamp(s.Discipline),
                Speed = Clamp(s.Speed),
                Effort = Clamp(s.Effort),
                Negotiation = Clamp(s.Negotiation),
                Patience = Clamp(s.Patience),
                Creativity = Clamp(s.Creativity),
                Punctuality = Clamp(s.Punctuality),
                Teamwork = Clamp(s.Teamwork),
                Accuracy = Clamp(s.Accuracy),
                Planning = Clamp(s.Planning)
            };

            return Task.CompletedTask;
        }

        private static byte Clamp(int v) => (byte)Math.Clamp(v, 0, 100);
    }
}
