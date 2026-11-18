using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using HR.Models;
using HR.Services;
using HR.Services.Auth;
using HR.Services.Data;
using MaterialDesignThemes.Wpf;

namespace HR.Pages
{
    public partial class DepartmentDetailPage : Page
    {
        public event Action<Page>? NavigationRequested;

        private readonly DepartmentModel? _department;
        private static readonly PersianCalendar Calendar = new();

        public DepartmentDetailPage() : this(null) { }

        public DepartmentDetailPage(DepartmentModel? department)
        {
            InitializeComponent();
            _department = department;
            Loaded += async (_, _) => await BindDepartmentAsync();
        }

        private async System.Threading.Tasks.Task BindDepartmentAsync()
        {
            if (AuthSession.Current?.CanManagePeople != true)
            {
                EditDepartmentBtn.Visibility = Visibility.Collapsed;
                DeleteDepartmentBtn.Visibility = Visibility.Collapsed;
            }

            if (_department == null)
            {
                DeptTitleText.Text = "دپارتمانی انتخاب نشده";
                return;
            }

            try
            {
                using var service = new DepartmentService();
                var dept = await service.GetByIdAsync(_department.Id);
                var members = dept == null
                    ? new List<DepartmentMemberRow>()
                    : await service.ListMembersAsync(_department.Id);

                if (dept == null)
                {
                    DeptTitleText.Text = _department.Name;
                    return;
                }

                DeptTitleText.Text = dept.Title;
                var manager = Person(dept.Manager);
                var deputy = Person(dept.Deputy);
                var cashier = Person(dept.CashCustodian);
                ManagerNameText.Text = "مدیر: " + manager;
                PerformanceLabelText.Text = dept.IsActive ? "فعال" : "غیرفعال";

                var logo = ImagePicker.Load(dept.LogoPath);
                DeptLogoBrush.ImageSource = logo;
                DeptLogoEllipse.Visibility = logo == null ? Visibility.Collapsed : Visibility.Visible;

                var capacity = dept.ApprovedCapacity?.ToString() ?? "—";
                KpiMembers.Title = "تعداد کارکنان";
                KpiMembers.ValueText = members.Count.ToString();
                KpiMembers.SubText = "از ظرفیت " + capacity;

                KpiPerformance.Title = "رده حفاظتی";
                KpiPerformance.ValueText = string.IsNullOrWhiteSpace(dept.SecurityLevel) ? "—" : dept.SecurityLevel;
                KpiPerformance.SubText = "ثبت‌شده در پرونده";

                var office = dept.Offices.FirstOrDefault();
                KpiEvaluation.Title = "محل";
                KpiEvaluation.ValueText = office == null ? "—" : office.Name;
                KpiEvaluation.SubText = office == null ? "دفتری ثبت نشده" : string.Join("، ", new[] { office.City, office.Address }.Where(text => !string.IsNullOrWhiteSpace(text)));

                KpiProjects.Title = "زیرمجموعه";
                KpiProjects.ValueText = dept.Children.Count.ToString();
                KpiProjects.SubText = "دپارتمان فرزند";

                var recent = members.Count(item => item.StartDate.HasValue && item.StartDate.Value.Date >= DateTime.Today.AddDays(-30));
                KpiProjectProgress.Title = "استخدام اخیر";
                KpiProjectProgress.ValueText = recent.ToString();
                KpiProjectProgress.SubText = "۳۰ روز اخیر";

                KpiStatus.Title = "وضعیت دپارتمان";
                KpiStatus.ValueText = dept.IsActive ? "فعال" : "غیرفعال";
                KpiStatus.SubText = dept.Parent == null ? "بدون والد" : "والد: " + dept.Parent.Title;

                LeaderPanel.ManagerName = manager;
                LeaderPanel.ManagerTitle = "مدیر " + dept.Title;
                LeaderPanel.DeputyName = deputy;
                LeaderPanel.PettyCashName = cashier;
                LeaderPanel.LeadershipScore = 0;
                LeaderPanel.SatisfactionScore = 0;
                LeaderPanel.GrowthScore = 0;
                LeaderPanel.SetActivities(members
                    .Where(item => item.StartDate.HasValue)
                    .OrderByDescending(item => item.StartDate)
                    .Take(5)
                    .Select(item => new DeptActivityItem
                    {
                        Title = item.FullName,
                        Subtitle = string.IsNullOrWhiteSpace(item.Position) ? "شروع همکاری" : item.Position,
                        Date = ToPersian(item.StartDate),
                        IconKind = PackIconKind.AccountPlus
                    }));

                FinanceCard.Budget = (long)(dept.AnnualBudget ?? 0);
                FinanceCard.Spent = 0;

                MembersList.ItemsSource = members;
                MembersEmpty.Visibility = members.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                NotificationService.Warning("دپارتمان", ex.Message);
            }
        }

        private static string Person(HR.Data.Entities.Employee? employee)
            => employee == null ? "—" : $"{employee.FirstName} {employee.LastName}".Trim();

        private static string ToPersian(DateTime? date)
        {
            if (date == null) return "—";
            var value = date.Value;
            return $"{Calendar.GetYear(value):0000}/{Calendar.GetMonth(value):00}/{Calendar.GetDayOfMonth(value):00}";
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
