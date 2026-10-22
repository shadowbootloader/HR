using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HR.Pages
{
    public partial class DepartmentDetailPage : Page
    {
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
            if (_department == null) return;

            DeptTitleText.Text = _department.Name;
            ManagerNameText.Text = _department.Manager;
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

        private void EditDepartment_Click(object sender, RoutedEventArgs e) { }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }
    }
}