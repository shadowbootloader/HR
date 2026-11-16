using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class AbilityChildMapper : IEmployeeChildMapper
    {
        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.Abilities == null) return;

            foreach (var item in profile.Abilities)
            {
                if (string.IsNullOrWhiteSpace(item.Title) || item.Title.Trim() == "—") continue;

                entity.Abilities.Add(new EmployeeAbility
                {
                    EmployeeAbilityId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    AbilityId = await lookups.IdFromTitleAsync<Ability>(item.Title),
                    SkillLevelId = await lookups.FromEnumAsync<HR.Data.Entities.SkillLevel, HR.Models.SkillLevel>(item.Level),
                    Description = null
                });
            }
        }
    }
}
