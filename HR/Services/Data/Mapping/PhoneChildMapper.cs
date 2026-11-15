using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class PhoneChildMapper : IEmployeeChildMapper
    {
        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.Phones == null) return;

            foreach (var phone in profile.Phones)
            {
                entity.PhoneNumbers.Add(new PhoneNumber
                {
                    PhoneNumberId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    Number = phone.Number?.Trim() ?? string.Empty,
                    OwnerFirstName = MapperHelpers.Required(profile.FirstName),
                    OwnerLastName = MapperHelpers.Required(profile.LastName),
                    PhoneNumberType = await lookups.FromEnumAsync<PhoneType, PhoneKind>(phone.Kind),
                    PhoneNumberStatus = await lookups.ByteFromTitleAsync<PhoneStatus>("فعال"),
                    StartDate = phone.StartDate,
                    EndDate = phone.EndDate,
                    Description = phone.Note
                });
            }
        }
    }
}