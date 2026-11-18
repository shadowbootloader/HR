using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using HR.Services;
using HR.Services.Data;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;

namespace HR.Pages
{
    public partial class DashboardPage : Page
    {
        private static readonly SKTypeface Font = LoadFont();
        private readonly DashboardService _dashboard = new();

        public DashboardPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await LoadAsync();
            Unloaded += (_, _) => _dashboard.Dispose();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                var snapshot = await _dashboard.GetAsync();
                KpiActive.Text = snapshot.ActiveRoster.ToString();
                KpiActiveSub.Text = "بدون حذف‌شده و پایان‌همکاری";
                KpiNew.Text = snapshot.NewHireCount.ToString();
                var delta = snapshot.NewHireCount - snapshot.PreviousNewHireCount;
                KpiNewSub.Text = delta == 0 ? "مثل ۳۰ روز قبل" : (delta > 0 ? "↑ " : "↓ ") + Math.Abs(delta) + " نسبت به ۳۰ روز قبل";
                KpiLeave.Text = snapshot.OnLeaveCount.ToString();
                KpiLeaveSub.Text = "مرخصی باز امروز";
                KpiDepts.Text = snapshot.DepartmentCount.ToString();
                KpiCapacity.Text = snapshot.CapacityPercent.ToString("0.#") + "٪";
                KpiCapacitySub.Text = snapshot.DepartmentLoads.Sum(item => item.Capacity) == 0
                    ? "ظرفیت مصوب ثبت نشده"
                    : snapshot.DepartmentLoads.Sum(item => item.Actual) + " از " + snapshot.DepartmentLoads.Sum(item => item.Capacity);
                KpiTests.Text = snapshot.AssessmentsThisMonth.ToString();

