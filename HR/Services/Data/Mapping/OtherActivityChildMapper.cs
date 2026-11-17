using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class OtherActivityChildMapper : IEmployeeChildMapper
    {
        public Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.OtherActivities != null)
            {
                foreach (var record in profile.OtherActivities)
                {
                    entity.OtherActivities.Add(new OtherActivity
                    {
                        OtherActivityId = Guid.NewGuid(),
                        EmployeeId = entity.EmployeeId,
                        Title = MapperHelpers.Required(record.Title, "عنوان فعالیت"),
                        ExecutionPlace = MapperHelpers.EmptyIfMissing(record.Place),
                        Description = record.Note
                    });
                }
            }

            return Task.CompletedTask;
        }
    }
}