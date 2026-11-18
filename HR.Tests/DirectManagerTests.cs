using HR.Data;
using HR.Data.Entities;
using HR.Services.Auth;
using HR.Services.Data;
using HR.Services.Files;
using Microsoft.EntityFrameworkCore;

namespace HR.Tests;

[Collection("sqlserver")]
public class DirectManagerTests
{
    private readonly SqlServerFixture _sql;

    public DirectManagerTests(SqlServerFixture sql) => _sql = sql;

    [SkippableFact]
    public async Task DepartmentIsOptional_AndDirectManagerComesFromThatDepartment()
    {
        Skip.IfNot(_sql.IsAvailable, _sql.SkipReason ?? "SQL Server آماده نیست.");
        var storage = Path.Combine(Path.GetTempPath(), "hr-mgr-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storage);
        Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, storage);
        AuthSession.SignIn(new CurrentUser
        {
            Username = "manager",
            DisplayName = "مدیر",
            Roles = new[] { AppRoles.Hr }
        });

        try
        {
            await using var db = NewContext();
            var insurance = await db.InsuranceTypes.Where(item => item.Title == "تامین اجتماعی").Select(item => item.InsuranceTypeId).SingleAsync();
            var cooperation = await db.CooperationTypes.Where(item => item.Title == "تمام‌وقت").Select(item => item.CooperationTypeId).SingleAsync();
            var department = await new DepartmentService(db).CreateAsync(new Department
            {
                Title = "مدیریت " + Guid.NewGuid().ToString("N")[..6],
                IsActive = true
            });
            var other = await new DepartmentService(db).CreateAsync(new Department
            {
                Title = "دیگر " + Guid.NewGuid().ToString("N")[..6],
                IsActive = true
            });

            var managerId = Guid.NewGuid();
            var outsiderId = Guid.NewGuid();
            var employeeId = Guid.NewGuid();
            await new EmployeeService(db).CreateAsync(Person(managerId, "1122334451", "نادر", department.DepartmentId, insurance, cooperation));
            await new EmployeeService(db).CreateAsync(Person(outsiderId, "2233445562", "بهروز", other.DepartmentId, insurance, cooperation));
            await new EmployeeService(db).CreateAsync(Person(employeeId, "3344556673", "سارا", null, insurance, cooperation));

            await using var read = NewContext();
            var optional = await new EmployeeService(read).GetByIdAsync(employeeId);
            Assert.Null(optional!.Supplement!.DepartmentId);

            var options = await new EmployeeService(read).ListDirectManagerOptionsAsync(department.DepartmentId, employeeId);
            Assert.Contains(options, item => item.EmployeeId == managerId && item.Label.Contains("نادر"));
            Assert.DoesNotContain(options, item => item.EmployeeId == outsiderId || item.EmployeeId == employeeId);

            optional.Supplement.DepartmentId = department.DepartmentId;
            optional.Supplement.DirectManagerEmployeeId = managerId;
            await using (var update = NewContext())
                await new EmployeeService(update).UpdateAsync(optional);

            await using var saved = NewContext();
            var again = await new EmployeeService(saved).GetByIdAsync(employeeId);
            Assert.Equal(managerId, again!.Supplement!.DirectManagerEmployeeId);

            again.Supplement.DirectManagerEmployeeId = outsiderId;
            await using var reject = NewContext();
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => new EmployeeService(reject).UpdateAsync(again));
            Assert.Contains("همان دپارتمان", ex.Message);
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

    private static Employee Person(Guid id, string nationalCode, string firstName, short? departmentId, byte insurance, byte cooperation)
        => new()
        {
            EmployeeId = id,
            FirstName = firstName,
            LastName = "آزمون",
            FatherName = "رضا",
            NationalCode = nationalCode,
            BirthCertificateNumber = "10",
            BirthDate = new DateTime(1992, 2, 2),
            IssueDate = new DateTime(1992, 2, 3),
            Email = firstName + id.ToString("N")[..4] + "@example.com",
            InsuranceNumber = "1234567890",
            GenderId = 2,
            MaritalStatusId = 1,
            Supplement = new EmployeeSupplement
            {
                PositionTitle = "کارشناس",
                PersonnelCode = "M-" + id.ToString("N")[..6],
                InsuranceTypeId = insurance,
                CooperationTypeId = cooperation,
                DepartmentId = departmentId,
                StartDate = DateTime.Today.AddDays(-10),
                Salary = 0
            }
        };
}
