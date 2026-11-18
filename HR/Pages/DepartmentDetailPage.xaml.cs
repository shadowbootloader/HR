using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using HR.Controls;
using HR.Data;
using HR.Models;
using HR.Services;
using HR.Services.Auth;
using HR.Services.Data;
using HR.Services.Org;
using MaterialDesignThemes.Wpf;

namespace HR.Pages
{
    public partial class DepartmentDetailPage : Page
    {
        public event Action<Page>? NavigationRequested;

        private readonly DepartmentModel? _department;
        private static readonly PersianCalendar Calendar = new();
        private readonly List<EmployeeModel> _staff = new();
        private int _staffPage = 1;
        private const int StaffPageSize = 8;
        private bool _staffLoaded;
        private bool _chartLoaded;

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
                ShowTab(0);
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

        private async void DeptTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || DeptTabs == null) return;
            var index = DeptTabs.SelectedIndex;
            ShowTab(index);
            if (index == 1) await LoadStaffAsync();
            if (index == 2) await LoadChartAsync();
        }

        private void ShowTab(int index)
        {
            if (SummaryPanel == null) return;
            SummaryPanel.Visibility = index == 0 ? Visibility.Visible : Visibility.Collapsed;
            StaffPanel.Visibility = index == 1 ? Visibility.Visible : Visibility.Collapsed;
            ChartPanel.Visibility = index == 2 ? Visibility.Visible : Visibility.Collapsed;
            WorkPanel.Visibility = index is 3 or 4 ? Visibility.Visible : Visibility.Collapsed;
            ProjectsCard.Visibility = index == 3 ? Visibility.Visible : Visibility.Collapsed;
            FinanceCard.Visibility = index == 4 ? Visibility.Visible : Visibility.Collapsed;
            ContractsCard.Visibility = index == 4 ? Visibility.Visible : Visibility.Collapsed;
            ReportsPanel.Visibility = index == 5 ? Visibility.Visible : Visibility.Collapsed;
        }

        private async System.Threading.Tasks.Task LoadStaffAsync()
        {
            if (_department == null || _staffLoaded) 
            {
                RenderStaff();
                return;
            }
            using var people = new EmployeeService();
            var rows = await people.GetAllAsync(includeDeleted: true);
            _staff.Clear();
            foreach (var employee in rows.Where(item => item.Supplement?.DepartmentId == _department.Id))
                _staff.Add(EmployeeListPage.MapToModel(employee));
            _staffLoaded = true;
            EmployeeRowControl.NavigationRequested -= OnStaffNavigate;
            EmployeeRowControl.NavigationRequested += OnStaffNavigate;
            RenderStaff();
        }

        private void OnStaffNavigate(Page page) => NavigationRequested?.Invoke(page);

        private void RenderStaff()
        {
            var term = StaffSearch.Text?.Trim() ?? string.Empty;
            var filtered = _staff.Where(item =>
                string.IsNullOrWhiteSpace(term) ||
                item.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                item.PersonnelCode.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                item.Position.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
            var pages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)StaffPageSize));
            if (_staffPage > pages) _staffPage = pages;
            StaffList.Items.Clear();
            foreach (var item in filtered.Skip((_staffPage - 1) * StaffPageSize).Take(StaffPageSize))
                StaffList.Items.Add(new EmployeeRowControl { DataContext = item });
            StaffEmpty.Visibility = filtered.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            StaffPageText.Text = filtered.Count == 0
                ? "موردی یافت نشد"
                : $"صفحه {_staffPage} از {pages} · {filtered.Count} نفر";
        }

        private void StaffSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _staffPage = 1;
            if (_staffLoaded) RenderStaff();
        }

        private void StaffPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_staffPage > 1) { _staffPage--; RenderStaff(); }
        }

        private void StaffNext_Click(object sender, RoutedEventArgs e)
        {
            _staffPage++;
            RenderStaff();
        }

        private async System.Threading.Tasks.Task LoadChartAsync()
        {
            if (_department == null || _chartLoaded) return;
            await using var db = new PersonnelDbContext();
            var tree = OrgChartLayout.Build(
                await OrgChartPage.LoadDepartmentsAsync(db),
                await OrgChartPage.LoadPeopleAsync(db),
                _department.Id);
            DeptChart.Show(tree);
            _chartLoaded = true;
        }
    }
}
