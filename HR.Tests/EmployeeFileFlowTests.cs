using HR.Data;
using HR.Data.Entities;
using HR.Services.Auth;
using HR.Services.Data;
using HR.Services.Files;
using Microsoft.EntityFrameworkCore;

namespace HR.Tests;

[Collection("sqlserver")]
public class EmployeeFileFlowTests
{
    private readonly SqlServerFixture _sql;

    public EmployeeFileFlowTests(SqlServerFixture sql) => _sql = sql;

    [SkippableFact]
    public async Task Create_StoresResumeAndIdCard_ReplaceAndRemove_DoNotDuplicate()
    {
        Skip.IfNot(_sql.IsAvailable, _sql.SkipReason ?? "SQL Server آماده نیست.");
        var storage = Path.Combine(Path.GetTempPath(), "hr-files-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storage);
        Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, storage);
        AuthSession.SignIn(new CurrentUser { Username = "files", DisplayName = "فایل", Roles = new[] { AppRoles.Hr } });
        try
        {
            await using var db = New();
            var insurance = await db.InsuranceTypes.Where(item => item.Title == "تامین اجتماعی").Select(item => item.InsuranceTypeId).SingleAsync();
            var cooperation = await db.CooperationTypes.Where(item => item.Title == "تمام‌وقت").Select(item => item.CooperationTypeId).SingleAsync();
            var resumeType = await db.FileTypes.Where(item => item.Title == "رزومه").Select(item => item.FileTypeId).SingleAsync();
            var cardType = await db.FileTypes.Where(item => item.Title == "کارت ملی").Select(item => item.FileTypeId).SingleAsync();
            var id = Guid.NewGuid();
            await new EmployeeService(db).CreateAsync(new Employee
            {
                EmployeeId = id,
                FirstName = "هستی",
                LastName = "فایل",
                FatherName = "رضا",
                NationalCode = "7788990019",
                BirthCertificateNumber = "31",
                BirthDate = new DateTime(1994, 4, 4),
                IssueDate = new DateTime(1994, 4, 5),
                Email = "file-" + id.ToString("N")[..4] + "@example.com",
                InsuranceNumber = "1234567890",
                GenderId = 2,
                MaritalStatusId = 1,
                Supplement = new EmployeeSupplement
                {
                    PositionTitle = "کارشناس",
                    PersonnelCode = "FL-" + id.ToString("N")[..6],
                    InsuranceTypeId = insurance,
                    CooperationTypeId = cooperation,
                    StartDate = new DateTime(2022, 1, 1),
                    Salary = 0
                },
                PersonFiles =
                {
                    new PersonFile { FileTypeId = resumeType, FileAddress = Stage(storage, "resume.pdf", new byte[] { 9, 9, 9 }) },
                    new PersonFile { FileTypeId = cardType, FileAddress = Stage(storage, "card.png", new byte[] { 1, 2, 3 }) }
                }
            });

            await using var read = New();
            var loaded = await new EmployeeService(read).GetByIdAsync(id);
            Assert.Equal(2, loaded!.PersonFiles.Count);
            Assert.Contains(loaded.PersonFiles, file => file.FileType.Title == "رزومه");
            Assert.Contains(loaded.PersonFiles, file => file.FileType.Title == "کارت ملی");
            Assert.Equal(2, await read.Documents.CountAsync(item => item.EmployeeId == id && !item.IsDeleted));
            foreach (var file in loaded.PersonFiles)
                Assert.True(File.Exists(new FileStorageService(storage).FullPath(file.FileAddress)));

            var resume = loaded.PersonFiles.Single(file => file.FileType.Title == "رزومه");
            loaded.PersonFiles.Clear();
            loaded.PersonFiles.Add(new PersonFile
            {
                FileTypeId = resumeType,
                FileAddress = Stage(storage, "resume2.pdf", new byte[] { 4, 5, 6, 7 })
            });
            await using (var update = New())
                await new EmployeeService(update).UpdateAsync(loaded);

            await using var after = New();
            var edited = await new EmployeeService(after).GetByIdAsync(id);
            var kept = Assert.Single(edited!.PersonFiles);
            Assert.Equal("رزومه", kept.FileType.Title);
            Assert.NotEqual(resume.FileAddress, kept.FileAddress);
            Assert.True(File.Exists(new FileStorageService(storage).FullPath(kept.FileAddress)));
            var liveNames = await after.Documents.Where(item => item.EmployeeId == id && !item.IsDeleted).Select(item => item.OriginalFileName).ToListAsync();
            Assert.Equal(new[] { "resume2.pdf" }, liveNames);
            Assert.Equal(2, await after.Documents.CountAsync(item => item.EmployeeId == id && item.IsDeleted));
        }
        finally
        {
            AuthSession.SignOut();
            Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, null);
            if (Directory.Exists(storage)) Directory.Delete(storage, recursive: true);
        }
    }

    private PersonnelDbContext New()
        => new(new DbContextOptionsBuilder<PersonnelDbContext>().UseSqlServer(_sql.PersonnelConnection, sql => sql.UseHierarchyId()).Options);

    private static string Stage(string storage, string name, byte[] bytes)
    {
        var folder = Path.Combine(Path.GetTempPath(), "hr-src-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var source = Path.Combine(folder, name);
        File.WriteAllBytes(source, bytes);
        var kind = name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? FileKind.Document : FileKind.Image;
        return new FileStorageService(storage).Stage(source, kind).RelativePath;
    }
}
