using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR.Data;
using HR.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.Services.Files
{
    /// <summary>
    /// بعد از ذخیره کارمند یا دپارتمان، فایل مرحله‌ای را به پوشه مالک می‌برد و ردیف Document می‌سازد.
    /// </summary>
    public sealed class DocumentLinkService
    {
        public const string Actor = "کاربر سامانه";
        public const string LogoTypeTitle = "لوگوی دپارتمان";

        private readonly FileStorageService _storage;

        public DocumentLinkService(FileStorageService? storage = null)
        {
            _storage = storage ?? FileStorageService.Load();
        }

        public async Task SyncEmployeeAsync(PersonnelDbContext db, Employee employee)
        {
            var documents = await db.Documents
                .Where(d => d.EmployeeId == employee.EmployeeId)
                .ToListAsync();

            var live = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in employee.PersonFiles)
            {
                if (string.IsNullOrWhiteSpace(file.FileAddress) || file.FileAddress.Trim() == "—")
                    continue;

                if (!System.IO.File.Exists(_storage.FullPath(file.FileAddress)))
                {
                    live.Add(Normalize(file.FileAddress));
                    continue;
                }

                var linked = await LinkAsync(
                    db,
                    documents,
                    file.FileAddress,
                    $"employees/{employee.EmployeeId:N}",
                    employee.EmployeeId,
                    departmentId: null,
                    await FileTypeTitleAsync(db, file.FileTypeId));

                if (linked == null) continue;
                file.FileAddress = linked.StoredRelativePath;
                file.FileSize = linked.SizeBytes;
                live.Add(linked.StoredRelativePath);
            }

            RetireMissing(documents, live);
        }

        public async Task SyncDepartmentLogoAsync(PersonnelDbContext db, Department department)
        {
            var documents = await db.Documents
                .Where(d => d.DepartmentId == department.DepartmentId)
                .ToListAsync();

            var live = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(department.LogoPath))
            {
                var linked = await LinkAsync(
                    db,
                    documents,
                    department.LogoPath,
                    $"departments/{department.DepartmentId}",
                    employeeId: null,
                    department.DepartmentId,
                    LogoTypeTitle);

                if (linked != null)
                {
                    department.LogoPath = linked.StoredRelativePath;
                    live.Add(linked.StoredRelativePath);
                }
            }

            RetireMissing(documents, live);
        }

        public async Task RemoveEmployeeAsync(PersonnelDbContext db, Guid employeeId)
        {
            var documents = await db.Documents.Where(d => d.EmployeeId == employeeId).ToListAsync();
            RemoveRows(db, documents);
        }

        public async Task RemoveDepartmentAsync(PersonnelDbContext db, short departmentId)
        {
            var documents = await db.Documents.Where(d => d.DepartmentId == departmentId).ToListAsync();
            RemoveRows(db, documents);
        }

        private async Task<Document?> LinkAsync(
            PersonnelDbContext db,
            List<Document> documents,
            string? storedPath,
            string ownerFolder,
            Guid? employeeId,
            short? departmentId,
            string typeTitle)
        {
            if (string.IsNullOrWhiteSpace(storedPath) || storedPath.Trim() == "—") return null;

            var full = _storage.FullPath(storedPath);
            if (!System.IO.File.Exists(full)) return null;

            var hash = FileStorageService.ComputeHash(full);
            var duplicatePath = FileStorageService.MatchingPath(
                documents.Select(d => (d.Sha256, d.StoredRelativePath, d.IsDeleted)),
                hash);

            if (duplicatePath != null)
            {
                if (!string.Equals(Normalize(storedPath), Normalize(duplicatePath), StringComparison.OrdinalIgnoreCase))
                    _storage.TryDelete(storedPath);
                return documents.First(d =>
                    !d.IsDeleted && string.Equals(d.Sha256, hash, StringComparison.OrdinalIgnoreCase));
            }

            var promoted = _storage.Promote(storedPath, ownerFolder);
            var typeId = await RequireTypeIdAsync(db, typeTitle);
            var document = new Document
            {
                DocumentId = Guid.NewGuid(),
                EmployeeId = employeeId,
                DepartmentId = departmentId,
                DocumentTypeId = typeId,
                OriginalFileName = promoted.OriginalFileName,
                StoredRelativePath = promoted.RelativePath,
                ContentType = promoted.ContentType,
                SizeBytes = promoted.SizeBytes,
                Sha256 = promoted.Sha256,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Actor,
                IsDeleted = false
            };
            db.Documents.Add(document);
            documents.Add(document);
            return document;
        }

        private void RetireMissing(List<Document> documents, HashSet<string> livePaths)
        {
            foreach (var document in documents.Where(d => !d.IsDeleted).ToList())
            {
                if (livePaths.Contains(document.StoredRelativePath)) continue;
                var stillUsed = documents.Any(d =>
                    d.DocumentId != document.DocumentId &&
                    !d.IsDeleted &&
                    string.Equals(d.StoredRelativePath, document.StoredRelativePath, StringComparison.OrdinalIgnoreCase));
                document.IsDeleted = true;
                if (!stillUsed)
                    _storage.TryDelete(document.StoredRelativePath);
            }
        }

        private void RemoveRows(PersonnelDbContext db, List<Document> documents)
        {
            var paths = documents
                .Select(d => d.StoredRelativePath)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            db.Documents.RemoveRange(documents);
            foreach (var path in paths)
                _storage.TryDelete(path);
        }

        private static async Task<string> FileTypeTitleAsync(PersonnelDbContext db, byte fileTypeId)
        {
            var title = await db.FileTypes.AsNoTracking()
                .Where(t => t.FileTypeId == fileTypeId)
                .Select(t => t.Title)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(title))
                throw new InvalidOperationException("نوع فایل شخص در پایگاه پیدا نشد.");

            return title;
        }

        private static async Task<byte> RequireTypeIdAsync(PersonnelDbContext db, string title)
        {
            var id = await db.DocumentTypes.AsNoTracking()
                .Where(t => t.Title == title)
                .Select(t => (byte?)t.DocumentTypeId)
                .FirstOrDefaultAsync();

            if (id == null)
                throw new InvalidOperationException(
                    $"نوع سند «{title}» در جدول DocumentType نیست. بلوک ارتقای LastPersonnel.sql را اجرا کنید.");

            return id.Value;
        }

        private static string Normalize(string path) => path.Replace('\\', '/').TrimStart('/');
    }
}
