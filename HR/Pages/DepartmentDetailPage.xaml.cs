using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using HR.Services;
using HR.Services.Auth;
using HR.Services.Data;

namespace HR.Pages
{
    public partial class DepartmentDetailPage : Page
    {
        public event Action<Page>? NavigationRequested;

        private readonly DepartmentModel? _department;

        public DepartmentDetailPage() : this(null) { }

        public DepartmentDetailPage(DepartmentModel? department)
        {
            InitializeComponent();
            _department = department;

            Loaded += (_, __) => BindDepartment();
        }

        private void BindDepartment()
        {
            if (AuthSession.Current?.CanManagePeople != true)
            {
                EditDepartmentBtn.Visibility = Visibility.Collapsed;
                DeleteDepartmentBtn.Visibility = Visibility.Collapsed;
            }

            if (_department == null) return;

            DeptTitleText.Text = _department.Name;
            ManagerNameText.Text = _department.Manager;
            var logo = ImagePicker.Load(_department.LogoPath);
            DeptLogoBrush.ImageSource = logo;
            DeptLogoEllipse.Visibility = logo == null ? Visibility.Collapsed : Visibility.Visible;
            PerformanceLabelText.Text = _department.Status;

            KpiMembers.Title = "تعداد کل کارکنان";
            KpiMembers.ValueText = _department.MembersCount.ToString();

            KpiPerformance.Title = "میانگین عملکرد";
            KpiPerformance.ValueText = _department.Performance.ToString("0") + "٪";

            KpiEvaluation.Title = "پیشرفت ارزیابی";
            KpiEvaluation.ValueText = _department.EvaluationProgress.ToString("0") + "٪";

            KpiProjects.Title = "پروژه‌های فعال";
            KpiProjects.ValueText = _department.ProjectsCount.ToString();

            KpiProjectProgress.Title = "پیشرفت پروژه‌ها";
            KpiProjectProgress.ValueText = _department.ProjectProgress.ToString("0") + "٪";

            KpiStatus.Title = "وضعیت دپارتمان";
            KpiStatus.ValueText = _department.Status;

            LeaderPanel.ManagerName = _department.Manager;
            LeaderPanel.ManagerTitle = "مدیر " + _department.Name;

            // FinanceCard.UsedPercent را Rebuild() خودش ست می‌کند،
            // پس اگر مقدار دستی بدهیم با تغییر Budget/Spent پاک می‌شود.
            FinanceCard.UsedPercent = _department.ProjectProgress;
        }

        private void EditDepartment_Click(object sender, RoutedEventArgs e)
        {
            if (_department == null) return;
            NavigationRequested?.Invoke(new AddDepartmentPage(_department.Id));
        }

        private async void DeleteDepartment_Click(object sender, RoutedEventArgs e)
        {
            if (_department == null) return;

            var answer = MessageBox.Show(
                $"دپارتمان «{_department.Name}» حذف شود؟",
                "حذف دپارتمان",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (answer != MessageBoxResult.Yes) return;

            try
            {
                using var service = new DepartmentService();
                var ok = await service.DeleteAsync(_department.Id);
                if (!ok)
                {
                    NotificationService.Warning("حذف دپارتمان", "دپارتمان یافت نشد.");
                    return;
                }

                NotificationService.Success("حذف شد", $"«{_department.Name}» حذف شد.");
                NavigationRequested?.Invoke(new DepartmentsPage());
            }
            catch (Exception ex)
            {
                NotificationService.Warning("حذف دپارتمان", ex.Message);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        private void DeptTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}