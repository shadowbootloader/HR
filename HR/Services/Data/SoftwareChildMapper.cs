using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class SoftwareChildMapper : IEmployeeChildMapper
    {
        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.SoftwareSkills == null) return;

            foreach (var software in profile.SoftwareSkills)
            {
                entity.Softwares.Add(new EmployeeSoftware
                {
                    EmployeeSoftwareId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    SoftwareId = await lookups.IdFromTitleAsync<Software>(software.SoftwareName),
                    SkillLevelId = await lookups.FromEnumAsync<HR.Data.Entities.SkillLevel, HR.Models.SkillLevel>(software.Level),
                    Description = software.Note
                });
            }
        }
    }
}