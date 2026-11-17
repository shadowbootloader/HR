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
                    OwnerFirstName = MapperHelpers.Required(profile.FirstName, "نام"),
                    OwnerLastName = MapperHelpers.Required(profile.LastName, "نام خانوادگی"),
                    PhoneNumberType = await lookups.ByteFromTitleAsync<PhoneType>(phone.KindTitle),
                    PhoneNumberStatus = lookups.MatchTitle<PhoneStatus>(new[] { "فعال" }, "وضعیت شماره تلفن"),
                    StartDate = phone.StartDate,
                    EndDate = phone.EndDate,
                    Description = phone.Note
                });
            }
        }
    }
}