using System;
using System.Collections.Generic;
using System.Linq;
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

            await LinkCertificates(entity, lookups, profile.Educations, entity.Educations,
                edu => edu.CertificateImagePath, (row, file) => row.CertificateFileId = file.PersonFileId, "مدرک تحصیلی");
            await LinkCertificates(entity, lookups, profile.Courses, entity.Courses,
                course => course.CertificateImagePath, (row, file) => row.CertificateFileId = file.PersonFileId, "مدرک دوره");
        }

        private static async Task LinkCertificates<TProfile, TEntity>(
            Employee entity,
            LookupResolver lookups,
            IEnumerable<TProfile>? profileRows,
            IEnumerable<TEntity> entityRows,
            Func<TProfile, string?> pathOf,
            Action<TEntity, PersonFile> assign,
            string fileTypeTitle)
        {
            if (profileRows == null) return;
            var left = profileRows.ToList();
            var right = entityRows.ToList();
            var count = Math.Min(left.Count, right.Count);
            for (var i = 0; i < count; i++)
            {
                var file = await AddIfAny(entity, lookups, pathOf(left[i]), fileTypeTitle);
                if (file != null) assign(right[i], file);
            }
        }

        private static async Task<PersonFile?> AddIfAny(Employee entity, LookupResolver lookups, string? path, string fileTypeTitle)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Trim() == "—") return null;

            var file = new PersonFile
            {
                PersonFileId = Guid.NewGuid(),
                EmployeeId = entity.EmployeeId,
                FileTypeId = await lookups.ByteFromTitleAsync<FileType>(fileTypeTitle),
                FileAddress = path.Trim(),
                FileSize = 0
            };
            entity.PersonFiles.Add(file);
            return file;
        }
    }
}
