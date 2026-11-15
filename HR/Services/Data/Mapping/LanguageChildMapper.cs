using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class LanguageChildMapper : IEmployeeChildMapper
    {
        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.Languages == null) return;

            foreach (var language in profile.Languages)
            {
                entity.Languages.Add(new EmployeeLanguage
                {
                    EmployeeLanguageId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    LanguageId = await lookups.IdFromTitleAsync<Language>(language.Language),
                    ReadingLevelId = await lookups.FromEnumAsync<SkillLevel, LanguageLevel>(language.Reading),
                    WritingLevelId = await lookups.FromEnumAsync<SkillLevel, LanguageLevel>(language.Writing),
                    ComprehensionLevelId = await lookups.FromEnumAsync<SkillLevel, LanguageLevel>(language.Comprehension),
                    SpeakingLevelId = await lookups.FromEnumAsync<SkillLevel, LanguageLevel>(language.Speaking),
                    Description = null
                });
            }
        }
    }
}