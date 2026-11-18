using HR.Data;
using HR.Data.Entities;
using HR.Services.Auth;
using HR.Services.Data;
using HR.Services.Files;
using Microsoft.EntityFrameworkCore;

namespace HR.Tests;

[Collection("sqlserver")]
public class DepartmentRulesTests
{
    private readonly SqlServerFixture _sql;

    public DepartmentRulesTests(SqlServerFixture sql) => _sql = sql;

    [SkippableFact]
    public async Task Edit_SavesRolesAndSecurity_AndDelete_BlocksChildrenAndEmployees()
    {
        Skip.IfNot(_sql.IsAvailable, _sql.SkipReason ?? "SQL Server آماده نیست.");
        var storage = Path.Combine(Path.GetTempPath(), "hr-dept-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storage);
        Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, storage);
        SignIn(AppRoles.Hr);

        try
        {
            await using var db = NewContext();
            var insurance = await db.InsuranceTypes.Where(item => item.Title == "تامین اجتماعی").Select(item => item.InsuranceTypeId).SingleAsync();
            var cooperation = await db.CooperationTypes.Where(item => item.Title == "تمام‌وقت").Select(item => item.CooperationTypeId).SingleAsync();
            var parent = await new DepartmentService(db).CreateAsync(new Department { Title = "والد " + Guid.NewGuid().ToString("N")[..6], IsActive = true });
            var child = await new DepartmentService(db).CreateAsync(new Department
            {
                Title = "فرزند " + Guid.NewGuid().ToString("N")[..6],
                ParentDepartmentId = parent.DepartmentId,
                IsActive = true
            });
            var editable = await new DepartmentService(db).CreateAsync(new Department { Title = "ویرایش " + Guid.NewGuid().ToString("N")[..6], IsActive = true });

            var managerId = Guid.NewGuid();
            await new EmployeeService(db).CreateAsync(new Employee
            {
                EmployeeId = managerId,
                FirstName = "کیان",
                LastName = "مدیر",
                FatherName = "رضا",
                NationalCode = "5566778895",
                BirthCertificateNumber = "15",
                BirthDate = new DateTime(1990, 1, 2),
                IssueDate = new DateTime(1990, 1, 3),
                Email = "kian-" + managerId.ToString("N")[..4] + "@example.com",
                InsuranceNumber = "1234567890",
                GenderId = 2,
                MaritalStatusId = 1,
                Supplement = new EmployeeSupplement
                {
                    PositionTitle = "مدیر",
                    PersonnelCode = "K-" + managerId.ToString("N")[..6],
                    InsuranceTypeId = insurance,
                    CooperationTypeId = cooperation,
                    DepartmentId = parent.DepartmentId,
                    StartDate = new DateTime(2020, 1, 1),
                    Salary = 0
                }
            });

            editable.SecurityLevel = SecurityLevels.All[1];
            editable.ApprovedCapacity = 12;
            editable.ParentDepartmentId = parent.DepartmentId;
            editable.ManagerEmployeeId = managerId;
            editable.DeputyEmployeeId = managerId;
            editable.IsActive = false;
            await new DepartmentService(db).UpdateAsync(editable);

            await using (var read = NewContext())
            {
                var saved = await new DepartmentService(read).GetByIdAsync(editable.DepartmentId);
                Assert.Equal("محرمانه", saved!.SecurityLevel);
                Assert.Equal(12, saved.ApprovedCapacity);
                Assert.Equal(parent.DepartmentId, saved.ParentDepartmentId);
                Assert.Equal(managerId, saved.ManagerEmployeeId);
                Assert.Equal(managerId, saved.DeputyEmployeeId);
                Assert.False(saved.IsActive);
            }

            SignIn(AppRoles.Viewer);
            await using (var blocked = NewContext())
            {
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new DepartmentService(blocked).DeleteAsync(editable.DepartmentId));
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new DepartmentService(blocked).UpdateAsync(editable));
            }

            SignIn(AppRoles.Hr);
            await using (var rules = NewContext())
            {
                var error = await Assert.ThrowsAsync<InvalidOperationException>(() => new DepartmentService(rules).DeleteAsync(parent.DepartmentId));
                Assert.Contains("زیرمجموعه", error.Message);
            }

            await using (var removeEditable = NewContext())
                Assert.True(await new DepartmentService(removeEditable).DeleteAsync(editable.DepartmentId));

            await using (var stillChild = NewContext())
            {
                var error = await Assert.ThrowsAsync<InvalidOperationException>(() => new DepartmentService(stillChild).DeleteAsync(parent.DepartmentId));
                Assert.Contains("زیرمجموعه", error.Message);
            }

            await using (var removeChild = NewContext())
                Assert.True(await new DepartmentService(removeChild).DeleteAsync(child.DepartmentId));

            await using (var staff = NewContext())
            {
                var error = await Assert.ThrowsAsync<InvalidOperationException>(() => new DepartmentService(staff).DeleteAsync(parent.DepartmentId));
                Assert.Contains("کارمند", error.Message);
                Assert.NotNull(await staff.Employees.FindAsync(managerId));
            }
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
            Username = "dept-" + role,
            DisplayName = "دپارتمان",
            Roles = new[] { role }
        });
}
