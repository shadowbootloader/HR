using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HR.Controls;
using HR.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace HR.Pages
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly Dictionary<string, Page> _pages = new();
        private Button? _activeSideMenu;
        private bool _sideMenuOpen = true;

        public MainWindow()
        {
            InitializeComponent();

            // مهم: چارت‌ها باید قبل از DataContext مقدار بگیرند،
            // وگرنه بایندینگ‌ها نال می‌خوانند و هرگز آپدیت نمی‌شوند.
            SetupCharts();
            DataContext = this;

            _activeSideMenu = DashbordBynSide;
            NotificationService.Initialize(NotificationHost);

            ShowDashboard();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ==================== چارت‌ها ====================

        public ISeries[]? CompetencySeries { get; private set; }
        public ISeries[]? PerformanceSeries { get; private set; }
        public PolarAxis[]? AngleAxes { get; private set; }
        public Axis[]? XAxes { get; private set; }
        public Axis[]? YAxes { get; private set; }

        private void SetupCharts()
        {
            CompetencySeries = new ISeries[]
            {
                new PolarLineSeries<double>
                {
                    Values = new double[] { 4.6, 4.4, 4.5, 4.3, 4.6, 4.2 },
                    Fill = new SolidColorPaint(SKColor.Parse("#3B82F6").WithAlpha(70)),
                    Stroke = new SolidColorPaint(SKColor.Parse("#3B82F6"), 2.5f),
                    GeometrySize = 8,
                    GeometryFill = new SolidColorPaint(SKColors.White),
                    GeometryStroke = new SolidColorPaint(SKColor.Parse("#3B82F6"), 2),
                    LineSmoothness = 0.2,
                    IsClosed = true
                }
            };

            AngleAxes = new PolarAxis[]
            {
                new PolarAxis
                {
                    Labels = new[]
                    {
                        "Leadership", "Communication", "Collaboration",
                        "Problem Solving", "Results Orientation", "Strategic Thinking"
                    },
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
                    TextSize = 11,
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#334155"))
                }
            };

            PerformanceSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = new double[] { 62, 68, 71, 74, 73, 78, 81, 80, 84, 85, 87, 87 },
                    Fill = new SolidColorPaint(SKColor.Parse("#3B82F6").WithAlpha(40)),
                    Stroke = new SolidColorPaint(SKColor.Parse("#3B82F6"), 2.5f),
                    GeometrySize = 6,
                    GeometryFill = new SolidColorPaint(SKColors.White),
                    GeometryStroke = new SolidColorPaint(SKColor.Parse("#3B82F6"), 2),
                    LineSmoothness = 0.5
                }
            };

            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = new[] { "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Jan", "Feb", "Mar", "Apr", "May" },
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#1E293B")),
                    TextSize = 10
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = 100,
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#1E293B")),
                    TextSize = 10
                }
            };
        }

        // ==================== ناوبری ====================

        private T GetPage<T>(Func<T> factory, string key) where T : Page
        {
            if (_pages.TryGetValue(key, out var existing) && existing is T typed)
                return typed;

            var page = factory();
            _pages[key] = page;
            return page;
        }

        private void Navigate(Page page, string title)
        {
            PageName.Text = title;
            DataPanel.Navigate(page);
        }

        private void ShowDashboard()
        {
            PageName.Text = "مرور کلی پرونده";
            DataPanel.Navigate(GetPage(() => new EmployeeAssessmentCenterPage(), "assessment"));
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DataPanel.CanGoBack)
                DataPanel.GoBack();
            else
                ShowDashboard();
        }

        private void HamBtn_Click(object sender, RoutedEventArgs e)
        {
            _sideMenuOpen = !_sideMenuOpen;
            RightMenu.Width = _sideMenuOpen ? double.NaN : 60;

            foreach (var button in NavPanel.Children.OfType<Button>())
                SetButtonTextVisible(button, _sideMenuOpen);
        }

        /// <summary>متن دکمه‌های منو را مخفی/نمایان می‌کند (بدون وابستگی به sender).</summary>
        private static void SetButtonTextVisible(Button button, bool visible)
        {
            var stack = FindVisualChild<StackPanel>(button);
            if (stack == null) return;

            foreach (var child in stack.Children.OfType<TextBlock>())
                child.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typed) return typed;

                var nested = FindVisualChild<T>(child);
                if (nested != null) return nested;
            }

            return null;
        }

        private void SideMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button target) return;

            _activeSideMenu?.Style = (Style)FindResource("NavButton");
            target.Style = (Style)FindResource("NavButtonActive");
            _activeSideMenu = target;

            if (ReferenceEquals(target, DashbordBynSide))
            {
                ShowDashboard();
                return;
            }

            if (ReferenceEquals(target, EmployeesBtn))
            {
                var page = GetPage(() =>
                {
                    var list = new EmployeeListPage();
                    list.NavigationRequested += p => Navigate(p, PageName.Text);
                    return list;
                }, "employees");

                Navigate(page, "کارکنان");
                return;
            }

            if (ReferenceEquals(target, DepartmentsBtn))
            {
                var page = GetPage(() =>
                {
                    var departments = new DepartmentsPage();
                    departments.NavigationRequested += p => Navigate(p, PageName.Text);
                    return departments;
                }, "departments");

                Navigate(page, "دپارتمان‌ها");
            }
        }

        public void NavigateTo(Page page) => DataPanel.Navigate(page);
    }
}