                BindDepartments(snapshot.DepartmentLoads);
                BindDonut(StatusChart, StatusEmpty, snapshot.ByStatus.Where(item => item.Count > 0).ToList(), StatusColor);
                BindTrend(snapshot.Hires, snapshot.LeaveStarts);
                BindDonut(GenderChart, GenderEmpty, snapshot.ByGender, (_, index) => GenderColor(index));
                BindAges(snapshot.ByAge);
                BindList(NewList, NewEmpty, snapshot.NewHires, true);
                BindList(LeaveList, LeaveEmpty, snapshot.OpenLeaves, true);
                BindList(AttemptList, AttemptEmpty, snapshot.RecentAttempts, false);
            }
            catch (Exception ex)
            {
                KpiActiveSub.Text = ex.Message;
            }
        }

        private void BindDepartments(IReadOnlyList<DepartmentLoad> rows)
        {
            var has = rows.Count > 0;
            DepartmentChart.Visibility = has ? Visibility.Visible : Visibility.Collapsed;
            DepartmentEmpty.Visibility = has ? Visibility.Collapsed : Visibility.Visible;
            if (!has) return;
            DepartmentChart.Height = Math.Max(220, rows.Count * 42);
            DepartmentChart.Series = new ISeries[]
            {
                new RowSeries<double>
                {
                    Name = "حاضر",
                    Values = rows.Select(item => (double)item.Actual).ToList(),
                    Fill = new SolidColorPaint(SKColor.Parse("#3B82F6")),
                    DataLabelsPaint = Label("#F8FAFC"),
                    DataLabelsSize = 12,
                    DataLabelsPosition = DataLabelsPosition.End
                },
                new RowSeries<double>
                {
                    Name = "ظرفیت",
                    Values = rows.Select(item => (double)item.Capacity).ToList(),
                    Fill = new SolidColorPaint(SKColor.Parse("#334155"))
                }
            };
            DepartmentChart.LegendPosition = LegendPosition.Bottom;
            DepartmentChart.LegendTextPaint = Label();
            DepartmentChart.YAxes = new[] { AxisLabels(rows.Select(item => item.Name).ToList(), invert: true) };
            DepartmentChart.XAxes = new[] { ValueAxis() };
            PaintTooltip(DepartmentChart);
        }

        private void BindTrend(IReadOnlyList<MonthCount> hires, IReadOnlyList<MonthCount> leaves)
        {
            var has = hires.Any(item => item.Count > 0) || leaves.Any(item => item.Count > 0);
            TrendChart.Visibility = has ? Visibility.Visible : Visibility.Collapsed;
            TrendEmpty.Visibility = has ? Visibility.Collapsed : Visibility.Visible;
            if (!has) return;
            TrendChart.Series = new ISeries[]
            {
                Line("استخدام", hires.Select(item => (double)item.Count).ToList(), "#38BDF8"),
                Line("شروع مرخصی", leaves.Select(item => (double)item.Count).ToList(), "#F59E0B")
            };
            TrendChart.LegendPosition = LegendPosition.Bottom;
            TrendChart.LegendTextPaint = Label();
            TrendChart.XAxes = new[] { AxisLabels(hires.Select(item => item.Label).ToList(), rotate: -35) };
            TrendChart.YAxes = new[] { ValueAxis() };
            PaintTooltip(TrendChart);
        }

        private static void BindDonut(PieChart chart, TextBlock empty, IReadOnlyList<NamedCount> rows, Func<string, int, SKColor> color)
        {
            var slices = rows.Where(item => item.Count > 0).ToList();
            chart.Visibility = slices.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            empty.Visibility = slices.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
            if (slices.Count == 0) return;
            chart.Series = slices.Select((item, index) => (ISeries)new PieSeries<double>
            {
                Name = item.Name + " (" + item.Count + ")",
                Values = new[] { (double)item.Count },
                InnerRadius = 52,
                Fill = new SolidColorPaint(color(item.Name, index))
            }).ToArray();
            chart.LegendPosition = LegendPosition.Bottom;
            chart.LegendTextPaint = Label();
            chart.LegendTextSize = 13;
            chart.TooltipTextPaint = Label("#F8FAFC");
            chart.TooltipBackgroundPaint = new SolidColorPaint(SKColor.Parse("#1E293B"));
        }

        private void BindAges(IReadOnlyList<NamedCount> rows)
        {
            var has = rows.Any(item => item.Count > 0);
            AgeChart.Visibility = has ? Visibility.Visible : Visibility.Collapsed;
            AgeEmpty.Visibility = has ? Visibility.Collapsed : Visibility.Visible;
            if (!has) return;
            AgeChart.Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = rows.Select(item => (double)item.Count).ToList(),
                    Fill = new SolidColorPaint(SKColor.Parse("#A78BFA")),
                    Name = "نفر"
                }
            };
            AgeChart.LegendPosition = LegendPosition.Hidden;
            AgeChart.XAxes = new[] { AxisLabels(rows.Select(item => item.Name).ToList()) };
            AgeChart.YAxes = new[] { ValueAxis() };
            PaintTooltip(AgeChart);
        }

        private static void BindList(StackPanel panel, TextBlock empty, IReadOnlyList<DashboardListItem> items, bool avatar)
        {
            panel.Children.Clear();
            empty.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            foreach (var item in items)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4) };
                if (avatar)
                {
                    var photo = ImagePicker.Load(item.PhotoPath);
                    row.Children.Add(new Ellipse
                    {
                        Width = 32,
                        Height = 32,
                        Margin = new Thickness(0, 0, 8, 0),
                        Fill = photo == null
                            ? new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B))
                            : new ImageBrush(photo) { Stretch = Stretch.UniformToFill }
                    });
                }
                var text = new StackPanel();
                text.Children.Add(new TextBlock { Text = item.Title, Foreground = Brushes.White });
                text.Children.Add(new TextBlock { Text = item.Subtitle, Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)), FontSize = 12 });
                row.Children.Add(text);
                panel.Children.Add(row);
            }
        }

        private static LineSeries<double> Line(string name, IReadOnlyCollection<double> values, string color)
            => new()
            {
                Name = name,
                Values = values,
                Stroke = new SolidColorPaint(SKColor.Parse(color), 2.5f),
                Fill = new SolidColorPaint(SKColor.Parse(color).WithAlpha(40)),
                GeometrySize = 6,
                GeometryFill = new SolidColorPaint(SKColor.Parse(color)),
                LineSmoothness = 0.25
            };

        private static Axis AxisLabels(IList<string> labels, int rotate = 0, bool invert = false)
            => new()
            {
                Labels = labels,
                LabelsPaint = Label(),
                TextSize = 12,
                LabelsRotation = rotate,
                IsInverted = invert,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#1E293B"))
            };

        private static Axis ValueAxis()
            => new()
            {
                MinLimit = 0,
                LabelsPaint = Label(),
                TextSize = 12,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#1E293B"))
            };

        private static void PaintTooltip(CartesianChart chart)
        {
            chart.TooltipTextPaint = Label("#F8FAFC");
            chart.TooltipBackgroundPaint = new SolidColorPaint(SKColor.Parse("#1E293B"));
        }

        private static SKColor StatusColor(string name, int _)
            => name switch
            {
                RosterStatus.NewHire => SKColor.Parse("#3B82F6"),
                RosterStatus.Active => SKColor.Parse("#22C55E"),
                RosterStatus.OnLeave => SKColor.Parse("#F59E0B"),
                RosterStatus.Inactive => SKColor.Parse("#94A3B8"),
                RosterStatus.Deleted => SKColor.Parse("#F87171"),
                _ => SKColor.Parse("#38BDF8")
            };

        private static SKColor GenderColor(int index)
        {
            SKColor[] palette = { SKColor.Parse("#38BDF8"), SKColor.Parse("#F472B6"), SKColor.Parse("#A78BFA"), SKColor.Parse("#34D399") };
            return palette[index % palette.Length];
        }

        private static SolidColorPaint Label(string hex = "#CBD5E1")
            => new(SKColor.Parse(hex)) { SKTypeface = Font };

        private static SKTypeface LoadFont()
        {
            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "Fonts", "BNazanin.ttf");
            return File.Exists(path) ? SKTypeface.FromFile(path) : SKTypeface.Default;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var columns = e.NewSize.Width < 900 ? 2 : e.NewSize.Width < 1280 ? 3 : 6;
            KpiGrid.Columns = columns;
            ListGrid.Columns = e.NewSize.Width < 980 ? 1 : 3;
        }
    }
}
