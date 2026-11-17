using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class CitizenshipChildMapper : IEmployeeChildMapper
    {
        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.Citizenships == null) return;

            foreach (var citizenship in profile.Citizenships)
            {
                entity.Citizenships.Add(new Citizenship
                {
                    CitizenshipId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    CountryId = await lookups.CountryAsync(citizenship.Country),
                    CitizenshipReasonId = await lookups.ByteFromTitleAsync<CitizenshipReason>(citizenship.Reason),
                    StartDate = MapperHelpers.RequireDate(citizenship.StartDate, "تاریخ شروع تابعیت"),
                    Description = citizenship.Note
                });
            }
        }
    }
}