using HR.Data;
using HR.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.Services.Auth
{
    public sealed class AuthService : IDisposable
    {
        public const int MinimumPasswordLength = 8;

        private readonly PersonnelDbContext _db;
        private readonly bool _ownsContext;

        public AuthService(PersonnelDbContext? db = null)
        {
            if (db == null)
            {
                _db = new PersonnelDbContext();
                _ownsContext = true;
            }
            else
            {
                _db = db;
            }
        }

        public async Task<bool> HasUsersAsync() => await _db.AppUsers.AnyAsync();

        public async Task EnsureRolesAsync()
        {
            var existing = await _db.Roles.Select(role => role.Name).ToListAsync();
            var nextId = (byte)((await _db.Roles.Select(role => (int?)role.RoleId).MaxAsync() ?? 0) + 1);
            foreach (var name in AppRoles.All)
            {
                if (existing.Contains(name)) continue;
                _db.Roles.Add(new Role { RoleId = nextId++, Name = name });
            }

            if (_db.ChangeTracker.HasChanges())
                await _db.SaveChangesAsync();
        }

        public async Task<CurrentUser> CreateInitialAdminAsync(string username, string password, string? displayName)
        {
            if (await HasUsersAsync())
                throw new InvalidOperationException("کاربری از قبل وجود دارد. مدیر اولیه فقط وقتی ساخته می‌شود که جدول کاربران خالی باشد.");

            var user = await CreateUserCoreAsync(username, password, displayName, new[] { AppRoles.Admin }, employeeId: null);
            return ToCurrent(user);
        }

        public async Task<AppUser> CreateUserAsync(string username, string password, string? displayName, IEnumerable<string> roles, Guid? employeeId)
        {
            AccessGuard.RequireAdmin();
            return await CreateUserCoreAsync(username, password, displayName, roles, employeeId);
        }

        public async Task ResetPasswordAsync(Guid userId, string password)
        {
            AccessGuard.RequireAdmin();
            ValidatePassword(password);
            var user = await RequireUserAsync(userId);
            ApplyPassword(user, password);
            user.FailedAttemptCount = 0;
            user.LockoutEndUtc = null;
            await _db.SaveChangesAsync();
        }

        public async Task SetActiveAsync(Guid userId, bool isActive)
        {
            AccessGuard.RequireAdmin();
            if (AuthSession.Current?.UserId == userId && !isActive)
                throw new InvalidOperationException("نمی‌توانید حساب خود را غیرفعال کنید.");

            var user = await RequireUserAsync(userId);
            user.IsActive = isActive;
            await _db.SaveChangesAsync();
        }

        public async Task AssignRolesAsync(Guid userId, IEnumerable<string> roles)
        {
            AccessGuard.RequireAdmin();
            var user = await _db.AppUsers.Include(item => item.Roles).FirstOrDefaultAsync(item => item.AppUserId == userId)
                ?? throw new InvalidOperationException("کاربر یافت نشد.");

            var wanted = await ResolveRolesAsync(roles);
            _db.UserRoles.RemoveRange(user.Roles);
            foreach (var role in wanted)
                _db.UserRoles.Add(new UserRole { AppUserId = user.AppUserId, RoleId = role.RoleId });

            await _db.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<AppUser>> ListUsersAsync()
        {
            AccessGuard.RequireAdmin();
            return await _db.AppUsers
                .Include(user => user.Roles).ThenInclude(link => link.Role)
                .OrderBy(user => user.Username)
                .ToListAsync();
        }

        public async Task<(LoginOutcome Outcome, CurrentUser? User, string Message)> LoginAsync(string username, string password)
        {
            await EnsureRolesAsync();
            var name = (username ?? string.Empty).Trim();
            var user = await _db.AppUsers
                .Include(item => item.Roles).ThenInclude(link => link.Role)
                .FirstOrDefaultAsync(item => item.Username == name);

            var matches = user != null && PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt, user.Iterations);
            var decision = LoginPolicy.Evaluate(
                user != null,
                user?.IsActive ?? false,
                user?.FailedAttemptCount ?? 0,
                user?.LockoutEndUtc,
                matches,
                DateTime.UtcNow);

            if (user != null && decision.Outcome != LoginOutcome.Locked)
            {
                user.FailedAttemptCount = decision.FailedAttemptCount;
                user.LockoutEndUtc = decision.LockoutEndUtc;
                if (decision.Outcome == LoginOutcome.Success)
                    user.LastLoginAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            else if (user != null && decision.LockoutEndUtc != user.LockoutEndUtc)
            {
                user.FailedAttemptCount = decision.FailedAttemptCount;
                user.LockoutEndUtc = decision.LockoutEndUtc;
                await _db.SaveChangesAsync();
            }

            var message = decision.Outcome switch
            {
                LoginOutcome.Success => "ورود انجام شد.",
                LoginOutcome.Inactive => "این حساب غیرفعال است.",
                LoginOutcome.Locked => "به‌خاطر تلاش‌های ناموفق، ورود تا مدتی قفل شد.",
                _ => "نام کاربری یا رمز عبور نادرست است."
            };

            return (decision.Outcome, decision.Outcome == LoginOutcome.Success ? ToCurrent(user!) : null, message);
        }

        private async Task<AppUser> CreateUserCoreAsync(string username, string password, string? displayName, IEnumerable<string> roles, Guid? employeeId)
        {
            await EnsureRolesAsync();
            var name = (username ?? string.Empty).Trim();
            if (name.Length < 3)
                throw new InvalidOperationException("نام کاربری باید حداقل ۳ نویسه باشد.");
            ValidatePassword(password);

            if (await _db.AppUsers.AnyAsync(user => user.Username == name))
                throw new InvalidOperationException("این نام کاربری قبلاً ثبت شده است.");

            if (employeeId.HasValue && !await _db.Employees.AnyAsync(employee => employee.EmployeeId == employeeId))
                throw new InvalidOperationException("کارمند انتخاب‌شده وجود ندارد.");

            var resolved = await ResolveRolesAsync(roles);
            var user = new AppUser
            {
                AppUserId = Guid.NewGuid(),
                Username = name,
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? name : displayName.Trim(),
                EmployeeId = employeeId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            ApplyPassword(user, password);
            _db.AppUsers.Add(user);
            foreach (var role in resolved)
                _db.UserRoles.Add(new UserRole { AppUserId = user.AppUserId, RoleId = role.RoleId });

            await _db.SaveChangesAsync();
            user.Roles = resolved.Select(role => new UserRole { AppUserId = user.AppUserId, RoleId = role.RoleId, Role = role }).ToList();
            return user;
        }

        private async Task<List<Role>> ResolveRolesAsync(IEnumerable<string> roles)
        {
            var names = roles.Where(role => !string.IsNullOrWhiteSpace(role)).Distinct(StringComparer.Ordinal).ToList();
            if (names.Count == 0)
                throw new InvalidOperationException("حداقل یک نقش لازم است.");

            var found = await _db.Roles.Where(role => names.Contains(role.Name)).ToListAsync();
            if (found.Count != names.Count)
                throw new InvalidOperationException("نقش ناشناخته است. نقش‌های مجاز: مدیر، منابع انسانی، بیننده.");

            return found;
        }

        private async Task<AppUser> RequireUserAsync(Guid userId)
            => await _db.AppUsers.FirstOrDefaultAsync(user => user.AppUserId == userId)
               ?? throw new InvalidOperationException("کاربر یافت نشد.");

        private static void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < MinimumPasswordLength)
                throw new InvalidOperationException($"رمز عبور باید حداقل {MinimumPasswordLength} نویسه باشد.");
        }

        private static void ApplyPassword(AppUser user, string password)
        {
            var hashed = PasswordHasher.Create(password);
            user.PasswordHash = hashed.Hash;
            user.PasswordSalt = hashed.Salt;
            user.Iterations = hashed.Iterations;
        }

        public static CurrentUser ToCurrent(AppUser user)
        {
            var roles = user.Roles
                .Select(link => link.Role?.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Cast<string>()
                .ToList();

            return new CurrentUser
            {
                UserId = user.AppUserId,
                Username = user.Username,
                DisplayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.Username : user.DisplayName,
                EmployeeId = user.EmployeeId,
                Roles = roles
            };
        }

        public void Dispose()
        {
            if (_ownsContext) _db.Dispose();
        }
    }
}
