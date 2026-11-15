using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class ForeignRelativeChildMapper : IEmployeeChildMapper
    {
        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.ForeignRelatives == null) return;

            foreach (var record in profile.ForeignRelatives)
            {
                var (first, last) = MapperHelpers.SplitName(record.FullName);

                entity.ForeignRelatives.Add(new ForeignRelative
                {
                    ForeignRelativeId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    FirstName = first,
                    LastName = last,
                    AbroadReasonId = await lookups.ByteFromTitleAsync<AbroadReason>(record.Reason),
                    StartDate = record.DepartureDate ?? DateTime.Today,
                    GeoLocation = await lookups.CountryNodeAsync(record.Country),
                    Description = record.Note
                });
            }
        }
    }
}