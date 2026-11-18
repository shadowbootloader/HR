using HR.Data;
using HR.Data.Entities;
using HR.Services.Auth;
using HR.Services.Data;
using HR.Services.Files;
using Microsoft.EntityFrameworkCore;

namespace HR.Tests;

[Collection("sqlserver")]
public class InterviewTests
{
    private readonly SqlServerFixture _sql;

    public InterviewTests(SqlServerFixture sql) => _sql = sql;

    [SkippableFact]
    public async Task SaveInterview_StoresTranscriptAndAudio_AndReadsThemBack()
    {
        Skip.IfNot(_sql.IsAvailable, _sql.SkipReason ?? "SQL Server آماده نیست.");
        var storageRoot = Path.Combine(Path.GetTempPath(), "hr-int-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storageRoot);
        Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, storageRoot);
        AuthSession.SignIn(new CurrentUser { Username = "int", DisplayName = "مصاحبه", Roles = new[] { AppRoles.Hr } });
        try
        {
            await using var db = new PersonnelDbContext(new DbContextOptionsBuilder<PersonnelDbContext>().UseSqlServer(_sql.PersonnelConnection, sql => sql.UseHierarchyId()).Options);
            var insurance = await db.InsuranceTypes.Where(item => item.Title == "تامین اجتماعی").Select(item => item.InsuranceTypeId).SingleAsync();
            var cooperation = await db.CooperationTypes.Where(item => item.Title == "تمام‌وقت").Select(item => item.CooperationTypeId).SingleAsync();
            var id = Guid.NewGuid();
            await new EmployeeService(db).CreateAsync(new Employee
            {
                EmployeeId = id,
                FirstName = "مونا",
                LastName = "مصاحبه",
                FatherName = "رضا",
                NationalCode = "8899001121",
                BirthCertificateNumber = "41",
                BirthDate = new DateTime(1995, 5, 5),
                IssueDate = new DateTime(1995, 5, 6),
                Email = "int-" + id.ToString("N")[..4] + "@example.com",
                InsuranceNumber = "1234567890",
                GenderId = 2,
                MaritalStatusId = 1,
                Supplement = new EmployeeSupplement
                {
                    PositionTitle = "کارشناس",
                    PersonnelCode = "IN-" + id.ToString("N")[..6],
                    InsuranceTypeId = insurance,
                    CooperationTypeId = cooperation,
                    StartDate = new DateTime(2023, 1, 1),
                    Salary = 0
                }
            });

            var storage = new FileStorageService(storageRoot);
            var transcript = Stage(storage, "talk.txt", "متن مصاحبه"u8.ToArray(), FileKind.Document);
            var audio = Stage(storage, "talk.wav", new byte[] { 1, 2, 3, 4 }, FileKind.Audio);
            await new InterviewService(db, storage).ReplaceAsync(id, new[]
            {
                new InterviewDraft
                {
                    HeldOn = new DateTime(2026, 11, 1),
                    Interviewer = "سارا",
                    Notes = "نوبت اول",
                    TranscriptPath = transcript,
                    AudioPath = audio
                }
            });

            var rows = await new InterviewService(db, storage).ListAsync(id);
            var row = Assert.Single(rows);
            Assert.Equal("سارا", row.Interviewer);
            Assert.Equal("نوبت اول", row.Notes);
            Assert.True(File.Exists(storage.FullPath(row.TranscriptPath)));
            Assert.True(File.Exists(storage.FullPath(row.AudioPath)));
        }
        finally
        {
            AuthSession.SignOut();
            Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, null);
        }
    }

    private static string Stage(FileStorageService storage, string name, byte[] bytes, FileKind kind)
    {
        var folder = Path.Combine(Path.GetTempPath(), "hr-src-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var source = Path.Combine(folder, name);
        File.WriteAllBytes(source, bytes);
        return storage.Stage(source, kind).RelativePath;
    }
}
