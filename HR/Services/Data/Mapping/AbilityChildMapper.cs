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
                    AbilityId = checked((int)await lookups.IdFromUserCatalogAsync<Ability>(item.Title)),
                    SkillLevelId = await lookups.ByteFromTitleAsync<HR.Data.Entities.SkillLevel>(item.LevelTitle),
                    Description = null
                });
            }
        }
    }
}
