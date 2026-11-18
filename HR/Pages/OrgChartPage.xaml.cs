using System.Windows;
using System.Windows.Controls;
using HR.Data;
using HR.Services.Data;
using HR.Services.Org;
using Microsoft.EntityFrameworkCore;

namespace HR.Pages
{
    public partial class OrgChartPage : Page
    {
        public event Action<Page>? NavigationRequested;

        public OrgChartPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await ReloadAsync();
        }

        private async Task ReloadAsync()
        {
            try
            {
                await using var db = new PersonnelDbContext();
                await new DepartmentService(db).EnsureHeadquartersAsync();
                var tree = OrgChartLayout.Build(await LoadDepartmentsAsync(db), await LoadPeopleAsync(db));
                Board.Show(tree);
            }
            catch (Exception ex)
            {
                Board.Show(new OrgBox { Title = "چارت", Subtitle = ex.Message, Kind = "empty" });
            }
        }

        internal static async Task<List<OrgDepartmentInput>> LoadDepartmentsAsync(PersonnelDbContext db)
            => await db.Departments.AsNoTracking()
                .Select(department => new OrgDepartmentInput
                {
                    Id = department.DepartmentId,
                    Title = department.Title,
                    ParentId = department.ParentDepartmentId,
                    LogoPath = department.LogoPath,
                    ManagerId = department.ManagerEmployeeId,
                    ManagerName = department.Manager == null
                        ? null
                        : (department.Manager.FirstName + " " + department.Manager.LastName).Trim()
                })
                .ToListAsync();

        internal static async Task<List<OrgPersonInput>> LoadPeopleAsync(PersonnelDbContext db)
            => await db.EmployeeSupplements.AsNoTracking()
                .Select(item => new OrgPersonInput
                {
                    EmployeeId = item.EmployeeId,
                    DepartmentId = item.DepartmentId,
                    Name = (item.Employee.FirstName + " " + item.Employee.LastName).Trim(),
                    Position = item.PositionTitle,
                    IsDeleted = item.Employee.IsDeleted,
                    PhotoPath = item.Employee.PersonFiles
                        .Where(file => file.FileType.Title.Contains("عکس"))
                        .Select(file => file.FileAddress)
                        .FirstOrDefault()
                })
                .ToListAsync();

        private void Zoom_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Board != null) Board.SetZoom(e.NewValue);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
            => NavigationRequested?.Invoke(new DepartmentsPage());
    }
}
