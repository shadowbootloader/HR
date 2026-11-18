using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using HR.Data.Entities;
using HR.Services.Auth;

namespace HR.Pages
{
    public partial class UsersPage : Page
    {
        private readonly AuthService _auth = new();
        private readonly ObservableCollection<UserRow> _rows = new();
        private readonly List<UserRow> _all = new();
        private Guid? _selectedId;
        private bool _selectedActive = true;

        public UsersPage()
        {
            InitializeComponent();
            UserGrid.ItemsSource = _rows;
            Loaded += async (_, _) => await ReloadAsync();
            Unloaded += (_, _) => _auth.Dispose();
        }

        private async System.Threading.Tasks.Task ReloadAsync()
        {
            _all.Clear();
            _rows.Clear();
            try
            {
                foreach (var user in await _auth.ListUsersAsync())
                    _all.Add(UserRow.From(user));
                ApplyFilter();
            }
            catch (Exception ex)
            {
                StatusText.Text = ex.Message;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

        private void ApplyFilter()
        {
            var term = SearchBox?.Text?.Trim() ?? string.Empty;
            _rows.Clear();
            foreach (var row in _all.Where(item =>
                         string.IsNullOrWhiteSpace(term) ||
                         item.Username.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                         item.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                         item.EmployeeName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                         item.RoleLabel.Contains(term, StringComparison.OrdinalIgnoreCase)))
                _rows.Add(row);
        }

        private void UserGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserGrid.SelectedItem is not UserRow row)
            {
                _selectedId = null;
                FormTitle.Text = "کاربر جدید";
                return;
            }

            _selectedId = row.Id;
            _selectedActive = row.IsActive;
            UsernameBox.Text = row.Username;
            DisplayNameBox.Text = row.DisplayName;
            FormTitle.Text = row.IsActive ? "ویرایش کاربر" : "کاربر غیرفعال";
            ActiveButton.Content = row.IsActive ? "غیرفعال‌سازی" : "فعال‌سازی";
            SelectRole(row.Role);
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _auth.CreateUserAsync(UsernameBox.Text, PasswordBox.Password, DisplayNameBox.Text, new[] { SelectedRole() }, null);
                PasswordBox.Clear();
                StatusText.Text = "کاربر ساخته شد.";
                await ReloadAsync();
            }
            catch (Exception ex)
            {
                StatusText.Text = ex.Message;
            }
        }

        private async void SaveRole_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedId == null)
            {
                StatusText.Text = "ابتدا یک کاربر را از فهرست انتخاب کنید.";
                return;
            }

            try
            {
                await _auth.AssignRolesAsync(_selectedId.Value, new[] { SelectedRole() });
                StatusText.Text = "نقش ذخیره شد.";
                await ReloadAsync();
            }
            catch (Exception ex)
            {
                StatusText.Text = ex.Message;
            }
        }

        private async void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedId == null)
            {
                StatusText.Text = "ابتدا یک کاربر را از فهرست انتخاب کنید.";
                return;
            }

            try
            {
                await _auth.ResetPasswordAsync(_selectedId.Value, PasswordBox.Password);
                PasswordBox.Clear();
                StatusText.Text = "رمز عبور بازنشانی شد.";
            }
            catch (Exception ex)
            {
                StatusText.Text = ex.Message;
            }
        }

        private async void ToggleActive_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedId == null)
            {
                StatusText.Text = "ابتدا یک کاربر را از فهرست انتخاب کنید.";
                return;
            }

            try
            {
                await _auth.SetActiveAsync(_selectedId.Value, !_selectedActive);
                StatusText.Text = _selectedActive ? "کاربر غیرفعال شد." : "کاربر فعال شد.";
                await ReloadAsync();
            }
            catch (Exception ex)
            {
                StatusText.Text = ex.Message;
            }
        }

        private string SelectedRole()
            => (RoleBox.SelectedItem as ComboBoxItem)?.Tag as string ?? AppRoles.Viewer;

        private void SelectRole(string role)
        {
            foreach (ComboBoxItem item in RoleBox.Items)
            {
                if ((item.Tag as string) == role)
                {
                    RoleBox.SelectedItem = item;
                    return;
                }
            }
        }

        private sealed class UserRow
        {
            public Guid Id { get; init; }
            public string Username { get; init; } = "";
            public string DisplayName { get; init; } = "";
            public string Role { get; init; } = AppRoles.Viewer;
            public bool IsActive { get; init; }
            public string RoleLabel { get; init; } = "";
            public string EmployeeName { get; init; } = "—";
            public string State { get; init; } = "";
            public string LastLogin { get; init; } = "—";

            public static UserRow From(AppUser user)
            {
                var role = user.Roles.Select(link => link.Role?.Name).FirstOrDefault(name => name != null) ?? AppRoles.Viewer;
                var locked = user.LockoutEndUtc is DateTime until && until > DateTime.UtcNow;
                var employee = user.Employee == null ? "—" : $"{user.Employee.FirstName} {user.Employee.LastName}".Trim();
                return new UserRow
                {
                    Id = user.AppUserId,
                    Username = user.Username,
                    DisplayName = user.DisplayName ?? user.Username,
                    Role = role!,
                    RoleLabel = AppRoles.PersianName(role!),
                    IsActive = user.IsActive,
                    EmployeeName = string.IsNullOrWhiteSpace(employee) ? "—" : employee,
                    State = !user.IsActive ? "غیرفعال" : locked ? "قفل‌شده" : "فعال",
                    LastLogin = user.LastLoginAt?.ToLocalTime().ToString("yyyy/MM/dd HH:mm") ?? "—"
                };
            }
        }
    }
}
