using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class EducationChildMapper : IEmployeeChildMapper
    {
        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.Educations == null) return;

            foreach (var edu in profile.Educations)
            {
                entity.Educations.Add(new Education
                {
                    EducationId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    DegreeId = await lookups.FromEnumAsync<EducationDegree, DegreeLevel>(edu.Degree),
                    StatusId = await lookups.FromEnumAsync<EducationStatus, EducationStatusKind>(edu.Status),
                    FieldOfStudy = MapperHelpers.Required(edu.Field),
                    InstitutionName = MapperHelpers.Required(edu.University),
                    GPA = (decimal)(edu.Gpa ?? 0),
                    StartDate = edu.StartDate ?? DateTime.Today,
                    EndDate = edu.EndDate,
                    ThesisTitle = edu.ThesisTitle,
                    EducationLocationNode = await lookups.EducationLocationAsync(edu.State, edu.City),
                    Description = null
                });
            }
        }
    }
}