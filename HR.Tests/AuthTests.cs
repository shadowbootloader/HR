using HR.Services.Auth;

namespace HR.Tests;

public class AuthTests
{
    [Fact]
    public void Hash_VerifiesTheSamePassword_AndRejectsAnother()
    {
        var hashed = PasswordHasher.Create("رمز-امن-۱۲۳", iterations: 10_000);

        Assert.Equal(PasswordHasher.HashSize, hashed.Hash.Length);
        Assert.Equal(PasswordHasher.SaltSize, hashed.Salt.Length);
        Assert.True(PasswordHasher.Verify("رمز-امن-۱۲۳", hashed.Hash, hashed.Salt, hashed.Iterations));
        Assert.False(PasswordHasher.Verify("رمز-دیگر", hashed.Hash, hashed.Salt, hashed.Iterations));
        Assert.False(PasswordHasher.Verify("", hashed.Hash, hashed.Salt, hashed.Iterations));
    }

    [Fact]
    public void TwoHashesOfTheSamePassword_UseDifferentSalts()
    {
        var first = PasswordHasher.Create("same-password", iterations: 10_000);
        var second = PasswordHasher.Create("same-password", iterations: 10_000);

        Assert.NotEqual(first.Salt, second.Salt);
        Assert.NotEqual(first.Hash, second.Hash);
    }

    [Fact]
    public void FifthFailure_LocksTheAccount_UntilTheWindowEnds()
    {
        var now = new DateTime(2026, 11, 17, 8, 0, 0, DateTimeKind.Utc);
        var decision = new LoginDecision(LoginOutcome.InvalidCredentials, 0, null);

        for (var attempt = 1; attempt <= 4; attempt++)
        {
            decision = LoginPolicy.Evaluate(true, true, decision.FailedAttemptCount, decision.LockoutEndUtc, false, now);
            Assert.Equal(LoginOutcome.InvalidCredentials, decision.Outcome);
            Assert.Equal(attempt, decision.FailedAttemptCount);
        }

        decision = LoginPolicy.Evaluate(true, true, decision.FailedAttemptCount, decision.LockoutEndUtc, false, now);
        Assert.Equal(LoginOutcome.Locked, decision.Outcome);
        Assert.Equal(now.Add(LoginPolicy.LockoutDuration), decision.LockoutEndUtc);

        var during = LoginPolicy.Evaluate(true, true, 0, decision.LockoutEndUtc, true, now.AddMinutes(1));
        Assert.Equal(LoginOutcome.Locked, during.Outcome);

        var after = LoginPolicy.Evaluate(true, true, 0, decision.LockoutEndUtc, true, now.Add(LoginPolicy.LockoutDuration).AddSeconds(1));
        Assert.Equal(LoginOutcome.Success, after.Outcome);
        Assert.Equal(0, after.FailedAttemptCount);
        Assert.Null(after.LockoutEndUtc);
    }

    [Fact]
    public void InactiveUser_CannotSignIn()
    {
        var decision = LoginPolicy.Evaluate(true, false, 0, null, true, DateTime.UtcNow);
        Assert.Equal(LoginOutcome.Inactive, decision.Outcome);
    }

    [Fact]
    public void Roles_GatePeopleAndUserManagement()
    {
        Assert.True(AppRoles.CanManagePeople(new[] { AppRoles.Admin }));
        Assert.True(AppRoles.CanManagePeople(new[] { AppRoles.Hr }));
        Assert.False(AppRoles.CanManagePeople(new[] { AppRoles.Viewer }));
        Assert.False(AppRoles.CanManagePeople(Array.Empty<string>()));

        Assert.True(AppRoles.CanManageUsers(new[] { AppRoles.Admin, AppRoles.Hr }));
        Assert.False(AppRoles.CanManageUsers(new[] { AppRoles.Hr }));
        Assert.False(AppRoles.CanManageUsers(new[] { AppRoles.Viewer }));
    }
}
