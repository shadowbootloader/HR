using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class FamilyChildMapper : IEmployeeChildMapper
    {
        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.Family == null) return;

            foreach (var member in profile.Family)
            {
                entity.RelatedPersons.Add(new RelatedPerson
                {
                    RelatedPersonId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    FirstName = MapperHelpers.Required(member.FirstName, "نام بستگان"),
                    LastName = MapperHelpers.Required(member.LastName, "نام خانوادگی بستگان"),
                    FatherName = string.IsNullOrWhiteSpace(member.FatherName) ? null : member.FatherName.Trim(),
                    NationalCode = string.IsNullOrWhiteSpace(member.NationalCode) ? null : member.NationalCode.Trim(),
                    BirthCertificateNumber = null,
                    BirthDate = null,
                    IssueDate = null,
                    JobTitle = null,
                    Description = string.IsNullOrWhiteSpace(member.Note) ? null : member.Note.Trim(),
                    Relationship = MapperHelpers.Required(member.Relation, "نسبت"),
                    PersonRole = 0,
                    PhoneNumber = string.IsNullOrWhiteSpace(member.PhoneNumber) ? null : member.PhoneNumber.Trim(),
                    Address = string.IsNullOrWhiteSpace(member.Address) ? null : member.Address.Trim()
                });
            }

            await Task.CompletedTask;
        }
    }
}
