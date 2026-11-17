using System.Windows.Controls;
using HR.Services.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;

namespace HR.Pages
{
    public partial class DashboardPage : Page
    {
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
                Bind(HiresChart, snapshot.Hires.Select(item => item.Label), snapshot.Hires.Select(item => (double)item.Count), line: true);
                Bind(DepartmentChart, snapshot.ByDepartment.Select(item => item.Name), snapshot.ByDepartment.Select(item => (double)item.Count), line: false);
                Bind(StatusChart, snapshot.ByStatus.Select(item => item.Name), snapshot.ByStatus.Select(item => (double)item.Count), line: false);
                Bind(GenderChart, snapshot.ByGender.Select(item => item.Name), snapshot.ByGender.Select(item => (double)item.Count), line: false);
            }
            catch (Exception ex)
            {
                HeadcountText.Text = ex.Message;
            }
        }

        private static void Bind(CartesianChart chart, IEnumerable<string> labels, IEnumerable<double> values, bool line)
        {
            var labelList = labels.ToList();
            var valueList = values.ToList();
            if (labelList.Count == 0)
            {
                labelList.Add("—");
                valueList.Add(0);
            }

            ISeries series = line
                ? new LineSeries<double>
                {
                    Values = valueList,
                    Stroke = new SolidColorPaint(SKColor.Parse("#38BDF8"), 2.5f),
                    Fill = new SolidColorPaint(SKColor.Parse("#38BDF8").WithAlpha(40)),
                    GeometrySize = 6
                }
                : new ColumnSeries<double>
                {
                    Values = valueList,
                    Fill = new SolidColorPaint(SKColor.Parse("#3B82F6"))
                };

            chart.Series = new[] { series };
            chart.XAxes = new[]
            {
                new Axis { Labels = labelList, LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")), TextSize = 12 }
            };
            chart.YAxes = new[]
            {
                new Axis { MinLimit = 0, LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")) }
            };
        }
    }
}
