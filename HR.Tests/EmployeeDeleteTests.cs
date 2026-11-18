using HR.Data;
using HR.Data.Entities;
using HR.Services.Auth;
using HR.Services.Data;
using HR.Services.Files;
using Microsoft.EntityFrameworkCore;

namespace HR.Tests;

[Collection("sqlserver")]
public class EmployeeDeleteTests
{
    private readonly SqlServerFixture _sql;

    public EmployeeDeleteTests(SqlServerFixture sql) => _sql = sql;

    [SkippableFact]
    public async Task SoftDelete_HidesFromRoster_AndHardDelete_ClearsRelatedRows()
    {
        Skip.IfNot(_sql.IsAvailable, _sql.SkipReason ?? "SQL Server آماده نیست.");
        var storage = Path.Combine(Path.GetTempPath(), "hr-del-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storage);
        Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, storage);
        SignIn(AppRoles.Hr);

        try
        {
            await using var db = NewContext();
            var before = await new DashboardService(db).GetAsync();
            var insurance = await db.InsuranceTypes.Where(item => item.Title == "تامین اجتماعی").Select(item => item.InsuranceTypeId).SingleAsync();
            var cooperation = await db.CooperationTypes.Where(item => item.Title == "تمام‌وقت").Select(item => item.CooperationTypeId).SingleAsync();
            var photoType = await db.FileTypes.Where(item => item.Title == "عکس پرسنلی").Select(item => item.FileTypeId).SingleAsync();
            var phoneType = await db.PhoneTypes.Select(item => item.PhoneTypeId).FirstAsync();
            var phoneStatus = await db.PhoneStatuses.Select(item => item.PhoneStatusId).FirstAsync();
            var department = await new DepartmentService(db).CreateAsync(new Department
            {
                Title = "حذف " + Guid.NewGuid().ToString("N")[..6],
                IsActive = true
            });

            var id = Guid.NewGuid();
            await new EmployeeService(db).CreateAsync(new Employee
            {
                EmployeeId = id,
                FirstName = "سارا",
                LastName = "حذف",
                FatherName = "رضا",
                NationalCode = "4455667784",
                BirthCertificateNumber = "77",
                BirthDate = new DateTime(1994, 5, 5),
                IssueDate = new DateTime(1994, 5, 6),
                Email = "del-" + id.ToString("N")[..6] + "@example.com",
                InsuranceNumber = "1234567890",
                GenderId = 2,
                MaritalStatusId = 1,
                Supplement = new EmployeeSupplement
                {
                    PositionTitle = "کارشناس",
                    PersonnelCode = "DL-" + id.ToString("N")[..6],
                    InsuranceTypeId = insurance,
                    CooperationTypeId = cooperation,
                    DepartmentId = department.DepartmentId,
                    StartDate = DateTime.Today.AddYears(-2),
                    Salary = 0
                },
                SelfAssessment = new EmployeeSelfAssessment
                {
                    Discipline = 10, Speed = 10, Effort = 10, Negotiation = 10, Patience = 10,
                    Creativity = 10, Punctuality = 10, Teamwork = 10, Accuracy = 10, Planning = 10
                },
                PhoneNumbers =
                {
                    new PhoneNumber
                    {
                        Number = "09120000000",
                        OwnerFirstName = "سارا",
                        OwnerLastName = "حذف",
                        PhoneNumberType = phoneType,
                        PhoneNumberStatus = phoneStatus
                    }
                },
                PersonFiles =
                {
                    new PersonFile { FileTypeId = photoType, FileAddress = Stage(storage, "photo.png") }
                }
            });

            await new LeaveService(db).RegisterAsync(id, "استحقاقی", DateTime.Today.AddDays(-20), DateTime.Today.AddDays(-10));
            var assessmentId = Guid.NewGuid();
            db.Assessments.Add(new Assessment
            {
                AssessmentId = assessmentId,
                Code = "del-" + id.ToString("N")[..8],
                Title = "آزمون حذف",
                Category = "عمومی",
                CreatedAt = DateTime.UtcNow
            });
            db.AssessmentAttempts.Add(new AssessmentAttempt
            {
                AttemptId = Guid.NewGuid(),
                AssessmentId = assessmentId,
                EmployeeId = id,
                StartedAt = DateTime.UtcNow,
                SubmittedAt = DateTime.UtcNow,
                TotalScore = 10
            });
            var managed = await db.Departments.SingleAsync(item => item.DepartmentId == department.DepartmentId);
            managed.ManagerEmployeeId = id;
            await db.SaveChangesAsync();

            var afterCreate = await new DashboardService(db).GetAsync();
            Assert.Equal(before.Headcount + 1, afterCreate.Headcount);

            SignIn(AppRoles.Viewer);
            await using (var blocked = NewContext())
            {
                var viewer = new EmployeeService(blocked);
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => viewer.SoftDeleteAsync(id));
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => viewer.HardDeleteAsync(id));
            }

            SignIn(AppRoles.Hr);
            await using (var softDb = NewContext())
                Assert.True(await new EmployeeService(softDb).SoftDeleteAsync(id));

            await using var afterSoft = NewContext();
            Assert.Equal(before.Headcount, (await new DashboardService(afterSoft).GetAsync()).Headcount);
            Assert.DoesNotContain(await new EmployeeService(afterSoft).GetAllAsync(), item => item.EmployeeId == id);

            await using (var hardDb = NewContext())
                Assert.True(await new EmployeeService(hardDb).HardDeleteAsync(id));

            await using var check = NewContext();
            Assert.Null(await new EmployeeService(check).GetByIdAsync(id));
            Assert.False(await check.PhoneNumbers.AnyAsync(item => item.EmployeeId == id));
            Assert.False(await check.PersonFiles.AnyAsync(item => item.EmployeeId == id));
            Assert.False(await check.Documents.AnyAsync(item => item.EmployeeId == id));
            Assert.False(await check.LeaveRecords.AnyAsync(item => item.EmployeeId == id));
            Assert.False(await check.AssessmentAttempts.AnyAsync(item => item.EmployeeId == id));
            Assert.False(await check.EmployeeSelfAssessments.AnyAsync(item => item.EmployeeId == id));
            Assert.Null(await check.Departments.Where(item => item.DepartmentId == department.DepartmentId).Select(item => item.ManagerEmployeeId).SingleAsync());
        }
        finally
        {
            AuthSession.SignOut();
            Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, null);
            if (Directory.Exists(storage))
                Directory.Delete(storage, recursive: true);
        }
    }

    private PersonnelDbContext NewContext()
        => new(new DbContextOptionsBuilder<PersonnelDbContext>()
            .UseSqlServer(_sql.PersonnelConnection, sql => sql.UseHierarchyId())
            .Options);

    private static void SignIn(string role)
        => AuthSession.SignIn(new CurrentUser
        {
            Username = "delete-" + role,
            DisplayName = "حذف",
            Roles = new[] { role }
        });

    private static string Stage(string storage, string name)
    {
        var folder = Path.Combine(Path.GetTempPath(), "hr-src-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var source = Path.Combine(folder, name);
        File.WriteAllBytes(source, new byte[] { 1, 2, 3, 4 });
        return new FileStorageService(storage).Stage(source, FileKind.Image).RelativePath;
    }
}
