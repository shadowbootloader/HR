using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HR.Data;
using HR.Services;
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
                var departments = await db.Departments.AsNoTracking()
                    .Select(department => new DeptRow
                    {
                        Id = department.DepartmentId,
                        Title = department.Title,
                        ParentId = department.ParentDepartmentId,
                        LogoPath = department.LogoPath,
                        Manager = department.Manager == null
                            ? null
                            : (department.Manager.FirstName + " " + department.Manager.LastName).Trim()
                    })
                    .ToListAsync();

                var counts = await db.EmployeeSupplements.AsNoTracking()
                    .Where(item => item.DepartmentId != null && !item.Employee.IsDeleted)
                    .GroupBy(item => item.DepartmentId)
                    .Select(group => new { Id = group.Key, Count = group.Count() })
                    .ToListAsync();
                var headcount = counts.Where(item => item.Id != null).ToDictionary(item => item.Id!.Value, item => item.Count);

                var people = IncludeEmployees.IsChecked == true
                    ? await db.EmployeeSupplements.AsNoTracking()
                        .Where(item => item.DepartmentId != null && !item.Employee.IsDeleted)
                        .Select(item => new PersonRow
                        {
                            EmployeeId = item.EmployeeId,
                            DepartmentId = item.DepartmentId!.Value,
                            ManagerId = item.DirectManagerEmployeeId,
                            Name = (item.Employee.FirstName + " " + item.Employee.LastName).Trim(),
                            Position = item.PositionTitle
                        })
                        .ToListAsync()
                    : new List<PersonRow>();

                var nodes = departments.ToDictionary(item => item.Id, item => NodeFor(item, headcount));
                var roots = new ObservableCollection<OrgNode>();
                var byId = departments.ToDictionary(item => item.Id);
                foreach (var department in departments.OrderBy(item => item.Title))
                {
                    var node = nodes[department.Id];
                    if (department.ParentId is short parentId && parentId != department.Id && nodes.ContainsKey(parentId) && !Loops(department.Id, parentId, byId))
                        nodes[parentId].Children.Add(node);
                    else
                        roots.Add(node);

                    AddEmployees(node, people.Where(person => person.DepartmentId == department.Id));
                }

                OrgTree.ItemsSource = roots;
                EmptyText.Visibility = roots.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                OrgTree.Visibility = roots.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception ex)
            {
                EmptyText.Text = ex.Message;
                EmptyText.Visibility = Visibility.Visible;
            }
        }

        private static bool Loops(short id, short parentId, IReadOnlyDictionary<short, DeptRow> departments)
        {
            var seen = new HashSet<short> { id };
            var cursor = parentId;
            while (departments.TryGetValue(cursor, out var row))
            {
                if (!seen.Add(cursor)) return true;
                if (row.ParentId is not short next || next == cursor) return false;
                cursor = next;
            }
            return false;
        }

        private static void AddEmployees(OrgNode department, IEnumerable<PersonRow> people)
        {
            var list = people.ToList();
            if (list.Count == 0) return;
            var byId = list.ToDictionary(item => item.EmployeeId);
            var nodes = list.ToDictionary(item => item.EmployeeId, item => new OrgNode
            {
                Title = item.Name,
                Subtitle = string.IsNullOrWhiteSpace(item.Position) ? "کارمند" : item.Position
            });
            foreach (var person in list)
            {
                if (person.ManagerId is Guid managerId && managerId != person.EmployeeId && byId.ContainsKey(managerId) && !ManagerLoop(person.EmployeeId, managerId, byId))
                    nodes[managerId].Children.Add(nodes[person.EmployeeId]);
                else
                    department.Children.Add(nodes[person.EmployeeId]);
            }
        }

        private static bool ManagerLoop(Guid personId, Guid managerId, IReadOnlyDictionary<Guid, PersonRow> people)
        {
            var seen = new HashSet<Guid> { personId };
            var cursor = managerId;
            while (people.TryGetValue(cursor, out var row))
            {
                if (!seen.Add(cursor)) return true;
                if (row.ManagerId is not Guid next || next == cursor) return false;
                cursor = next;
            }
            return false;
        }

        private static OrgNode NodeFor(DeptRow department, IReadOnlyDictionary<short, int> headcount)
        {
            var count = headcount.TryGetValue(department.Id, out var value) ? value : 0;
            var manager = string.IsNullOrWhiteSpace(department.Manager) ? "بدون مدیر" : "مدیر: " + department.Manager;
            return new OrgNode
            {
                Title = department.Title,
                Subtitle = manager + " · " + count + " نفر",
                Logo = ImagePicker.Load(department.LogoPath)
            };
        }

        private async void IncludeEmployees_Changed(object sender, RoutedEventArgs e)
        {
            if (IsLoaded) await ReloadAsync();
        }

        private void Zoom_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ZoomTransform == null) return;
            ZoomTransform.ScaleX = e.NewValue;
            ZoomTransform.ScaleY = e.NewValue;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
            => NavigationRequested?.Invoke(new DepartmentsPage());

        private sealed class DeptRow
        {
            public short Id { get; init; }
            public string Title { get; init; } = string.Empty;
            public short? ParentId { get; init; }
            public string? LogoPath { get; init; }
            public string? Manager { get; init; }
        }

        private sealed class PersonRow
        {
            public Guid EmployeeId { get; init; }
            public short DepartmentId { get; init; }
            public Guid? ManagerId { get; init; }
            public string Name { get; init; } = string.Empty;
            public string Position { get; init; } = string.Empty;
        }

        private sealed class OrgNode
        {
            public string Title { get; set; } = string.Empty;
            public string Subtitle { get; set; } = string.Empty;
            public ImageSource? Logo { get; set; }
            public ObservableCollection<OrgNode> Children { get; } = new();
        }
    }
}
