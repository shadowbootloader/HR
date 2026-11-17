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
using HR.Services.Auth;
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

        public bool LogoutRequested { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            var current = AuthSession.Current;
            CurrentUserName.Text = current?.DisplayName ?? current?.Username ?? "";
            CurrentUserRole.Text = current?.RoleLabel ?? "";
            UsersBtn.Visibility = current?.IsAdmin == true ? Visibility.Visible : Visibility.Collapsed;

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
                        "رهبری", "ارتباط", "همکاری",
                        "حل مسئله", "نتیجه‌گرایی", "تفکر راهبردی"
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
                    Labels = new[] { "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "فروردین", "اردیبهشت" },
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
            WireNavigation(page);
            PageName.Text = title;
            DataPanel.Navigate(page);
        }

        private void OnChildNavigation(Page page)
        {
            var title = page switch
            {
                EmployeeListPage => "کارکنان",
                AddEmployeePage => "پرونده کارمند",
                DepartmentsPage => "دپارتمان‌ها",
                AddDepartmentPage => "دپارتمان",
                DepartmentDetailPage => "جزئیات دپارتمان",
                _ => PageName.Text
            };
            Navigate(page, title);
        }

        private void WireNavigation(Page page)
        {
            switch (page)
            {
                case EmployeeListPage list:
                    list.NavigationRequested -= OnChildNavigation;
                    list.NavigationRequested += OnChildNavigation;
                    break;
                case AddEmployeePage addEmployee:
                    addEmployee.NavigationRequested -= OnChildNavigation;
                    addEmployee.NavigationRequested += OnChildNavigation;
                    break;
                case DepartmentsPage departments:
                    departments.NavigationRequested -= OnChildNavigation;
                    departments.NavigationRequested += OnChildNavigation;
                    break;
                case AddDepartmentPage addDepartment:
                    addDepartment.NavigationRequested -= OnChildNavigation;
                    addDepartment.NavigationRequested += OnChildNavigation;
                    break;
                case DepartmentDetailPage detail:
                    detail.NavigationRequested -= OnChildNavigation;
                    detail.NavigationRequested += OnChildNavigation;
                    break;
            }
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
                var page = GetPage(() => new EmployeeListPage(), "employees");

                Navigate(page, "کارکنان");
                return;
            }

            if (ReferenceEquals(target, DepartmentsBtn))
            {
                var page = GetPage(() => new DepartmentsPage(), "departments");

                Navigate(page, "دپارتمان‌ها");
                return;
            }

            if (ReferenceEquals(target, UsersBtn))
            {
                Navigate(GetPage(() => new UsersPage(), "users"), "کاربران");
                return;
            }

            NotificationService.Info("به‌زودی", "این بخش هنوز پیاده‌سازی نشده است.");
        }

        private void ProfileMenu_Click(object sender, RoutedEventArgs e)
            => NotificationService.Info("پروفایل", $"{CurrentUserName.Text} — {CurrentUserRole.Text}");

        private void SettingsMenu_Click(object sender, RoutedEventArgs e)
            => NotificationService.Info("تنظیمات", "این بخش هنوز پیاده‌سازی نشده است.");

        private void LogoutMenu_Click(object sender, RoutedEventArgs e)
        {
            AuthSession.SignOut();
            LogoutRequested = true;
            Close();
        }

        public void NavigateTo(Page page) => DataPanel.Navigate(page);
    }
}