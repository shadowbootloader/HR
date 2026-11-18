using System.IO;
using System.Windows;
using System.Windows.Controls;
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
                HeadcountText.Text = snapshot.Headcount == 0
                    ? "هنوز کارمندی ثبت نشده است."
                    : $"تعداد کارکنان: {snapshot.Headcount}";

                var hires = snapshot.Hires.ToList();
                BindHires(hires.Select(item => item.Label).ToList(), hires.Select(item => (double)item.Count).ToList());
                BindDepartments(snapshot.ByDepartment);
                BindDonut(StatusChart, StatusEmpty, snapshot.ByStatus, StatusColor);
                BindDonut(GenderChart, GenderEmpty, snapshot.ByGender, GenderColor);
            }
            catch (Exception ex)
            {
                HeadcountText.Text = ex.Message;
            }
        }

        private void BindHires(IList<string> labels, IReadOnlyList<double> values)
        {
            var hasData = values.Any(value => value > 0);
            HiresChart.Visibility = hasData ? Visibility.Visible : Visibility.Collapsed;
            HiresEmpty.Visibility = hasData ? Visibility.Collapsed : Visibility.Visible;
            if (!hasData) return;

            HiresChart.Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = values,
                    Stroke = new SolidColorPaint(SKColor.Parse("#38BDF8"), 2.5f),
                    Fill = new SolidColorPaint(SKColor.Parse("#38BDF8").WithAlpha(48)),
                    GeometrySize = 7,
                    GeometryFill = new SolidColorPaint(SKColor.Parse("#38BDF8")),
                    GeometryStroke = new SolidColorPaint(SKColor.Parse("#0B1120"), 2),
                    LineSmoothness = 0.3
                }
            };
            HiresChart.LegendPosition = LegendPosition.Hidden;
            HiresChart.XAxes = new[] { CategoryAxis(labels, rotate: -35) };
            HiresChart.YAxes = new[] { ValueAxis() };
            HiresChart.TooltipTextPaint = LabelPaint("#F8FAFC");
            HiresChart.TooltipBackgroundPaint = new SolidColorPaint(SKColor.Parse("#1E293B"));
        }

        private void BindDepartments(IReadOnlyList<NamedCount> rows)
        {
            var hasData = rows.Any(item => item.Count > 0);
            DepartmentChart.Visibility = hasData ? Visibility.Visible : Visibility.Collapsed;
            DepartmentEmpty.Visibility = hasData ? Visibility.Collapsed : Visibility.Visible;
            if (!hasData) return;

            DepartmentChart.Height = Math.Max(180, rows.Count * 36);
            DepartmentChart.Series = new ISeries[]
            {
                new RowSeries<double>
                {
                    Values = rows.Select(item => (double)item.Count).ToList(),
                    Fill = new SolidColorPaint(SKColor.Parse("#3B82F6")),
                    DataLabelsPaint = LabelPaint("#F8FAFC"),
                    DataLabelsSize = 13,
                    DataLabelsPosition = DataLabelsPosition.End,
                    DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("0")
                }
            };
            DepartmentChart.LegendPosition = LegendPosition.Hidden;
            DepartmentChart.YAxes = new[]
            {
                new Axis
                {
                    Labels = rows.Select(item => item.Name).ToList(),
                    LabelsPaint = LabelPaint(),
                    TextSize = 14,
                    IsInverted = true,
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#1E293B"))
                }
            };
            DepartmentChart.XAxes = new[] { ValueAxis() };
            DepartmentChart.TooltipTextPaint = LabelPaint("#F8FAFC");
            DepartmentChart.TooltipBackgroundPaint = new SolidColorPaint(SKColor.Parse("#1E293B"));
        }

        private static void BindDonut(PieChart chart, TextBlock empty, IReadOnlyList<NamedCount> rows, Func<string, int, SKColor> colorOf)
        {
            var slices = rows.Where(item => item.Count > 0).ToList();
            var hasData = slices.Count > 0;
            chart.Visibility = hasData ? Visibility.Visible : Visibility.Collapsed;
            empty.Visibility = hasData ? Visibility.Collapsed : Visibility.Visible;
            if (!hasData) return;

            chart.Series = slices.Select((item, index) => (ISeries)new PieSeries<double>
            {
                Values = new[] { (double)item.Count },
                Name = $"{item.Name} ({item.Count})",
                InnerRadius = 52,
                Fill = new SolidColorPaint(colorOf(item.Name, index)),
                DataLabelsPaint = null
            }).ToArray();
            chart.LegendPosition = LegendPosition.Bottom;
            chart.LegendTextPaint = LabelPaint();
            chart.LegendTextSize = 14;
            chart.TooltipTextPaint = LabelPaint("#F8FAFC");
            chart.TooltipBackgroundPaint = new SolidColorPaint(SKColor.Parse("#1E293B"));
        }

        private static Axis CategoryAxis(IList<string> labels, int rotate)
            => new()
            {
                Labels = labels,
                LabelsPaint = LabelPaint(),
                TextSize = 13,
                LabelsRotation = rotate,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#1E293B"))
            };

        private static Axis ValueAxis()
            => new()
            {
                MinLimit = 0,
                LabelsPaint = LabelPaint(),
                TextSize = 12,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#1E293B"))
            };

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

        private static SKColor GenderColor(string _, int index)
        {
            SKColor[] palette =
            {
                SKColor.Parse("#38BDF8"),
                SKColor.Parse("#F472B6"),
                SKColor.Parse("#A78BFA"),
                SKColor.Parse("#34D399")
            };
            return palette[index % palette.Length];
        }

        private static SolidColorPaint LabelPaint(string hex = "#CBD5E1")
            => new(SKColor.Parse(hex)) { SKTypeface = Font };

        private static SKTypeface LoadFont()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Fonts", "BNazanin.ttf");
            return File.Exists(path) ? SKTypeface.FromFile(path) : SKTypeface.Default;
        }
    }
}
