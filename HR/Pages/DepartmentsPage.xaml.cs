using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HR.Controls;
using HR.Services.Data;
using MaterialDesignThemes.Wpf;
using static HR.Controls.DepartmentCardControl;

namespace HR.Pages
{
    public partial class DepartmentsPage : Page
    {
        public event Action<Page>? NavigationRequested;

        public ObservableCollection<DepartmentModel> Departments { get; set; } = new();

        private readonly IDepartmentService _departmentService = new DepartmentService();

        public DepartmentsPage()
        {
            InitializeComponent();
            DepartmentItems.ItemsSource = Departments;
            Loaded += async (_, _) => await LoadDepartmentsAsync();
            Unloaded += (_, _) => (_departmentService as IDisposable)?.Dispose();
        }

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var list = await _departmentService.GetAllAsync();
                var counts = await _departmentService.CountMembersAsync();

                Departments.Clear();

                // رنگ‌ها و آیکون‌ها را بر اساس نام یا به صورت چرخشی اختصاص می‌دهیم
                var styles = new[]
                {
                    (PackIconKind.AccountGroup, CircleColor.LawnGreen, "#14532D", "#86EFAC"),
                    (PackIconKind.DesktopClassic, CircleColor.Blue, "#1E3A5F", "#93C5FD"),
                    (PackIconKind.Cash, CircleColor.LawnGreen, "#14532D", "#86EFAC"),
                    (PackIconKind.CartOutline, CircleColor.Red, "#7F1D1D", "#FECACA"),
                    (PackIconKind.Headset, CircleColor.LawnGreen, "#14532D", "#86EFAC"),
                    (PackIconKind.School, CircleColor.Blue, "#1E3A5F", "#93C5FD"),
                };

                int i = 0;
                foreach (var d in list)
                {
                    var style = styles[i % styles.Length];
                    i++;

                    Departments.Add(new DepartmentModel
                    {
                        Id = d.DepartmentId,
                        Name = d.Title,
                        Manager = d.Manager != null ? $"{d.Manager.FirstName} {d.Manager.LastName}".Trim() : "—",
                        MembersCount = counts.TryGetValue(d.DepartmentId, out var members) ? members : 0,
                        ProjectsCount = 0,
                        Performance = 0,
                        EvaluationProgress = 0,
                        ProjectProgress = 0,
                        Status = d.IsActive ? "فعال" : "غیرفعال",
                        StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(style.Item3)),
                        StatusForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(style.Item4)),
                        IconKind = style.Item1,
                        StatusCircle = style.Item2
                    });
                }

                UpdateSideCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در بارگذاری دپارتمان‌ها:\n" + ex.Message, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSideCards()
        {
            CardDepartmentsCount.ValueText = Departments.Count.ToString();
            CardTotalMembers.ValueText = Departments.Sum(d => d.MembersCount).ToString();
            CardAveragePerformance.ValueText = Departments.Any()
                ? Departments.Average(d => d.Performance).ToString("0") + "%"
                : "0%";
        }

        private void DeptTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void DepartmentCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement element) return;

            var model = element.DataContext as DepartmentModel;
            if (model == null) return;

            NavigationRequested?.Invoke(new DepartmentDetailPage(model));
        }

        private void BtnAddDepartment_Click(object sender, RoutedEventArgs e)
            => NavigationRequested?.Invoke(new AddDepartmentPage());
    }

    public class DepartmentModel
    {
        public short Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Manager { get; set; } = string.Empty;
        public int MembersCount { get; set; }
        public int ProjectsCount { get; set; }
        public double Performance { get; set; }
        public double EvaluationProgress { get; set; }
        public double ProjectProgress { get; set; }
        public string Status { get; set; } = string.Empty;
        public Brush StatusBackground { get; set; } = Brushes.Transparent;
        public Brush StatusForeground { get; set; } = Brushes.Black;
        public PackIconKind IconKind { get; set; }
        public CircleColor StatusCircle { get; set; }
    }
}
