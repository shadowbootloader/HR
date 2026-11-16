using System;
using System.IO;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class PersonFileChildMapper : IEmployeeChildMapper
    {
        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            await AddIfAny(entity, lookups, profile.PhotoPath, "عکس پرسنلی");
            await AddIfAny(entity, lookups, profile.NationalCardImagePath, "کارت ملی");
            await AddIfAny(entity, lookups, profile.IdCardImagePath, "شناسنامه");
            await AddIfAny(entity, lookups, profile.MilitaryImagePath, "پایان خدمت");
            await AddIfAny(entity, lookups, profile.Supplementary?.ResumePath, "رزومه");

            if (profile.Educations != null)
            {
                foreach (var edu in profile.Educations)
                    await AddIfAny(entity, lookups, edu.CertificateImagePath, "مدرک تحصیلی");
            }
        }

        private static async Task AddIfAny(Employee entity, LookupResolver lookups, string? path, string fileTypeTitle)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Trim() == "—") return;

            long size = 0;
            try
            {
                if (File.Exists(path))
                    size = new FileInfo(path).Length;
            }
            catch { /* ignore */ }

            entity.PersonFiles.Add(new PersonFile
            {
                PersonFileId = Guid.NewGuid(),
                EmployeeId = entity.EmployeeId,
                FileTypeId = await lookups.ByteFromTitleAsync<FileType>(fileTypeTitle),
                FileAddress = path.Trim(),
                FileSize = size
            });
        }
    }
}
