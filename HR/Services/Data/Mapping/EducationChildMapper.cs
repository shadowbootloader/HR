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
                    DegreeId = await lookups.ByteFromTitleAsync<EducationDegree>(edu.DegreeTitle),
                    StatusId = await lookups.ByteFromTitleAsync<EducationStatus>(edu.StatusTitle),
                    FieldOfStudy = MapperHelpers.Required(edu.Field, "رشته تحصیلی"),
                    InstitutionName = MapperHelpers.Required(edu.University, "موسسه آموزشی"),
                    GPA = edu.Gpa is double gpa
                        ? (decimal)gpa
                        : throw new InvalidOperationException("معدل الزامی است."),
                    StartDate = MapperHelpers.RequireDate(edu.StartDate, "تاریخ شروع تحصیل"),
                    EndDate = edu.EndDate,
                    ThesisTitle = edu.ThesisTitle,
                    EducationLocationNode = await lookups.EducationLocationAsync(edu.State, edu.City),
                    Description = null
                });
            }
        }
    }
}