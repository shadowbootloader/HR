using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class JobHistoryChildMapper : IEmployeeChildMapper
    {
        private const string NoneItem = "—";

        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.JobHistories == null) return;

            foreach (var job in profile.JobHistories)
            {
                var reason = job.LeaveReason?.Trim();

                entity.EmploymentHistories.Add(new EmploymentHistory
                {
                    EmploymentHistoryId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    WorkplaceName = MapperHelpers.Required(job.Company, "نام موسسه"),
                    JobTitle = MapperHelpers.Required(job.JobTitle, "سمت"),
                    PhoneNumber = MapperHelpers.EmptyIfMissing(job.CompanyPhone),
                    StartDate = MapperHelpers.RequireDate(job.StartDate, "تاریخ شروع سابقه شغلی"),
                    EndDate = job.EndDate,
                    EndReasonId = string.IsNullOrWhiteSpace(reason) || reason == NoneItem
                        ? null
                        : await lookups.ByteFromTitleAsync<EmploymentEndReason>(reason),
                    Description = job.Note ?? string.Empty
                });
            }
        }
    }
}