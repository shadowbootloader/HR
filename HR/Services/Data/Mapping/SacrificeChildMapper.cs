using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class SacrificeChildMapper : IEmployeeChildMapper
    {
        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.Sacrifices == null) return;

            foreach (var record in profile.Sacrifices)
            {
                entity.SacrificeRecords.Add(new HR.Data.Entities.SacrificeRecord
                {
                    SacrificeRecordId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    SacrificeRelationshipId = await lookups.ByteFromTitleAsync<SacrificeRelationship>(record.SacrificeRelationship),
                    SacrificeTypeId = await lookups.ByteFromTitleAsync<HR.Data.Entities.SacrificeType>(record.SacrificeType),
                    Amount = MapperHelpers.Decimal(record.Amount),
                    AmountUnitId = await lookups.ByteFromTitleAsync<AmountUnit>(record.AmountUnit),
                    Description = MapperHelpers.JoinNote(record.FullName, record.Note)
                });
            }
        }
    }
}