using HR.Data;
using HR.Data.Entities;
using HR.Services.Auth;
using HR.Services.Data;
using Microsoft.EntityFrameworkCore;

namespace HR.Tests;

[Collection("sqlserver")]
public class AuthFlowTests
{
    private readonly SqlServerFixture _sql;

    public AuthFlowTests(SqlServerFixture sql) => _sql = sql;

    [SkippableFact]
    public async Task InitialAdmin_Login_Lockout_AndViewerCannotMutate()
    {
        Skip.IfNot(_sql.IsAvailable, _sql.SkipReason ?? "SQL Server آماده نیست.");
        AuthSession.SignOut();

        await using var db = new PersonnelDbContext(
            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<PersonnelDbContext>()
                .UseSqlServer(_sql.PersonnelConnection, sql => sql.UseHierarchyId())
                .Options);

        var auth = new AuthService(db);
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var admin = await auth.CreateInitialAdminAsync("admin-" + suffix, "AdminPass1", "مدیر آزمون");
        Assert.Contains(AppRoles.Admin, admin.Roles);

        var signedIn = await auth.LoginAsync("admin-" + suffix, "AdminPass1");
        Assert.Equal(LoginOutcome.Success, signedIn.Outcome);
        Assert.Equal("مدیر آزمون", signedIn.User!.DisplayName);

        for (var i = 0; i < 4; i++)
        {
            var wrong = await auth.LoginAsync("admin-" + suffix, "wrong-pass");
            Assert.Equal(LoginOutcome.InvalidCredentials, wrong.Outcome);
        }

        var locked = await auth.LoginAsync("admin-" + suffix, "wrong-pass");
        Assert.Equal(LoginOutcome.Locked, locked.Outcome);
        var stillLocked = await auth.LoginAsync("admin-" + suffix, "AdminPass1");
        Assert.Equal(LoginOutcome.Locked, stillLocked.Outcome);

        AuthSession.SignIn(signedIn.User);
        var viewer = await auth.CreateUserAsync("view-" + suffix, "ViewerPass1", "بیننده", new[] { AppRoles.Viewer }, null);
        AuthSession.SignIn(AuthService.ToCurrent(viewer));

        await using var people = new PersonnelDbContext(
            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<PersonnelDbContext>()
                .UseSqlServer(_sql.PersonnelConnection, sql => sql.UseHierarchyId())
                .Options);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new EmployeeService(people).CreateAsync(new Employee()));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new DepartmentService(people).CreateAsync(new Department { Title = "ممنوع" }));

        // مدیر می‌تواند کاربر را حذف کند، ولی حساب خودش را نه.
        AuthSession.SignIn(signedIn.User);
        await auth.DeleteUserAsync(viewer.AppUserId);
        Assert.DoesNotContain(await auth.ListUsersAsync(), item => item.AppUserId == viewer.AppUserId);
        await Assert.ThrowsAsync<InvalidOperationException>(() => auth.DeleteUserAsync(signedIn.User.UserId));

        AuthSession.SignOut();
    }
}
