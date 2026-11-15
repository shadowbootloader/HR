using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class OrganCooperationChildMapper : IEmployeeChildMapper
    {
        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.OrganCollaborations == null) return;

            foreach (var record in profile.OrganCollaborations)
            {
                entity.OrganizationCooperations.Add(new OrganizationCooperation
                {
                    OrganizationCooperationId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    OrganizationName = MapperHelpers.Required(record.OrganName),
                    SectorTypeId = await lookups.ByteFromTitleAsync<SectorType>(record.OrganType),
                    Description = record.Note
                });
            }
        }
    }
}