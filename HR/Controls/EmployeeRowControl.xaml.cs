using System;
using System.Windows;
using System.Windows.Controls;
using HR.Pages;
using HR.Services;
using HR.Services.Auth;
using HR.Services.Data;

namespace HR.Controls
{
    public partial class EmployeeRowControl : UserControl
    {
        public EmployeeModel? Employee => DataContext as EmployeeModel;

        public static event Action<Page>? NavigationRequested;
        public static event Action? ListRefreshRequested;

        public EmployeeRowControl()
        {
            InitializeComponent();
            DataContextChanged += (_, _) =>
            {
                UpdateActionButtons();
                UpdateAvatar();
            };
        }

        private void UpdateAvatar()
        {
            var image = ImagePicker.Load(Employee?.PhotoPath);
            AvatarBrush.ImageSource = image;
            AvatarIcon.Visibility = image == null ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateActionButtons()
        {
            if (AuthSession.Current?.CanManagePeople != true)
            {
                EditBtn.Visibility = Visibility.Collapsed;
                DeleteBtn.Visibility = Visibility.Collapsed;
                RestoreBtn.Visibility = Visibility.Collapsed;
                HardDeleteBtn.Visibility = Visibility.Collapsed;
                //LeaveBtn.Visibility = Visibility.Collapsed;
                return;
            }

            var deleted = Employee?.IsDeleted == true;
            //LeaveBtn.Visibility = deleted ? Visibility.Collapsed : Visibility.Visible;
            DeleteBtn.Visibility = deleted ? Visibility.Collapsed : Visibility.Visible;
            RestoreBtn.Visibility = deleted ? Visibility.Visible : Visibility.Collapsed;
            HardDeleteBtn.Visibility = deleted ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ViewBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Employee == null) return;
            NavigationRequested?.Invoke(new EmployeOverview(Employee));
        }

        private void LeaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Employee == null || Employee.Id == Guid.Empty) return;
            var window = new LeaveWindow(Employee.Id, Employee.FullName) { Owner = Window.GetWindow(this) };
            if (window.ShowDialog() == true)
                ListRefreshRequested?.Invoke();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Employee == null || Employee.Id == Guid.Empty) return;
            NavigationRequested?.Invoke(new AddEmployeePage(Employee.Id));
        }

        private async void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Employee == null || Employee.Id == Guid.Empty) return;

            var answer = MessageBox.Show(
                $"آیا از حذف «{Employee.FullName}» مطمئن هستید؟\n(حذف به‌صورت نرم انجام می‌شود و قابل بازگردانی است)",
                "حذف کارمند",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (answer != MessageBoxResult.Yes) return;

            try
            {
                using var service = new EmployeeService();
                var ok = await service.SoftDeleteAsync(Employee.Id);
                if (ok)
                {
                    NotificationService.Success("حذف شد", $"«{Employee.FullName}» حذف شد.");
                    ListRefreshRequested?.Invoke();
                }
                else
                {
                    NotificationService.Warning("خطا", "کارمند یافت نشد.");
                }
            }
            catch (Exception ex)
            {
                NotificationService.Warning("خطا در حذف", ex.Message);
            }
        }

        private async void RestoreBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Employee == null || Employee.Id == Guid.Empty) return;

            try
            {
                using var service = new EmployeeService();
                var ok = await service.RestoreAsync(Employee.Id);
                if (ok)
                {
                    NotificationService.Success("بازگردانی شد", $"«{Employee.FullName}» به فهرست فعال برگشت.");
                    ListRefreshRequested?.Invoke();
                }
                else
                {
                    NotificationService.Warning("خطا", "کارمند یافت نشد.");
                }
            }
            catch (Exception ex)
            {
                NotificationService.Warning("خطا در بازگردانی", ex.Message);
            }
        }

        private async void HardDeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Employee == null || Employee.Id == Guid.Empty) return;

            var answer = MessageBox.Show(
                $"حذف دائمی «{Employee.FullName}» قابل بازگشت نیست. ادامه می‌دهید؟",
                "حذف دائمی",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (answer != MessageBoxResult.Yes) return;

            try
            {
                using var service = new EmployeeService();
                var ok = await service.HardDeleteAsync(Employee.Id);
                if (ok)
                {
                    NotificationService.Success("حذف دائمی", $"«{Employee.FullName}» برای همیشه حذف شد.");
                    ListRefreshRequested?.Invoke();
                }
                else
                {
                    NotificationService.Warning("خطا", "کارمند یافت نشد.");
                }
            }
            catch (Exception ex)
            {
                NotificationService.Warning("خطا در حذف دائمی", ex.Message);
            }
        }

        // ==================== Dependency Properties ====================

        public static readonly DependencyProperty FullNameProperty =
            DependencyProperty.Register(nameof(FullName), typeof(string), typeof(EmployeeRowControl));

        public string FullName
        {
            get => (string)GetValue(FullNameProperty);
            set => SetValue(FullNameProperty, value);
        }

        public static readonly DependencyProperty EmailProperty =
            DependencyProperty.Register(nameof(Email), typeof(string), typeof(EmployeeRowControl));

        public string Email
        {
            get => (string)GetValue(EmailProperty);
            set => SetValue(EmailProperty, value);
        }

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(string), typeof(EmployeeRowControl));

        public string Position
        {
            get => (string)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        public static readonly DependencyProperty DepartmentProperty =
            DependencyProperty.Register(nameof(Department), typeof(string), typeof(EmployeeRowControl));

        public string Department
        {
            get => (string)GetValue(DepartmentProperty);
            set => SetValue(DepartmentProperty, value);
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(nameof(Status), typeof(string), typeof(EmployeeRowControl),
                new PropertyMetadata("Active"));

        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public static readonly DependencyProperty PerformanceValueProperty =
            DependencyProperty.Register(nameof(PerformanceValue), typeof(double), typeof(EmployeeRowControl),
                new PropertyMetadata(0.0));

        public double PerformanceValue
        {
            get => (double)GetValue(PerformanceValueProperty);
            set
            {
                SetValue(PerformanceValueProperty, value);
                Performancegauge.Value = PerformanceValue;
            }
        }

        public static readonly DependencyProperty PerformanceLabelProperty =
            DependencyProperty.Register(nameof(PerformanceLabel), typeof(string), typeof(EmployeeRowControl));

        public string PerformanceLabel
        {
            get => (string)GetValue(PerformanceLabelProperty);
            set => SetValue(PerformanceLabelProperty, value);
        }

        public static readonly DependencyProperty AvatarUrlProperty =
            DependencyProperty.Register(nameof(AvatarUrl), typeof(string), typeof(EmployeeRowControl));

        public string AvatarUrl
        {
            get => (string)GetValue(AvatarUrlProperty);
            set => SetValue(AvatarUrlProperty, value);
        }

        public string DepartmentIcon
        {
            get => (string)GetValue(DepartmentIconProperty);
            set => SetValue(DepartmentIconProperty, value);
        }

        public static readonly DependencyProperty DepartmentIconProperty =
            DependencyProperty.Register(nameof(DepartmentIcon), typeof(string), typeof(EmployeeRowControl));
    }
}
