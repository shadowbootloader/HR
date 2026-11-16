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
                    FirstName = MapperHelpers.Required(member.FirstName),
                    LastName = MapperHelpers.Required(member.LastName),
                    FatherName = MapperHelpers.Required(member.FatherName),
                    NationalCode = string.IsNullOrWhiteSpace(member.NationalCode)
                        ? MapperHelpers.SyntheticNationalCode()
                        : member.NationalCode.Trim(),
                    BirthCertificateNumber = "—",
                    BirthDate = DateTime.Now,
                    IssueDate = DateTime.Now,
                    JobTitle = string.IsNullOrWhiteSpace(member.Note) ? "—" : member.Note.Trim(),
                    Relationship = MapperHelpers.Required(member.Relation),
                    PersonRole = 0,
                    PhoneNumber = string.IsNullOrWhiteSpace(member.PhoneNumber) ? null : member.PhoneNumber.Trim(),
                    Address = string.IsNullOrWhiteSpace(member.Address) ? null : member.Address.Trim()
                });
            }

            await Task.CompletedTask;
        }
    }
}
