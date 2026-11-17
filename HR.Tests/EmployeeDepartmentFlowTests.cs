using HR.Data;
using HR.Data.Entities;
using HR.Services.Auth;
using HR.Services.Data;
using HR.Services.Files;
using Microsoft.EntityFrameworkCore;

namespace HR.Tests;

[Collection("sqlserver")]
public class EmployeeDepartmentFlowTests
{
    private readonly SqlServerFixture _sql;

    public EmployeeDepartmentFlowTests(SqlServerFixture sql) => _sql = sql;

    [SkippableFact]
    public async Task ListAddSaveEditDeleteRestoreHardDelete_AndDepartmentDelete()
    {
        Skip.IfNot(_sql.IsAvailable, _sql.SkipReason ?? "SQL Server آماده نیست.");
        AuthSession.SignIn(new CurrentUser
        {
            UserId = Guid.NewGuid(),
            Username = "hr-flow",
            DisplayName = "آزمون منابع انسانی",
            Roles = new[] { AppRoles.Hr }
        });

        var storageRoot = Path.Combine(Path.GetTempPath(), "hr-flow-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storageRoot);
        Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, storageRoot);

        try
        {
            await using (var seed = NewContext())
            {
                Assert.Equal("مرد", await seed.Genders.Where(g => g.GenderId == 1).Select(g => g.Title).SingleAsync());
                Assert.Equal("پایان خدمت", await seed.MilitaryStatuses.Where(m => m.MilitaryStatusId == 2).Select(m => m.Title).SingleAsync());
            }

            await using var createDepartments = NewContext();
            var departments = new DepartmentService(createDepartments);
            var createdDepartment = await departments.CreateAsync(new Department
            {
                Title = "منابع انسانی",
                IsActive = true,
                LogoPath = Stage("logo.png", "logo-bytes")
            });

            var logo = await createDepartments.Documents.SingleAsync(d => d.DepartmentId == createdDepartment.DepartmentId && !d.IsDeleted);
            Assert.Equal("logo.png", logo.OriginalFileName);
            Assert.StartsWith($"departments/{createdDepartment.DepartmentId}/", logo.StoredRelativePath);
            Assert.True(File.Exists(Path.Combine(storageRoot, logo.StoredRelativePath.Replace('/', Path.DirectorySeparatorChar))));

            createdDepartment.Title = "منابع انسانی مرکزی";
            var updatedDepartment = await departments.UpdateAsync(createdDepartment);
            Assert.Equal("منابع انسانی مرکزی", (await departments.GetByIdAsync(updatedDepartment.DepartmentId))!.Title);

            byte insuranceId;
            byte cooperationId;
            byte photoTypeId;
            await using (var lookup = NewContext())
            {
                insuranceId = await lookup.InsuranceTypes.Where(t => t.Title == "تامین اجتماعی").Select(t => t.InsuranceTypeId).SingleAsync();
                cooperationId = await lookup.CooperationTypes.Where(t => t.Title == "تمام‌وقت").Select(t => t.CooperationTypeId).SingleAsync();
                photoTypeId = await lookup.FileTypes.Where(t => t.Title == "عکس پرسنلی").Select(t => t.FileTypeId).SingleAsync();
            }

            var employeeId = Guid.NewGuid();
            await using var createEmployees = NewContext();
            var employees = new EmployeeService(createEmployees);
            var employee = NewEmployee(employeeId, insuranceId, cooperationId, updatedDepartment.DepartmentId);
            employee.OtherActivities.Add(new OtherActivity
            {
                Title = "تدریس",
                ExecutionPlace = "تهران"
            });
            employee.PersonFiles.Add(new PersonFile
            {
                FileTypeId = photoTypeId,
                FileAddress = Stage("photo.png", "photo-bytes")
            });

            await employees.CreateAsync(employee);

            await using var readEmployees = NewContext();
            var loaded = await new EmployeeService(readEmployees).GetByIdAsync(employeeId);
            Assert.NotNull(loaded);
            Assert.Equal("نگار", loaded!.FirstName);
            Assert.Single(loaded.OtherActivities);
            var document = await readEmployees.Documents.SingleAsync(d => d.EmployeeId == employeeId && !d.IsDeleted);
            Assert.Equal("photo.png", document.OriginalFileName);
            Assert.True(File.Exists(Path.Combine(storageRoot, document.StoredRelativePath.Replace('/', Path.DirectorySeparatorChar))));

            loaded.FirstName = "نرگس";
            foreach (var file in loaded.PersonFiles)
                file.PersonFileId = Guid.Empty;
            loaded.OtherActivities.Clear();
            loaded.OtherActivities.Add(new OtherActivity { Title = "پژوهش", ExecutionPlace = "اصفهان" });
            await using (var updateEmployees = NewContext())
                await new EmployeeService(updateEmployees).UpdateAsync(loaded);

            await using var readEdited = NewContext();
            var edited = await new EmployeeService(readEdited).GetByIdAsync(employeeId);
            Assert.Equal("نرگس", edited!.FirstName);
            Assert.Equal("پژوهش", Assert.Single(edited.OtherActivities).Title);

            edited.OtherActivities.Clear();
            edited.PersonFiles.Clear();
            await using (var clearEmployees = NewContext())
                await new EmployeeService(clearEmployees).UpdateAsync(edited);
            await using var readCleared = NewContext();
            var cleared = await new EmployeeService(readCleared).GetByIdAsync(employeeId);
            Assert.Empty(cleared!.OtherActivities);
            Assert.Empty(cleared.PersonFiles);
            Assert.False(await readCleared.Documents.AnyAsync(d => d.EmployeeId == employeeId && !d.IsDeleted));

            await using var softDelete = NewContext();
            var soft = new EmployeeService(softDelete);
            Assert.True(await soft.SoftDeleteAsync(employeeId));
            Assert.DoesNotContain(await soft.GetAllAsync(), e => e.EmployeeId == employeeId);
            Assert.Contains(await soft.GetAllAsync(includeDeleted: true), e => e.EmployeeId == employeeId && e.IsDeleted);

            await using var blocked = NewContext();
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                new DepartmentService(blocked).DeleteAsync(updatedDepartment.DepartmentId));
            Assert.Contains("کارمند", ex.Message);

            await using var restore = NewContext();
            var restored = new EmployeeService(restore);
            Assert.True(await restored.RestoreAsync(employeeId));
            Assert.Contains(await restored.GetAllAsync(), e => e.EmployeeId == employeeId && !e.IsDeleted);

            await using var hard = NewContext();
            var hardDelete = new EmployeeService(hard);
            Assert.True(await hardDelete.HardDeleteAsync(employeeId));
            Assert.Null(await hardDelete.GetByIdAsync(employeeId));
            Assert.False(await hard.OtherActivities.AnyAsync(a => a.EmployeeId == employeeId));

            await using var removeDepartment = NewContext();
            var departmentCleanup = new DepartmentService(removeDepartment);
            Assert.True(await departmentCleanup.DeleteAsync(updatedDepartment.DepartmentId));
            Assert.Null(await departmentCleanup.GetByIdAsync(updatedDepartment.DepartmentId));
            Assert.False(await removeDepartment.Documents.AnyAsync(d => d.DepartmentId == updatedDepartment.DepartmentId));
        }
        finally
        {
            AuthSession.SignOut();
            Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, null);
            if (Directory.Exists(storageRoot))
                Directory.Delete(storageRoot, recursive: true);
        }
    }

    private PersonnelDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<PersonnelDbContext>()
            .UseSqlServer(_sql.PersonnelConnection, sql => sql.UseHierarchyId())
            .Options;
        return new PersonnelDbContext(options);
    }

    private string Stage(string name, string contents)
    {
        var folder = Path.Combine(Path.GetTempPath(), "hr-src-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var source = Path.Combine(folder, name);
        File.WriteAllText(source, contents);
        return new FileStorageService(Environment.GetEnvironmentVariable(FileStorageService.EnvironmentVariable)!).Stage(source, FileKind.Image).RelativePath;
    }

    private static Employee NewEmployee(Guid id, byte insuranceId, byte cooperationId, short departmentId) => new()
    {
        EmployeeId = id,
        FirstName = "نگار",
        LastName = "کریمی",
        FatherName = "رضا",
        NationalCode = "1234567891",
        BirthCertificateNumber = "123",
        BirthDate = new DateTime(1990, 1, 2),
        IssueDate = new DateTime(1990, 1, 3),
        Email = "negar@example.com",
        InsuranceNumber = "1234567890",
        ChildrenCount = 0,
        GenderId = 1,
        MaritalStatusId = 1,
        MilitaryStatusId = 2,
        Supplement = new EmployeeSupplement
        {
            PositionTitle = "کارشناس",
            PersonnelCode = "P100",
            InsuranceTypeId = insuranceId,
            CooperationTypeId = cooperationId,
            DepartmentId = departmentId,
            StartDate = new DateTime(2020, 3, 21),
            Salary = 0
        }
    };
}

[CollectionDefinition("sqlserver")]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>;
