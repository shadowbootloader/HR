using System.Windows.Controls;
using System.Windows.Media;
using HR.Services.Assessments;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MaterialDesignThemes.Wpf;
using SkiaSharp;

namespace HR.Pages
{
    public partial class EmployeOverview : Page
    {
        /// <summary>کارمندی که این صفحه نمایش می‌دهد (ممکن است null باشد).</summary>
        public EmployeeModel? Employee { get; }

        public EmployeOverview() : this(null) { }

        public EmployeOverview(EmployeeModel? employee)
        {
            InitializeComponent();
            Employee = employee;
            Loaded += async (_, _) => await BindEmployeeAsync();
        }

        private async System.Threading.Tasks.Task BindEmployeeAsync()
        {
            if (Employee == null)
            {
                Header.Bind("کارمندی انتخاب نشده", null, null, null);
                Insights.ShowFacts(("پرونده", "از فهرست کارکنان یک نفر را باز کنید."));
                ShowAttempts(Array.Empty<AttemptSummary>());
                return;
            }

            Header.Bind(Employee.FullName, Employee.Position, Employee.Department, Employee.Email);
            Insights.ShowFacts(
                ("کد ملی", Employee.NationalCode),
                ("کد پرسنلی", Employee.PersonnelCode),
                ("وضعیت", Employee.Status),
                ("ایمیل", Employee.Email),
                ("دپارتمان", Employee.Department));

            try
            {
                using var assessments = new AssessmentService();
                ShowAttempts(await assessments.HistoryAsync(Employee.Id));
            }
            catch (Exception ex)
            {
                Evaluations.ClearItems();
                Evaluations.AddItem("آزمون‌ها", ex.Message, PackIconKind.Alert, Brushes.Transparent, "");
            }
        }

        private void ShowAttempts(IReadOnlyList<AttemptSummary> history)
        {
            Evaluations.ClearItems();
            if (history.Count == 0)
            {
                Evaluations.AddItem("آزمونی ثبت نشده", "برای این کارمند نتیجه‌ای نیست.", PackIconKind.ClipboardText, Brushes.Transparent, "");
            }
            else
            {
                foreach (var attempt in history)
                {
                    var score = attempt.TotalScore?.ToString("0.#") ?? "—";
                    Evaluations.AddItem(
                        attempt.Title,
                        attempt.SubmittedAt?.ToString("yyyy/MM/dd") ?? "",
                        PackIconKind.ClipboardCheck,
                        Brushes.Transparent,
                        score + " · " + (attempt.LevelLabel ?? ""));
                }
            }

            ScoreChart.Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = history.Select(item => item.TotalScore ?? 0).Reverse().ToArray(),
                    Fill = new SolidColorPaint(SKColor.Parse("#38BDF8")),
                    Name = "نمره"
                }
            };
            ScoreChart.XAxes = new[]
            {
                new Axis
                {
                    Labels = history.Select(item => item.Title).Reverse().ToArray(),
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8"))
                }
            };
            ScoreChart.YAxes = new[]
            {
                new Axis { MinLimit = 0, MaxLimit = 100, LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")) }
            };
        }
    }
}