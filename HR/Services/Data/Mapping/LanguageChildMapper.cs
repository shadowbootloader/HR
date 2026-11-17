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
                    ReadingLevelId = await lookups.ByteFromTitleAsync<HR.Data.Entities.SkillLevel>(language.ReadingTitle),
                    WritingLevelId = await lookups.ByteFromTitleAsync<HR.Data.Entities.SkillLevel>(language.WritingTitle),
                    ComprehensionLevelId = await lookups.ByteFromTitleAsync<HR.Data.Entities.SkillLevel>(language.ComprehensionTitle),
                    SpeakingLevelId = await lookups.ByteFromTitleAsync<HR.Data.Entities.SkillLevel>(language.SpeakingTitle),
                    Description = null
                });
            }
        }
    }
}