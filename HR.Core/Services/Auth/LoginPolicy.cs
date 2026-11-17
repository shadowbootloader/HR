namespace HR.Services.Auth
{
    public enum LoginOutcome
    {
        Success,
        InvalidCredentials,
        Locked,
        Inactive
    }

    public readonly record struct LoginDecision(LoginOutcome Outcome, int FailedAttemptCount, DateTime? LockoutEndUtc);

    public static class LoginPolicy
    {
        public const int MaxFailures = 5;
        public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        public static LoginDecision Evaluate(
            bool userFound,
            bool isActive,
            int failedAttemptCount,
            DateTime? lockoutEndUtc,
            bool passwordMatches,
            DateTime utcNow)
        {
            if (!userFound)
                return new LoginDecision(LoginOutcome.InvalidCredentials, failedAttemptCount, lockoutEndUtc);

            if (lockoutEndUtc is DateTime until && until > utcNow)
                return new LoginDecision(LoginOutcome.Locked, failedAttemptCount, lockoutEndUtc);

            if (!isActive)
                return new LoginDecision(LoginOutcome.Inactive, failedAttemptCount, null);

            if (!passwordMatches)
            {
                var next = failedAttemptCount + 1;
                if (next >= MaxFailures)
                    return new LoginDecision(LoginOutcome.Locked, 0, utcNow.Add(LockoutDuration));

                return new LoginDecision(LoginOutcome.InvalidCredentials, next, null);
            }

            return new LoginDecision(LoginOutcome.Success, 0, null);
        }
    }
}
