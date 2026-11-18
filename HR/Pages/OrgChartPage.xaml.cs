using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HR.Data;
using HR.Services.Data;
using HR.Services.Org;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Web.WebView2.Core;

namespace HR.Pages
{
    public partial class OrgChartPage : Page
    {
        public event Action<Page>? NavigationRequested;

        private bool _ready;
        private string _json = "[]";

        public OrgChartPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await StartAsync();
        }

        private async Task StartAsync()
        {
            await ChartView.EnsureCoreWebView2Async();
            var folder = AppDomain.CurrentDomain.BaseDirectory;
            ChartView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "hr.assets", folder, CoreWebView2HostResourceAccessKind.Allow);
            ChartView.CoreWebView2.WebMessageReceived += async (_, args) => await OnChartMessageAsync(args.TryGetWebMessageAsString());
            ChartView.CoreWebView2.NavigationCompleted += async (_, args) =>
            {
                if (!args.IsSuccess) return;
                _ready = true;
                await PushDataAsync();
            };
            ChartView.CoreWebView2.Navigate("https://hr.assets/Assets/orgchart/index.html");
            await ReloadDataAsync();
        }

        private async Task ReloadDataAsync()
        {
            await using var db = new PersonnelDbContext();
            var departments = await db.Departments.AsNoTracking()
                .Select(department => new OrgDepartment
                {
                    Id = department.DepartmentId,
                    Title = department.Title,
                    ParentId = department.ParentDepartmentId,
                    ManagerName = department.Manager == null
                        ? null
                        : (department.Manager.FirstName + " " + department.Manager.LastName).Trim()
                })
                .ToListAsync();

            var members = await db.EmployeeSupplements.AsNoTracking()
                .Select(item => new OrgMember
                {
                    EmployeeId = item.EmployeeId,
                    DepartmentId = item.DepartmentId,
                    FullName = (item.Employee.FirstName + " " + item.Employee.LastName).Trim(),
                    Position = item.PositionTitle,
                    IsDeleted = item.Employee.IsDeleted
                })
                .ToListAsync();

            _json = OrgChartMapper.ToJson(OrgChartMapper.Build(
                departments,
                members,
                IncludeEmployees.IsChecked == true));
            await PushDataAsync();
        }

        private async Task PushDataAsync()
        {
            if (!_ready) return;
            var argument = JsonSerializer.Serialize(_json);
            await ChartView.ExecuteScriptAsync("loadOrg(" + argument + ")");
        }

        private async Task OnChartMessageAsync(string? message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            ChartClick? click;
            try
            {
                click = JsonSerializer.Deserialize<ChartClick>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException)
            {
                return;
            }

            if (click == null) return;
            if (click.Kind == "department" && short.TryParse(click.EntityId, out var departmentId))
            {
                var department = await new DepartmentService().GetByIdAsync(departmentId);
                if (department == null) return;
                NavigationRequested?.Invoke(new DepartmentDetailPage(new DepartmentModel
                {
                    Id = department.DepartmentId,
                    Name = department.Title,
                    Manager = department.Manager == null ? "—" : $"{department.Manager.FirstName} {department.Manager.LastName}".Trim(),
                    LogoPath = department.LogoPath,
                    Status = department.IsActive ? "فعال" : "غیرفعال",
                    StatusBackground = Brushes.Transparent,
                    StatusForeground = Brushes.White,
                    IconKind = PackIconKind.Domain
                }));
            }
            else if (click.Kind == "employee" && Guid.TryParse(click.EntityId, out var employeeId))
            {
                var employee = await new EmployeeService().GetByIdAsync(employeeId);
                if (employee == null) return;
                NavigationRequested?.Invoke(new EmployeOverview(new EmployeeModel
                {
                    Id = employee.EmployeeId,
                    FullName = $"{employee.FirstName} {employee.LastName}".Trim(),
                    Email = employee.Email ?? "",
                    NationalCode = employee.NationalCode ?? "",
                    PersonnelCode = employee.Supplement?.PersonnelCode ?? "—",
                    Position = employee.Supplement?.PositionTitle ?? "—",
                    Department = employee.Supplement?.Department?.Title ?? "—",
                    Status = employee.IsDeleted ? "حذف‌شده" : "فعال",
                    IsDeleted = employee.IsDeleted
                }));
            }
        }

        private async void Search_Click(object sender, RoutedEventArgs e) => await RunAsync("searchOrg");
        private async void Fit_Click(object sender, RoutedEventArgs e) => await ChartView.ExecuteScriptAsync("fitOrg()");
        private async void Expand_Click(object sender, RoutedEventArgs e) => await ChartView.ExecuteScriptAsync("expandOrg()");
        private async void Collapse_Click(object sender, RoutedEventArgs e) => await ChartView.ExecuteScriptAsync("collapseOrg()");
        private async void Png_Click(object sender, RoutedEventArgs e) => await ChartView.ExecuteScriptAsync("exportPng()");
        private async void Svg_Click(object sender, RoutedEventArgs e) => await ChartView.ExecuteScriptAsync("exportSvg()");
        private async void IncludeEmployees_Changed(object sender, RoutedEventArgs e) => await ReloadDataAsync();

        private async void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) await RunAsync("searchOrg");
        }

        private async Task RunAsync(string function)
        {
            if (!_ready) return;
            await ChartView.ExecuteScriptAsync(function + "(" + JsonSerializer.Serialize(SearchBox.Text ?? "") + ")");
        }

        private sealed class ChartClick
        {
            public string? Kind { get; set; }
            public string? EntityId { get; set; }
        }
    }
}
