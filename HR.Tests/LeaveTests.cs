using HR.Data;
using HR.Data.Entities;
using HR.Services.Auth;
using HR.Services.Data;
using HR.Services.Files;
using Microsoft.EntityFrameworkCore;

namespace HR.Tests;

public class LeaveTests
{
    [Fact]
    public void Close_CancelsALeaveThatHasNotStarted_AndShortensAnOpenOne()
    {
        var today = new DateTime(2026, 11, 17);
        var future = LeaveService.Close(today.AddDays(2), today.AddDays(5), today);
        Assert.True(future.IsCancelled);

        var open = LeaveService.Close(today.AddDays(-3), today.AddDays(4), today);
        Assert.False(open.IsCancelled);
        Assert.Equal(today.AddDays(-1), open.EndDate);
        Assert.False(LeaveService.Covers(new LeaveRecord
        {
            StartDate = today.AddDays(-3),
            EndDate = open.EndDate,
            IsCancelled = false
        }, today));
    }
}

[Collection("sqlserver")]
public class LeaveFlowTests
{
    private readonly SqlServerFixture _sql;

    public LeaveFlowTests(SqlServerFixture sql) => _sql = sql;

    [SkippableFact]
    public async Task RegisterAndEndLeave_ChangesRosterStatus_AndViewerCannotRegister()
    {
        Skip.IfNot(_sql.IsAvailable, _sql.SkipReason ?? "SQL Server آماده نیست.");
        var storage = Path.Combine(Path.GetTempPath(), "hr-leave-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storage);
        Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, storage);
        AuthSession.SignIn(new CurrentUser
        {
            Username = "leave-hr",
            DisplayName = "مرخصی",
            Roles = new[] { AppRoles.Hr }
        });

        try
        {
            await using var db = new PersonnelDbContext(
                new DbContextOptionsBuilder<PersonnelDbContext>()
                    .UseSqlServer(_sql.PersonnelConnection, sql => sql.UseHierarchyId())
                    .Options);
            var insurance = await db.InsuranceTypes.Where(item => item.Title == "تامین اجتماعی").Select(item => item.InsuranceTypeId).SingleAsync();
            var cooperation = await db.CooperationTypes.Where(item => item.Title == "تمام‌وقت").Select(item => item.CooperationTypeId).SingleAsync();
            var department = await new DepartmentService(db).CreateAsync(new Department
            {
                Title = "مرخصی " + Guid.NewGuid().ToString("N")[..6],
                IsActive = true
            });
            var today = DateTime.Today;
            var employee = await new EmployeeService(db).CreateAsync(new Employee
            {
                FirstName = "لیلا",
                LastName = "مرخصی",
                FatherName = "رضا",
                NationalCode = "0084572914",
                BirthCertificateNumber = "9",
                BirthDate = new DateTime(1991, 3, 3),
                IssueDate = new DateTime(1991, 3, 4),
                Email = "leave@example.com",
                InsuranceNumber = "1234567890",
                GenderId = 2,
                MaritalStatusId = 1,
                Supplement = new EmployeeSupplement
                {
                    PositionTitle = "کارشناس",
                    PersonnelCode = "L-" + Guid.NewGuid().ToString("N")[..6],
                    InsuranceTypeId = insurance,
                    CooperationTypeId = cooperation,
                    DepartmentId = department.DepartmentId,
                    StartDate = today.AddMonths(-2),
                    Salary = 0
                }
            });

            var leaves = new LeaveService(db);
            var created = await leaves.RegisterAsync(employee.EmployeeId, "استحقاقی", today, today.AddDays(3));
            var snapshot = await new DashboardService(db).GetAsync(today);
            Assert.True(snapshot.ByStatus.Single(item => item.Name == RosterStatus.OnLeave).Count >= 1);

            var listed = await new EmployeeService(db).GetAllAsync(includeDeleted: true);
            var row = listed.Single(item => item.EmployeeId == employee.EmployeeId);
            Assert.Contains(row.Leaves, leave => LeaveService.Covers(leave, today));

            AuthSession.SignIn(new CurrentUser { Username = "view", DisplayName = "بیننده", Roles = new[] { AppRoles.Viewer } });
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                new LeaveService(db).RegisterAsync(employee.EmployeeId, "استعلاجی", today, today));

            AuthSession.SignIn(new CurrentUser { Username = "leave-hr", DisplayName = "مرخصی", Roles = new[] { AppRoles.Hr } });
            await leaves.EndAsync(created.LeaveId, today);
            var after = await db.LeaveRecords.SingleAsync(item => item.LeaveId == created.LeaveId);
            Assert.False(LeaveService.Covers(after, today));
        }
        finally
        {
            AuthSession.SignOut();
            Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, null);
            if (Directory.Exists(storage))
                Directory.Delete(storage, recursive: true);
        }
    }
}
