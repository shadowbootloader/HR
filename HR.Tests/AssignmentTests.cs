using HR.Data;
using HR.Data.Entities;
using HR.Services.Assessments;
using HR.Services.Auth;
using HR.Services.Data;
using HR.Services.Files;
using Microsoft.EntityFrameworkCore;

namespace HR.Tests;

[Collection("sqlserver")]
public class AssignmentTests
{
    private readonly SqlServerFixture _sql;

    public AssignmentTests(SqlServerFixture sql) => _sql = sql;

    [SkippableFact]
    public async Task AssignedTest_StaysPending_UntilItIsSubmitted()
    {
        Skip.IfNot(_sql.IsAvailable, _sql.SkipReason ?? "SQL Server آماده نیست.");
        var storage = Path.Combine(Path.GetTempPath(), "hr-asg-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storage);
        Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, storage);
        AuthSession.SignIn(new CurrentUser { Username = "asg", DisplayName = "آزمون", Roles = new[] { AppRoles.Hr } });
        try
        {
            await using var db = new PersonnelDbContext(new DbContextOptionsBuilder<PersonnelDbContext>().UseSqlServer(_sql.PersonnelConnection, sql => sql.UseHierarchyId()).Options);
            var insurance = await db.InsuranceTypes.Where(item => item.Title == "تامین اجتماعی").Select(item => item.InsuranceTypeId).SingleAsync();
            var cooperation = await db.CooperationTypes.Where(item => item.Title == "تمام‌وقت").Select(item => item.CooperationTypeId).SingleAsync();
            var id = Guid.NewGuid();
            await new EmployeeService(db).CreateAsync(new Employee
            {
                EmployeeId = id,
                FirstName = "نیلوفر",
                LastName = "آزمون",
                FatherName = "رضا",
                NationalCode = "9900112237",
                BirthCertificateNumber = "51",
                BirthDate = new DateTime(1996, 6, 6),
                IssueDate = new DateTime(1996, 6, 7),
                Email = "asg-" + id.ToString("N")[..4] + "@example.com",
                InsuranceNumber = "1234567890",
                GenderId = 2,
                MaritalStatusId = 1,
                Supplement = new EmployeeSupplement
                {
                    PositionTitle = "کارشناس",
                    PersonnelCode = "AS-" + id.ToString("N")[..6],
                    InsuranceTypeId = insurance,
                    CooperationTypeId = cooperation,
                    StartDate = new DateTime(2024, 1, 1),
                    Salary = 0
                }
            });

            var service = new AssessmentService(db);
            await service.AssignAsync(id, "manager-eval", DateTime.Today.AddDays(-1));
            var pending = await service.PendingAsync(id);
            Assert.Contains(pending, item => item.AssessmentCode == "manager-eval" && item.Status == "Overdue");

            await service.SaveAttemptAsync(new AttemptSave
            {
                EmployeeId = id,
                AssessmentCode = "manager-eval",
                TotalScore = 80,
                LevelLabel = "قوی",
                SubmittedAt = DateTime.UtcNow,
                Answers = new List<AnswerSave>()
            });
            Assert.DoesNotContain(await service.PendingAsync(id), item => item.AssessmentCode == "manager-eval");
        }
        finally
        {
            AuthSession.SignOut();
            Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, null);
            if (Directory.Exists(storage)) Directory.Delete(storage, recursive: true);
        }
    }

    [SkippableFact]
    public async Task DeletedEmployee_DropsOutOfAssessmentLists()
    {
        Skip.IfNot(_sql.IsAvailable, _sql.SkipReason ?? "SQL Server آماده نیست.");
        var storage = Path.Combine(Path.GetTempPath(), "hr-del-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storage);
        Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, storage);
        AuthSession.SignIn(new CurrentUser { Username = "del", DisplayName = "حذف", Roles = new[] { AppRoles.Hr } });
        try
        {
            await using var db = new PersonnelDbContext(new DbContextOptionsBuilder<PersonnelDbContext>().UseSqlServer(_sql.PersonnelConnection, sql => sql.UseHierarchyId()).Options);
            var insurance = await db.InsuranceTypes.Where(item => item.Title == "تامین اجتماعی").Select(item => item.InsuranceTypeId).SingleAsync();
            var cooperation = await db.CooperationTypes.Where(item => item.Title == "تمام‌وقت").Select(item => item.CooperationTypeId).SingleAsync();
            var id = Guid.NewGuid();
            await new EmployeeService(db).CreateAsync(new Employee
            {
                EmployeeId = id,
                FirstName = "حذف‌شده",
                LastName = "آزمونی",
                FatherName = "رضا",
                NationalCode = "1011121311",
                BirthCertificateNumber = "52",
                BirthDate = new DateTime(1995, 3, 3),
                IssueDate = new DateTime(1995, 3, 4),
                Email = "del-" + id.ToString("N")[..4] + "@example.com",
                InsuranceNumber = "1234567890",
                GenderId = 2,
                MaritalStatusId = 1,
                Supplement = new EmployeeSupplement
                {
                    PositionTitle = "کارشناس",
                    PersonnelCode = "DL-" + id.ToString("N")[..6],
                    InsuranceTypeId = insurance,
                    CooperationTypeId = cooperation,
                    StartDate = new DateTime(2024, 1, 1),
                    Salary = 0
                }
            });

            var service = new AssessmentService(db);
            await service.AssignAsync(id, "disc-short", DateTime.Today.AddDays(-2));
            await service.SaveAttemptAsync(new AttemptSave
            {
                EmployeeId = id,
                AssessmentCode = "manager-eval",
                TotalScore = 70,
                LevelLabel = "قابل قبول",
                SubmittedAt = DateTime.UtcNow,
                Answers = new List<AnswerSave>()
            });
            Assert.True(await new EmployeeService(db).SoftDeleteAsync(id));

            Assert.Empty(await service.PendingAsync(id));
            Assert.Empty(await service.HistoryAsync(id));
            Assert.DoesNotContain(await service.RecentAsync(20), item => item.EmployeeId == id);

            var snapshot = await new DashboardService(db).GetAsync();
            Assert.DoesNotContain(snapshot.RecentAttempts, item => item.Subtitle.Contains("آزمونی"));
            Assert.DoesNotContain(snapshot.Issues, item => item.Subtitle.Contains("آزمونی") || item.Title.Contains("آزمونی"));
        }
        finally
        {
            AuthSession.SignOut();
            Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, null);
            if (Directory.Exists(storage)) Directory.Delete(storage, recursive: true);
        }
    }
}
