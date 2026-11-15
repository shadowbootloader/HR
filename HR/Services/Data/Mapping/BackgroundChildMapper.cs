using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class BackgroundChildMapper : IEmployeeChildMapper
    {
        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.CriminalRecords == null) return;

            foreach (var record in profile.CriminalRecords)
            {
                entity.BackgroundRecords.Add(new BackgroundRecord
                {
                    BackgroundRecordId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    RecordTypeId = await lookups.ByteFromTitleAsync<RecordType>(record.RecordType),
                    Title = MapperHelpers.Required(record.Title),
                    Description = MapperHelpers.JoinNote(record.PersonName, record.Note)
                });
            }
        }
    }
}