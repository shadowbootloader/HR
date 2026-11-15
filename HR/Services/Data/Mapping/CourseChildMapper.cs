using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class CourseChildMapper : IEmployeeChildMapper
    {
        public Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.Courses != null)
            {
                foreach (var course in profile.Courses)
                {
                    entity.Courses.Add(new Course
                    {
                        CourseId = Guid.NewGuid(),
                        EmployeeId = entity.EmployeeId,
                        Title = MapperHelpers.Required(course.Title),
                        InstitutionName = MapperHelpers.Required(course.Institute),
                        DurationHours = MapperHelpers.Duration(course.DurationText),
                        Description = course.Note
                    });
                }
            }

            return Task.CompletedTask;
        }
    }
}