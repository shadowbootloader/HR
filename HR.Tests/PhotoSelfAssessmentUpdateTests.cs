using HR.Data;
using HR.Data.Entities;
using HR.Services.Auth;
using HR.Services.Data;
using HR.Services.Files;
using Microsoft.EntityFrameworkCore;

namespace HR.Tests;

[Collection("sqlserver")]
public class PhotoSelfAssessmentUpdateTests
{
    private readonly SqlServerFixture _sql;

    public PhotoSelfAssessmentUpdateTests(SqlServerFixture sql) => _sql = sql;

    [SkippableFact]
    public async Task Update_WithExistingSelfAssessment_AndANewPhoto_Saves()
    {
        Skip.IfNot(_sql.IsAvailable, _sql.SkipReason ?? "SQL Server آماده نیست.");
        var storage = Path.Combine(Path.GetTempPath(), "hr-photo-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storage);
        Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, storage);
        AuthSession.SignIn(new CurrentUser
        {
            Username = "photo",
            DisplayName = "عکس",
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
            var photoType = await db.FileTypes.Where(item => item.Title == "عکس پرسنلی").Select(item => item.FileTypeId).SingleAsync();
            var department = await new DepartmentService(db).CreateAsync(new Department
            {
                Title = "عکس " + Guid.NewGuid().ToString("N")[..6],
                IsActive = true
            });
            var id = Guid.NewGuid();
            await new EmployeeService(db).CreateAsync(new Employee
            {
                EmployeeId = id,
                FirstName = "نرگس",
                LastName = "عکس",
                FatherName = "رضا",
                NationalCode = "9988776659",
                BirthCertificateNumber = "44",
                BirthDate = new DateTime(1993, 4, 4),
                IssueDate = new DateTime(1993, 4, 5),
                Email = "photo-" + id.ToString("N")[..6] + "@example.com",
                InsuranceNumber = "1234567890",
                GenderId = 2,
                MaritalStatusId = 1,
                Supplement = new EmployeeSupplement
                {
                    PositionTitle = "کارشناس",
                    PersonnelCode = "PH-" + id.ToString("N")[..6],
                    InsuranceTypeId = insurance,
                    CooperationTypeId = cooperation,
                    DepartmentId = department.DepartmentId,
                    StartDate = new DateTime(2020, 4, 4),
                    Salary = 0
                },
                SelfAssessment = new EmployeeSelfAssessment
                {
                    Discipline = 40,
                    Speed = 40,
                    Effort = 40,
                    Negotiation = 40,
                    Patience = 40,
                    Creativity = 40,
                    Punctuality = 40,
                    Teamwork = 40,
                    Accuracy = 40,
                    Planning = 40
                }
            });

            var loaded = await new EmployeeService(db).GetByIdAsync(id);
            Assert.NotNull(loaded?.SelfAssessment);
            var incoming = new Employee
            {
                EmployeeId = id,
                FirstName = loaded!.FirstName,
                LastName = loaded.LastName,
                FatherName = loaded.FatherName,
                NationalCode = loaded.NationalCode,
                BirthCertificateNumber = loaded.BirthCertificateNumber,
                BirthDate = loaded.BirthDate,
                IssueDate = loaded.IssueDate,
                Email = loaded.Email,
                InsuranceNumber = loaded.InsuranceNumber,
                GenderId = loaded.GenderId,
                MaritalStatusId = loaded.MaritalStatusId,
                Supplement = new EmployeeSupplement
                {
                    PositionTitle = loaded.Supplement!.PositionTitle,
                    PersonnelCode = loaded.Supplement.PersonnelCode,
                    InsuranceTypeId = loaded.Supplement.InsuranceTypeId,
                    CooperationTypeId = loaded.Supplement.CooperationTypeId,
                    DepartmentId = loaded.Supplement.DepartmentId,
                    StartDate = loaded.Supplement.StartDate,
                    Salary = loaded.Supplement.Salary
                },
                SelfAssessment = new EmployeeSelfAssessment
                {
                    EmployeeSelfAssessmentId = Guid.NewGuid(),
                    EmployeeId = id,
                    Discipline = 80,
                    Speed = 40,
                    Effort = 40,
                    Negotiation = 40,
                    Patience = 40,
                    Creativity = 40,
                    Punctuality = 40,
                    Teamwork = 40,
                    Accuracy = 40,
                    Planning = 40
                }
            };
            incoming.PersonFiles.Add(new PersonFile
            {
                FileTypeId = photoType,
                FileAddress = Stage("photo.png")
            });

            await using var updateDb = new PersonnelDbContext(
                new DbContextOptionsBuilder<PersonnelDbContext>()
                    .UseSqlServer(_sql.PersonnelConnection, sql => sql.UseHierarchyId())
                    .Options);
            var updated = await new EmployeeService(updateDb).UpdateAsync(incoming);
            Assert.Equal(80, updated.SelfAssessment!.Discipline);
            Assert.Single(updated.PersonFiles);
        }
        finally
        {
            AuthSession.SignOut();
            Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, null);
        }
    }

    private static string Stage(string name)
    {
        var folder = Path.Combine(Path.GetTempPath(), "hr-src-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var source = Path.Combine(folder, name);
        File.WriteAllBytes(source, new byte[] { 1, 2, 3, 4 });
        return new FileStorageService(Environment.GetEnvironmentVariable(FileStorageService.EnvironmentVariable)!).Stage(source, FileKind.Image).RelativePath;
    }
}
