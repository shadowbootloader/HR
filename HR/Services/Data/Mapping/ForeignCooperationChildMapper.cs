using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class ForeignCooperationChildMapper : IEmployeeChildMapper
    {
        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.ForeignCooperations == null) return;

            foreach (var record in profile.ForeignCooperations)
            {
                var (first, last) = MapperHelpers.SplitName(record.PersonName);

                var person = new RelatedPerson
                {
                    RelatedPersonId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    FirstName = first,
                    LastName = last,
                    FatherName = "—",
                    NationalCode = MapperHelpers.SyntheticNationalCode(),
                    BirthCertificateNumber = "—",
                    BirthDate = DateTime.Today,
                    IssueDate = DateTime.Today,
                    JobTitle = "—",
                    Relationship = "همکار خارجی"
                };

                entity.RelatedPersons.Add(person);

                var start = record.StartDate ?? DateTime.Today;

                entity.ForeignCooperations.Add(new ForeignCooperation
                {
                    ForeignCooperationId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    RelatedPersonId = person.RelatedPersonId,
                    StartDate = start,
                    EndDate = record.EndDate,
                    CenterName = MapperHelpers.Required(record.InstitutionName),
                    CenterTypeId = await lookups.ByteFromTitleAsync<CenterType>(record.InstitutionType),
                    Description = MapperHelpers.JoinNote(record.Country, record.Note)
                });
            }
        }
    }
}