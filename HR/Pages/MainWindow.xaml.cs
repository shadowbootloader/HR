using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HR.Controls;
using HR.Services;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MaterialDesignThemes.Wpf;
using SkiaSharp;
using Chart = HR.Controls.Chart;

namespace HR.Pages
{
    public partial class MainWindow : Window
    {
        bool SideMenu_IsOpen = true;
        Button ActiveSideMenu;
        EmployeeListPage EmpListPage = new EmployeeListPage();
        DepartmentsPage AllDepartmentPage = new DepartmentsPage();
        DepartmentDetailPage DPDetail = new DepartmentDetailPage();
        EmployeeAssessmentCenterPage EACP = new EmployeeAssessmentCenterPage();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            SetupCharts();
            ActiveSideMenu = DashbordBynSide;

            DataPanel.Navigate(EACP);
            NotificationService.Initialize(NotificationHost);

            AllDepartmentPage.NavigationRequested += page =>
            {
                DataPanel.Navigate(page);
            };
            EmployeeRowControl.NavigationRequested += page =>
            {
                DataPanel.Navigate(page);
            };
            EmpListPage.NavigationRequested += page =>
            {
                DataPanel.Navigate( page);
            };
            List<ChartEmployee> AllData = new List<ChartEmployee>() {
            new ChartEmployee { Id = 1, ManagerId = null, FullName = "علی رضایی", Position = "مدیر فروش", DepartmentId = 1,DepartmentName="فتی",Role="CEO" },
            new ChartEmployee { Id = 2, ManagerId = 1, FullName = "مریم احمدی", Position = "سرپرست تیم A", DepartmentId = 1 ,DepartmentName="فتی",Role="CEO"},
            new ChartEmployee { Id = 3, ManagerId = 1, FullName = "حسین محمدی", Position = "سرپرست تیم B", DepartmentId = 1 ,DepartmentName="فتی",Role="CEO"},
            new ChartEmployee { Id = 4, ManagerId = 2, FullName = "سارا کریمی", Position = "کارشناس فروش", DepartmentId = 1 ,DepartmentName="فتی",Role="CEO"},
            new ChartEmployee { Id = 5, ManagerId = 2, FullName = "رضا نوری", Position = "کارشناس فروش", DepartmentId = 1,DepartmentName="فتی",Role="CEO" },
            new ChartEmployee {Id = 6, ManagerId = 3, FullName = "فاطمه موسوی", Position = "کارشناس فروش", DepartmentId = 1, DepartmentName = "فتی", Role = "CEO"},

            // ========== دپارتمان مالی (DepartmentId = 2) ==========
            //new ChartEmployee { Id = 10, ManagerId = null, FullName = "محمد حسینی", Position = "مدیر مالی", DepartmentId = 2 },
            new ChartEmployee { Id = 11, ManagerId = 1, FullName = "زهرا اکبری", Position = "حسابدار ارشد", DepartmentId = 2 ,DepartmentName="فتی",Role="CEO"},
            new ChartEmployee { Id = 12, ManagerId = 1, FullName = "امیر جعفری", Position = "کارشناس بودجه", DepartmentId = 2 ,DepartmentName="فتی",Role="CEO"},
            new ChartEmployee {Id = 13, ManagerId = 1, FullName = "نازنین صالحی", Position = "حسابدار", DepartmentId = 2, DepartmentName = "فتی", Role = "CEO"},

            // ========== دپارتمان فناوری (DepartmentId = 3) ==========
            //new ChartEmployee { Id = 20, ManagerId = null, FullName = "کیانوش مرادی", Position = "مدیر فناوری", DepartmentId = 3 },
            new ChartEmployee {Id = 21, ManagerId = 2, FullName = "نیما فرهادی", Position = "لید برنامه‌نویس", DepartmentId = 3, DepartmentName = "فتی", Role = "CEO"},
            new ChartEmployee {Id = 22, ManagerId = 2, FullName = "الهام رستمی", Position = "لید تست", DepartmentId = 3, DepartmentName = "فتی", Role = "CEO"},
            new ChartEmployee {Id = 23, ManagerId = 2, FullName = "پویا کاظمی", Position = "برنامه‌نویس بک‌اند", DepartmentId = 3, DepartmentName = "فتی", Role = "CEO"},
            new ChartEmployee {Id = 24, ManagerId = 2, FullName = "مینا شریفی", Position = "برنامه‌نویس فرانت", DepartmentId = 3, DepartmentName = "فتی", Role = "CEO"}};
            //Dispatcher.Invoke(async () => { await Test.ShowDepartment(AllData); });
        }

        public ISeries[] CompetencySeries { get; set; }
        public ISeries[] PerformanceSeries { get; set; }
        public PolarAxis[] AngleAxes { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }

        private void SetupCharts()
        {
            // ===== Radar Chart (درست شده) =====
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
                    Labels = new string[]
                    {
                        "Leadership",
                        "Communication",
                        "Collaboration",
                        "Problem Solving",
                        "Results Orientation",
                        "Strategic Thinking"
                    },
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
                    TextSize = 11,
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#334155"))
                }
            };

            // ===== Line Chart =====
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

        private void HamBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SideMenu_IsOpen)
            {
                SideMenu_IsOpen = false;
                CloseMenu(sender);
            }
            else
            {
                SideMenu_IsOpen = true;
                OpenMenu(sender);
            }
        }

        private void SideMenu_Click(object sender, RoutedEventArgs e)
        {
            ActiveSideMenu.Style = this.FindResource("NavButton") as Style;
            if (sender is Button targetBtn)
            {
                var style = this.FindResource("NavButtonActive") as Style;
                targetBtn.Style = style;
                ActiveSideMenu = targetBtn;
            }
            if(EmployeesBtn == (sender as Button))
            {
                PageName.Text = "کارکنان";
                DataPanel.Navigate(EmpListPage);
                //DataPanel.Content = EmpListPage;
                
            }
            if(DepartmentsBtn == (sender as Button))
            {
                PageName.Text = "دپارتمان ها";
                DataPanel.Navigate(AllDepartmentPage);
            }
            if(DashbordBynSide ==  (sender as Button))
            {
                DataPanel.Content = null;
            }
        }


        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                    return typedChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }

            return null;
        }


        void CloseMenu(object sender)
        {
            RightMenu.Width = 60;


            if (sender is Button targetBtn)
            {
                // پیدا کردن StackPanel داخل دکمه
                var stackPanel = FindVisualChild<StackPanel>(targetBtn);

                if (stackPanel != null && stackPanel.Children.Count > 1)
                {
                    // مخفی کردن TextBlock دوم (متن)
                    stackPanel.Children[1].Visibility = Visibility.Collapsed;
                }
            }
        }

        public void NavigateTo(Page page)
        {
            //DataPanel.Content = page;
            DataPanel.Navigate(page);
        }
        void OpenMenu(object sender)
        {
            RightMenu.Width = double.NaN;


            if (sender is Button targetBtn)
            {
                {
                    // پیدا کردن StackPanel داخل دکمه
                    var stackPanel = FindVisualChild<StackPanel>(targetBtn);

                    if (stackPanel != null && stackPanel.Children.Count > 1)
                    {
                        // مخفی کردن TextBlock دوم (متن)
                        stackPanel.Children[1].Visibility = Visibility.Visible;
                    }
                }
            }
        }
    }
}