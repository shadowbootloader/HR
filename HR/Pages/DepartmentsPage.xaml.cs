using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HR.Controls;
using MaterialDesignThemes.Wpf;
using System.Windows;
using static HR.Controls.DepartmentCardControl;

namespace HR.Pages
{
    public partial class DepartmentsPage : Page
    {
        public event Action<Page> NavigationRequested;

        public ObservableCollection<DepartmentModel> Departments { get; set; }

        public DepartmentsPage()
        {
            InitializeComponent();
            LoadSampleData();
            DepartmentItems.ItemsSource = Departments;
            UpdateSideCards();
        }

        private void LoadSampleData()
        {
            Departments = new ObservableCollection<DepartmentModel>
            {
                Make("منابع انسانی", "سارا محمدی", 12, 5, 88, 75, 70, "پایدار", "#14532D", "#86EFAC", PackIconKind.AccountGroup,CircleColor.LawnGreen),
                Make("فناوری اطلاعات", "علی رضایی", 24, 9, 91, 82, 85, "رشد", "#1E3A5F", "#93C5FD", PackIconKind.DesktopClassic,CircleColor.Blue),
                Make("مالی و حسابداری", "مریم کاظمی", 9, 3, 79, 60, 55, "پایدار", "#14532D", "#86EFAC", PackIconKind.Cash,CircleColor.LawnGreen),
                Make("فروش", "حسین نوری", 18, 7, 64, 45, 40, "نیاز به توجه", "#7F1D1D", "#FECACA", PackIconKind.CartOutline,CircleColor.Red),
                Make("پشتیبانی", "زهرا احمدی", 15, 4, 85, 70, 68, "پایدار", "#14532D", "#86EFAC", PackIconKind.Headset, CircleColor.LawnGreen),
                Make("آموزش و توسعه", "رضا کریمی", 8, 6, 87, 90, 78, "رشد", "#1E3A5F", "#93C5FD", PackIconKind.School,CircleColor.Blue),
            };
        }

        private static DepartmentModel Make(
            string name, string manager, int members, int projects,
            double perf, double eval, double proj, string status,
            string bg, string fg, PackIconKind icon, CircleColor _StatusCircle)
        {
            
            return new DepartmentModel
            {
                Name = name,
                Manager = manager,
                MembersCount = members,
                ProjectsCount = projects,
                Performance = perf,
                EvaluationProgress = eval,
                ProjectProgress = proj,
                Status = status,
                StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bg)),
                StatusForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fg)),
                IconKind = icon,
                StatusCircle = _StatusCircle,

            };
        }

        private void UpdateSideCards()
        {
            CardDepartmentsCount.ValueText = Departments.Count.ToString();
            CardTotalMembers.ValueText = Departments.Sum(d => d.MembersCount).ToString();
            CardAveragePerformance.ValueText = Departments.Average(d => d.Performance).ToString("0") + "%";
        }

        private void DepartmentCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement element) return;

            var model = element.DataContext as DepartmentModel;
            NavigationRequested?.Invoke(new DepartmentDetailPage(model));
        }

        private void BtnAddDepartment_Click(object sender, RoutedEventArgs e)
            => NavigationRequested?.Invoke(new AddDepartmentPage());
    }

    public class DepartmentModel
    {
        public string Name { get; set; }
        public string Manager { get; set; }
        public int MembersCount { get; set; }
        public int ProjectsCount { get; set; }
        public double Performance { get; set; }
        public double EvaluationProgress { get; set; }
        public double ProjectProgress { get; set; }
        public string Status { get; set; }
        public Brush StatusBackground { get; set; }
        public Brush StatusForeground { get; set; }
        public PackIconKind IconKind { get; set; }
        public CircleColor StatusCircle { get; set; }
    }
}