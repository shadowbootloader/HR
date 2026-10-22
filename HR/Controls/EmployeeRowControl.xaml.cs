using System.Windows;
using System.Windows.Controls;
using HR.Pages;

namespace HR.Controls
{
    public partial class EmployeeRowControl : UserControl
    {
        public EmployeeModel? Employee => DataContext as EmployeeModel;
        private void ViewBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationRequested?.Invoke(new EmployeOverview(Employee));
        }


        public static event Action<Page> NavigationRequested;
        public EmployeeRowControl()
        {
            InitializeComponent();

        }

        public static readonly DependencyProperty FullNameProperty =
            DependencyProperty.Register(nameof(FullName), typeof(string), typeof(EmployeeRowControl));

        public string FullName
        {
            get => (string)GetValue(FullNameProperty);
            set => SetValue(FullNameProperty, value);
        }

        public static readonly DependencyProperty EmailProperty =
            DependencyProperty.Register(nameof(Email), typeof(string), typeof(EmployeeRowControl));

        public string Email
        {
            get => (string)GetValue(EmailProperty);
            set => SetValue(EmailProperty, value);
        }

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(string), typeof(EmployeeRowControl));

        public string Position
        {
            get => (string)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        public static readonly DependencyProperty DepartmentProperty =
            DependencyProperty.Register(nameof(Department), typeof(string), typeof(EmployeeRowControl));

        public string Department
        {
            get => (string)GetValue(DepartmentProperty);
            set => SetValue(DepartmentProperty, value);
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(nameof(Status), typeof(string), typeof(EmployeeRowControl),
                new PropertyMetadata("Active"));

        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set
            {
                SetValue(StatusProperty, value);
                
            }
        }

        public static readonly DependencyProperty PerformanceValueProperty =
            DependencyProperty.Register(nameof(PerformanceValue), typeof(double), typeof(EmployeeRowControl),
                new PropertyMetadata(0.0));

        public double PerformanceValue
        {
            get => (double)GetValue(PerformanceValueProperty);
            set
            {
                SetValue(PerformanceValueProperty, value);
                Performancegauge.Value = PerformanceValue;
            }
        }

        public static readonly DependencyProperty PerformanceLabelProperty =
            DependencyProperty.Register(nameof(PerformanceLabel), typeof(string), typeof(EmployeeRowControl));

        public string PerformanceLabel
        {
            get => (string)GetValue(PerformanceLabelProperty);
            set => SetValue(PerformanceLabelProperty, value);
        }

        public static readonly DependencyProperty AvatarUrlProperty =
            DependencyProperty.Register(nameof(AvatarUrl), typeof(string), typeof(EmployeeRowControl));

        public string AvatarUrl
        {
            get => (string)GetValue(AvatarUrlProperty);
            set => SetValue(AvatarUrlProperty, value);
        }

        public string DepartmentIcon
        {
            get => (string)GetValue(DepartmentIconProperty);
            set => SetValue(DepartmentIconProperty, value);
        }

        public static readonly DependencyProperty DepartmentIconProperty =
            DependencyProperty.Register(nameof(DepartmentIcon), typeof(string), typeof(EmployeeRowControl));

        private void ViewBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationRequested?.Invoke(new EmployeOverview());
        }
    }
}