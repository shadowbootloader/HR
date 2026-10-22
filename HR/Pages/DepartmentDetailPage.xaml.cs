using System.Net.NetworkInformation;
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

            KpiMembers.ValueText = _department.MembersCount.ToString();
            KpiProjects.ValueText = _department.ProjectsCount.ToString();
            KpiPerformance.ValueText = _department.Performance.ToString("0") + "٪";
            KpiEvaluation.ValueText = _department.EvaluationProgress.ToString("0") + "٪";
            KpiProjectProgress.ValueText = _department.ProjectProgress.ToString("0") + "٪";
            KpiStatus.ValueText = _department.Status;

            LeaderPanel.ManagerName = _department.Manager;
            FinanceCard.UsedPercent = _department.Performance;
        }

        private void EditDepartment_Click(object sender, RoutedEventArgs e) { }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }
    }
}