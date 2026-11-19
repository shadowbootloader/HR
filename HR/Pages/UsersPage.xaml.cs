using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using HR.Data.Entities;
using HR.Services;
using HR.Services.Auth;

namespace HR.Pages
{
    public partial class UsersPage : Page
    {
        private readonly ObservableCollection<UserRow> _rows = new();
        private readonly List<UserRow> _all = new();
        private Guid? _selectedId;
        private bool _selectedActive = true;

        public UsersPage()
        {
            InitializeComponent();
            UserGrid.ItemsSource = _rows;
            Loaded += async (_, _) => await ReloadAsync();
        }

        private async System.Threading.Tasks.Task ReloadAsync()
        {
            _all.Clear();
            _rows.Clear();
            try
            {
                using var auth = new AuthService();
                foreach (var user in await auth.ListUsersAsync())
                    _all.Add(UserRow.From(user));
                ApplyFilter();
                SetFormMode(null);
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

            CountText.Text = _rows.Count + " کاربر";
        }

        /// <summary>فرم را برای کاربر جدید (null) یا ویرایش کاربر انتخاب‌شده تنظیم می‌کند.</summary>
        private void SetFormMode(UserRow? row)
        {
            var editing = row != null;
            _selectedId = row?.Id;
            _selectedActive = row?.IsActive ?? true;

            FormTitle.Text = row == null ? "کاربر جدید" : row.IsActive ? "ویرایش کاربر" : "کاربر غیرفعال";
            UsernameBox.IsEnabled = !editing;
            UsernameBox.Text = row?.Username ?? string.Empty;
            DisplayNameBox.Text = row?.DisplayName ?? string.Empty;
            PasswordBox.Clear();
            PasswordLabel.Text = editing ? "رمز جدید (فقط برای بازنشانی)" : "رمز عبور";
            ActiveButton.Content = row?.IsActive == false ? "فعال‌سازی" : "غیرفعال‌سازی";

            CreateButton.Visibility = editing ? Visibility.Collapsed : Visibility.Visible;
            SaveButton.Visibility = editing ? Visibility.Visible : Visibility.Collapsed;
            ResetButton.Visibility = editing ? Visibility.Visible : Visibility.Collapsed;
            ActiveButton.Visibility = editing ? Visibility.Visible : Visibility.Collapsed;
            NewButton.Visibility = editing ? Visibility.Visible : Visibility.Collapsed;

            if (row != null) SelectRole(row.Role);
            else RoleBox.SelectedIndex = 2;
        }

        private void UserGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserGrid.SelectedItem is UserRow row)
                SetFormMode(row);
        }

        private void EditRow_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not UserRow row) return;
            UserGrid.SelectedItem = row;
            SetFormMode(row);
            DisplayNameBox.Focus();
        }

        private async void DeleteRow_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not UserRow row) return;

            var answer = MessageBox.Show(
                $"آیا از حذف کاربر «{row.DisplayName}» مطمئن هستید؟\nاین کار قابل بازگشت نیست.",
                "حذف کاربر",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (answer != MessageBoxResult.Yes) return;

            try
            {
                using var auth = new AuthService();
                await auth.DeleteUserAsync(row.Id);
                NotificationService.Success("حذف شد", $"کاربر «{row.DisplayName}» حذف شد.");
                await ReloadAsync();
                StatusText.Text = "کاربر حذف شد.";
            }
            catch (Exception ex)
            {
                StatusText.Text = ex.Message;
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            UserGrid.SelectedItem = null;
            SetFormMode(null);
            UsernameBox.Focus();
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var auth = new AuthService();
                await auth.CreateUserAsync(UsernameBox.Text, PasswordBox.Password, DisplayNameBox.Text, new[] { SelectedRole() }, null);
                PasswordBox.Clear();
                StatusText.Text = "کاربر ساخته شد.";
                await ReloadAsync();
            }
            catch (Exception ex)
            {
                StatusText.Text = ex.Message;
            }
        }

        private async void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedId == null)
            {
                StatusText.Text = "ابتدا یک کاربر را از فهرست انتخاب کنید.";
                return;
            }

            try
            {
                using var auth = new AuthService();
                await auth.UpdateDisplayNameAsync(_selectedId.Value, DisplayNameBox.Text);
                await auth.AssignRolesAsync(_selectedId.Value, new[] { SelectedRole() });
                await ReloadAsync();
                StatusText.Text = "تغییرات ذخیره شد.";
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
                using var auth = new AuthService();
                await auth.ResetPasswordAsync(_selectedId.Value, PasswordBox.Password);
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
                using var auth = new AuthService();
                await auth.SetActiveAsync(_selectedId.Value, !_selectedActive);
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
            public string Initial { get; init; } = "?";
            public string RoleColor { get; init; } = "#38BDF8";
            public string RoleBackground { get; init; } = "#1A0C2A33";
            public string StateColor { get; init; } = "#22C55E";
            public string StateBackground { get; init; } = "#1A22C55E";

            public static UserRow From(AppUser user)
            {
                var role = user.Roles.Select(link => link.Role?.Name).FirstOrDefault(name => name != null) ?? AppRoles.Viewer;
                var locked = user.LockoutEndUtc is DateTime until && until > DateTime.UtcNow;
                var employee = user.Employee == null ? "—" : $"{user.Employee.FirstName} {user.Employee.LastName}".Trim();
                var state = !user.IsActive ? "غیرفعال" : locked ? "قفل‌شده" : "فعال";
                var display = user.DisplayName ?? user.Username;
                var roleColor = role == AppRoles.Admin ? "#FF3C91" : role == AppRoles.Hr ? "#00E5D4" : "#38BDF8";
                var stateColor = !user.IsActive ? "#94A3B8" : locked ? "#F59E0B" : "#22C55E";
                return new UserRow
                {
                    Initial = string.IsNullOrWhiteSpace(display) ? "?" : display.Trim().Substring(0, 1),
                    RoleColor = roleColor,
                    RoleBackground = "#22" + roleColor.TrimStart('#'),
                    StateColor = stateColor,
                    StateBackground = "#22" + stateColor.TrimStart('#'),
                    Id = user.AppUserId,
                    Username = user.Username,
                    DisplayName = user.DisplayName ?? user.Username,
                    Role = role!,
                    RoleLabel = AppRoles.PersianName(role!),
                    IsActive = user.IsActive,
                    EmployeeName = string.IsNullOrWhiteSpace(employee) ? "—" : employee,
                    State = state,
                    LastLogin = user.LastLoginAt?.ToLocalTime().ToString("yyyy/MM/dd HH:mm") ?? "—"
                };
            }
        }
    }
}
