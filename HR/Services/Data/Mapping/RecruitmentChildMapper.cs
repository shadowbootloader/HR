using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class RecruitmentChildMapper : IEmployeeChildMapper
    {
        private const string NoneItem = "—";

        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.Recruitments == null) return;

            foreach (var record in profile.Recruitments)
            {
                var result = record.Result?.Trim();

                entity.HiringRecords.Add(new HiringRecord
                {
                    HiringRecordId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    SectorTypeId = await lookups.ByteFromTitleAsync<SectorType>(record.InstitutionType),
                    OrganizationName = MapperHelpers.Required(record.InstitutionName),
                    HiringResultId = string.IsNullOrWhiteSpace(result) || result == NoneItem
                        ? null
                        : await lookups.ByteFromTitleAsync<HiringResult>(result),
                    Description = record.Note
                });
            }
        }
    }
}