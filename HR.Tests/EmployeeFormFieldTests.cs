using HR.Data;
using HR.Data.Entities;
using HR.Services.Auth;
using HR.Services.Data;
using HR.Services.Files;
using Microsoft.EntityFrameworkCore;

namespace HR.Tests;

public class EmployeeFormFieldRules
{
    [Fact]
    public void Postal_IsKeptWithoutAddressText_AndPostIsTheJobTitle()
    {
        var row = EmployeeFormFields.AddressFromForm(Guid.NewGuid(), null, null, "1234567890");
        Assert.NotNull(row);
        Assert.Equal("1234567890", row!.PostalCode);
        Assert.Equal(EmployeeFormFields.PostalOnlyAddress, row.AddressText);

        var read = EmployeeFormFields.ReadAddress(row);
        Assert.Equal(string.Empty, read.AddressText);
        Assert.Equal("1234567890", read.PostalCode);
        Assert.Equal("کارشناس فروش", EmployeeFormFields.PostFromTitle("  کارشناس فروش "));
        Assert.Null(EmployeeFormFields.AddressFromForm(Guid.NewGuid(), " ", null, null));
    }
}

[Collection("sqlserver")]
public class EmployeeFormFieldFlowTests
{
    private readonly SqlServerFixture _sql;

    public EmployeeFormFieldFlowTests(SqlServerFixture sql) => _sql = sql;

    [SkippableFact]
    public async Task CreateAndEdit_RoundTripPostalCodeAndPosition()
    {
        Skip.IfNot(_sql.IsAvailable, _sql.SkipReason ?? "SQL Server آماده نیست.");
        var storage = Path.Combine(Path.GetTempPath(), "hr-fields-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storage);
        Environment.SetEnvironmentVariable(FileStorageService.EnvironmentVariable, storage);
        AuthSession.SignIn(new CurrentUser { Username = "fields", DisplayName = "فرم", Roles = new[] { AppRoles.Hr } });
        try
        {
            await using var db = New();
            var insurance = await db.InsuranceTypes.Where(item => item.Title == "تامین اجتماعی").Select(item => item.InsuranceTypeId).SingleAsync();
            var cooperation = await db.CooperationTypes.Where(item => item.Title == "تمام‌وقت").Select(item => item.CooperationTypeId).SingleAsync();
            var id = Guid.NewGuid();
            var address = EmployeeFormFields.AddressFromForm(id, null, null, "9876543210");
            await new EmployeeService(db).CreateAsync(new Employee
            {
                EmployeeId = id,
                FirstName = "پویا",
                LastName = "فیلد",
                FatherName = "رضا",
                NationalCode = "6677889904",
                BirthCertificateNumber = "21",
                BirthDate = new DateTime(1993, 3, 3),
                IssueDate = new DateTime(1993, 3, 4),
                Email = "fields-" + id.ToString("N")[..4] + "@example.com",
                InsuranceNumber = "1234567890",
                GenderId = 2,
                MaritalStatusId = 1,
                Supplement = new EmployeeSupplement
                {
                    PositionTitle = "کارشناس فروش",
                    PersonnelCode = "F-" + id.ToString("N")[..6],
                    InsuranceTypeId = insurance,
                    CooperationTypeId = cooperation,
                    StartDate = new DateTime(2021, 5, 5),
                    Salary = 0
                },
                Addresses = { address! }
            });

            await using var read = New();
            var loaded = await new EmployeeService(read).GetByIdAsync(id);
            Assert.Equal("کارشناس فروش", loaded!.Supplement!.PositionTitle);
            Assert.Equal("کارشناس فروش", EmployeeFormFields.PostFromTitle(loaded.Supplement.PositionTitle));
            var postal = EmployeeFormFields.ReadAddress(loaded.Addresses.Single(item => item.RelatedPersonId == null));
            Assert.Equal("9876543210", postal.PostalCode);

            loaded.Supplement.PositionTitle = "سرپرست فروش";
            loaded.Addresses.Clear();
            loaded.Addresses.Add(EmployeeFormFields.AddressFromForm(id, null, null, "1122334455")!);
            await using (var update = New())
                await new EmployeeService(update).UpdateAsync(loaded);

            await using var reread = New();
            var edited = await new EmployeeService(reread).GetByIdAsync(id);
            Assert.Equal("سرپرست فروش", edited!.Supplement!.PositionTitle);
            Assert.Equal("1122334455", EmployeeFormFields.ReadAddress(edited.Addresses.Single(item => item.RelatedPersonId == null)).PostalCode);
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
}
