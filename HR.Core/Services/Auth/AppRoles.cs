namespace HR.Services.Auth
{
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Hr = "HR";
        public const string Viewer = "Viewer";

        public static readonly string[] All = { Admin, Hr, Viewer };

        public static bool CanManagePeople(IEnumerable<string>? roles)
            => roles?.Any(role => role is Admin or Hr) == true;

        public static bool CanManageUsers(IEnumerable<string>? roles)
            => roles?.Any(role => role == Admin) == true;

        public static string PersianName(string role) => role switch
        {
            Admin => "مدیر",
            Hr => "منابع انسانی",
            Viewer => "بیننده",
            _ => role
        };
    }

    public sealed class CurrentUser
    {
        public Guid UserId { get; init; }
        public string Username { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public Guid? EmployeeId { get; init; }
        public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();

        public bool IsAdmin => AppRoles.CanManageUsers(Roles);
        public bool CanManagePeople => AppRoles.CanManagePeople(Roles);

        public string RoleLabel => string.Join("، ", Roles.Select(AppRoles.PersianName));
    }

    public static class AuthSession
    {
        public static CurrentUser? Current { get; private set; }

        public static void SignIn(CurrentUser user) => Current = user ?? throw new ArgumentNullException(nameof(user));

        public static void SignOut() => Current = null;

        public static string ActorName
        {
            get
            {
                var current = Current;
                if (current == null) return "کاربر سامانه";
                return string.IsNullOrWhiteSpace(current.DisplayName) ? current.Username : current.DisplayName;
            }
        }
    }

    public static class AccessGuard
    {
        public static void RequirePeopleManagement()
        {
            if (!AppRoles.CanManagePeople(AuthSession.Current?.Roles))
                throw new UnauthorizedAccessException("برای ایجاد، ویرایش یا حذف، نقش منابع انسانی یا مدیر لازم است.");
        }

        public static void RequireAdmin()
        {
            if (!AppRoles.CanManageUsers(AuthSession.Current?.Roles))
                throw new UnauthorizedAccessException("فقط مدیر سامانه به این بخش دسترسی دارد.");
        }
    }
}
