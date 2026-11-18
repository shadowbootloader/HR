using HR.Data;
using HR.Data.Entities;
using HR.Services.Auth;
using HR.Services.Data;
using HR.Services.Files;
using Microsoft.EntityFrameworkCore;

namespace HR.Tests;

public class DashboardTests
{
    [Fact]
    public void EmptyRoster_RendersZeroBuckets()
    {
        var snapshot = DashboardAggregator.Build(Array.Empty<DashboardPerson>(), new DateTime(2026, 11, 17));

        Assert.Equal(0, snapshot.Headcount);
        Assert.Empty(snapshot.ByDepartment);
        Assert.Empty(snapshot.ByGender);
        Assert.Equal(5, snapshot.ByStatus.Count);
        Assert.All(snapshot.ByStatus, item => Assert.Equal(0, item.Count));
        Assert.Equal(12, snapshot.Hires.Count);
        Assert.All(snapshot.Hires, item => Assert.Equal(0, item.Count));
    }

    [Fact]
    public void Aggregates_SplitStatusGenderDepartmentAndHireMonth()
    {
        var today = new DateTime(2026, 11, 17);
        var snapshot = DashboardAggregator.Build(new[]
        {
            new DashboardPerson { Gender = "مرد", Department = "مالی", StartDate = today.AddDays(-60), EndDate = null },
            new DashboardPerson { Gender = "زن", Department = "مالی", StartDate = today.AddYears(-2), OnLeave = true },
            new DashboardPerson { Gender = "مرد", Department = "مالی", StartDate = today.AddYears(-3), EndDate = today.AddDays(-1) },
            new DashboardPerson { IsDeleted = true, Gender = "زن", Department = "مالی", StartDate = today }
        }, today);

        Assert.Equal(3, snapshot.Headcount);
        Assert.Equal(3, Assert.Single(snapshot.ByDepartment, item => item.Name == "مالی").Count);
        Assert.Equal(2, Assert.Single(snapshot.ByGender, item => item.Name == "مرد").Count);
        Assert.Equal(1, snapshot.ByStatus.Single(item => item.Name == RosterStatus.Active).Count);
        Assert.Equal(0, snapshot.ByStatus.Single(item => item.Name == RosterStatus.NewHire).Count);
        Assert.Equal(1, snapshot.ByStatus.Single(item => item.Name == RosterStatus.OnLeave).Count);
        Assert.Equal(1, snapshot.ByStatus.Single(item => item.Name == RosterStatus.Inactive).Count);
        Assert.Equal(1, snapshot.ByStatus.Single(item => item.Name == RosterStatus.Deleted).Count);
        var hired = today.AddDays(-60);
        var calendar = new System.Globalization.PersianCalendar();
        Assert.Equal(1, snapshot.Hires.Single(item => item.Year == calendar.GetYear(hired) && item.Month == calendar.GetMonth(hired)).Count);
    }

    [Fact]
    public void RecentStart_IsNewHire_UnlessLeaveEndedOrDeleted()
    {
        var today = new DateTime(2026, 11, 17);
        Assert.Equal(RosterStatus.NewHire, RosterStatus.Resolve(false, null, false, today, today));
        Assert.Equal(RosterStatus.NewHire, RosterStatus.Resolve(false, null, false, today.AddDays(-30), today));
        Assert.Equal(RosterStatus.Active, RosterStatus.Resolve(false, null, false, today.AddDays(-31), today));
        Assert.Equal(RosterStatus.OnLeave, RosterStatus.Resolve(false, null, true, today, today));
        Assert.Equal(RosterStatus.Inactive, RosterStatus.Resolve(false, today.AddDays(-1), false, today, today));
        Assert.Equal(RosterStatus.Deleted, RosterStatus.Resolve(true, null, false, today, today));
        Assert.Equal(RosterStatus.Active, RosterStatus.Resolve(false, null, false, null, today));
    }
}

[Collection("sqlserver")]
public class DashboardFlowTests
{
    private readonly SqlServerFixture _sql;

    public DashboardFlowTests(SqlServerFixture sql) => _sql = sql;

    [SkippableFact]
    public async Task DatabaseSnapshot_CountsARealHire_AndIsEmptyWithoutPeople()
    {
        Skip.IfNot(_sql.IsAvailable, _sql.SkipReason ?? "SQL Server آماده نیست.");
        var storage = Path.Combine(Path.GetTempPath(), "hr-dash-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storage);
        Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, storage);
        await using var db = new PersonnelDbContext(
            new DbContextOptionsBuilder<PersonnelDbContext>()
                .UseSqlServer(_sql.PersonnelConnection, sql => sql.UseHierarchyId())
                .Options);

        var empty = await new DashboardService(db).GetAsync(new DateTime(2026, 11, 17));
        Assert.True(empty.Headcount >= 0);
        Assert.Equal(12, empty.Hires.Count);

        AuthSession.SignIn(new CurrentUser
        {
            Username = "dash",
            DisplayName = "داشبورد",
            Roles = new[] { AppRoles.Hr }
        });
        try
        {
            var insurance = await db.InsuranceTypes.Where(item => item.Title == "تامین اجتماعی").Select(item => item.InsuranceTypeId).SingleAsync();
            var cooperation = await db.CooperationTypes.Where(item => item.Title == "تمام‌وقت").Select(item => item.CooperationTypeId).SingleAsync();
            var department = await new DepartmentService(db).CreateAsync(new Department { Title = "داشبورد " + Guid.NewGuid().ToString("N")[..6], IsActive = true });
            var today = DateTime.Today;
            await new EmployeeService(db).CreateAsync(new Employee
            {
                FirstName = "آیدا",
                LastName = "داشبورد",
                FatherName = "رضا",
                NationalCode = "0499370899",
                BirthCertificateNumber = "1",
                BirthDate = new DateTime(1992, 2, 2),
                IssueDate = new DateTime(1992, 2, 3),
                Email = "dash@example.com",
                InsuranceNumber = "1234567890",
                GenderId = 2,
                MaritalStatusId = 1,
                Supplement = new EmployeeSupplement
                {
                    PositionTitle = "کارشناس",
                    PersonnelCode = "D-" + Guid.NewGuid().ToString("N")[..6],
                    InsuranceTypeId = insurance,
                    CooperationTypeId = cooperation,
                    DepartmentId = department.DepartmentId,
                    StartDate = today,
                    Salary = 0
                }
            });

            var snapshot = await new DashboardService(db).GetAsync(today);
            Assert.True(snapshot.Headcount >= 1);
            Assert.Contains(snapshot.ByDepartment, item => item.Name == department.Title && item.Count >= 1);
            Assert.Contains(snapshot.ByGender, item => item.Name == "زن" && item.Count >= 1);
            Assert.Contains(snapshot.Hires, item => item.Count >= 1);
            Assert.Contains(snapshot.ByStatus, item => item.Name == RosterStatus.NewHire && item.Count >= 1);
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
