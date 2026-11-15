using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class JobHistoryChildMapper : IEmployeeChildMapper
    {
        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.JobHistories == null) return;

            foreach (var job in profile.JobHistories)
            {
                entity.EmploymentHistories.Add(new EmploymentHistory
                {
                    EmploymentHistoryId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    WorkplaceName = MapperHelpers.Required(job.Company),
                    JobTitle = MapperHelpers.Required(job.JobTitle),
                    PhoneNumber = MapperHelpers.Required(job.CompanyPhone),
                    StartDate = job.StartDate ?? DateTime.Today,
                    EndDate = job.EndDate,
                    Description = job.Note ?? job.LeaveReason ?? string.Empty
                });
            }
        }
    }
}