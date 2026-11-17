using HR.Data;
using Microsoft.EntityFrameworkCore;

namespace HR.Tests;

[Collection("sqlserver")]
public class LookupSeedTests
{
    private readonly SqlServerFixture _sql;

    public LookupSeedTests(SqlServerFixture sql) => _sql = sql;

    [SkippableFact]
    public async Task ClosedLookups_AreSeededWithStableIds()
    {
        Skip.IfNot(_sql.IsAvailable, _sql.SkipReason ?? "SQL Server آماده نیست.");
        await using var db = new PersonnelDbContext(
            new DbContextOptionsBuilder<PersonnelDbContext>()
                .UseSqlServer(_sql.PersonnelConnection, sql => sql.UseHierarchyId())
                .Options);

        Assert.Equal("مرد", await db.Genders.Where(item => item.GenderId == 1).Select(item => item.Title).SingleAsync());
        Assert.Equal("زن", await db.Genders.Where(item => item.GenderId == 2).Select(item => item.Title).SingleAsync());
        Assert.Equal("معاف", await db.MilitaryStatuses.Where(item => item.MilitaryStatusId == 1).Select(item => item.Title).SingleAsync());
        Assert.Equal("مجرد", await db.MaritalStatuses.Where(item => item.MaritalStatusId == 1).Select(item => item.Title).SingleAsync());
        Assert.Equal("کارشناسی", await db.EducationDegrees.Where(item => item.DegreeId == 4).Select(item => item.Title).SingleAsync());
        Assert.Contains("بانک ملی ایران", await db.Banks.Select(item => item.Title).ToListAsync());
        Assert.Contains("فارسی", await db.Languages.Select(item => item.Title).ToListAsync());
        Assert.Contains("تامین اجتماعی", await db.InsuranceTypes.Select(item => item.Title).ToListAsync());
        Assert.Contains("شیعه", await db.Denominations.Select(item => item.Title).ToListAsync());
    }
}
