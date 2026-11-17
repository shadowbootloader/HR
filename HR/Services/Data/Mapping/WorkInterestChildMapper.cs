using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class WorkInterestChildMapper : IEmployeeChildMapper
    {
        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.WorkInterests == null) return;

            foreach (var item in profile.WorkInterests)
            {
                if (string.IsNullOrWhiteSpace(item.Title) || item.Title.Trim() == "—") continue;

                entity.WorkInterests.Add(new EmployeeWorkInterest
                {
                    EmployeeWorkInterestId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    InterestLevelId = await lookups.ByteFromTitleAsync<HR.Data.Entities.InterestLevel>(item.LevelTitle),
                    Title = item.Title.Trim(),
                    Description = null
                });
            }
        }
    }
}
