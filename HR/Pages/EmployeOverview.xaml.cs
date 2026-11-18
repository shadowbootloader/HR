using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media;
using HR.Services.Assessments;
using HR.Services.Data;
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
                Header.BindIdentity(null, null, null, null, null, null);
                Header.BindPerformance(null, null, 0);
                Insights.ShowFacts(("پرونده", "از فهرست کارکنان یک نفر را باز کنید."));
                ShowAttempts(Array.Empty<AttemptSummary>());
                return;
            }

            Header.Bind(Employee.FullName, Employee.Position, Employee.Department, Employee.Email);
            Header.BindStatus(Employee.Status);
            Header.BindPhoto(Employee.PhotoPath);

            try
            {
                using var people = new EmployeeService();
                using var assessments = new AssessmentService();
                var entity = await people.GetByIdAsync(Employee.Id);
                var history = await assessments.HistoryAsync(Employee.Id);

                // ---------- اطلاعات هویتی از پایگاه داده ----------
                if (entity != null)
                {
                    var phone = entity.PhoneNumbers
                        .Where(item => item.EndDate == null)
                        .Select(item => item.Number)
                        .FirstOrDefault()
                        ?? entity.PhoneNumbers.Select(item => item.Number).FirstOrDefault();

                    string? managerName = null;
                    var direct = entity.Supplement?.DirectManager;
                    if (direct != null && direct.EmployeeId != entity.EmployeeId)
                        managerName = $"{direct.FirstName} {direct.LastName}".Trim();
                    var managerId = managerName == null ? entity.Supplement?.Department?.ManagerEmployeeId : null;
                    if (managerId != null && managerId != entity.EmployeeId)
                    {
                        var manager = await people.GetByIdAsync(managerId.Value, includeDetails: false);
                        if (manager != null) managerName = $"{manager.FirstName} {manager.LastName}".Trim();
                    }

                    Header.BindPhoto(EmployeePhotos.PortraitPath(entity.PersonFiles) ?? Employee.PhotoPath);

                    Header.BindIdentity(
                        entity.Supplement?.PersonnelCode,
                        ToPersianDate(entity.BirthDate),
                        entity.MaritalStatus?.Title,
                        phone,
                        ToPersianDate(entity.Supplement?.StartDate),
                        managerName);
                }

                // ---------- عملکرد کلی: میانگین نمرهٔ آزمون‌های ثبت‌شده ----------
                var scored = history.Where(item => item.TotalScore != null).ToList();
                Header.BindPerformance(
                    scored.Count == 0 ? null : scored.Average(item => item.TotalScore!.Value),
                    scored.OrderByDescending(item => item.SubmittedAt).Select(item => item.LevelLabel).FirstOrDefault(),
                    scored.Count);

                var scores = entity?.SelfAssessment == null ? null : new SelfAssessmentScores
                {
                    Discipline = entity.SelfAssessment.Discipline,
                    Speed = entity.SelfAssessment.Speed,
                    Effort = entity.SelfAssessment.Effort,
                    Negotiation = entity.SelfAssessment.Negotiation,
                    Patience = entity.SelfAssessment.Patience,
                    Creativity = entity.SelfAssessment.Creativity,
                    Punctuality = entity.SelfAssessment.Punctuality,
                    Teamwork = entity.SelfAssessment.Teamwork,
                    Accuracy = entity.SelfAssessment.Accuracy,
                    Planning = entity.SelfAssessment.Planning
                };
                var snapshot = ProfileInsights.Build(
                    scores,
                    history,
                    entity?.Softwares.Select(item => new SkillBar { Title = item.Software?.Title ?? "", LevelTitle = item.SkillLevel?.Title }) ?? Array.Empty<SkillBar>(),
                    entity?.Courses.Select(item => new CourseCertificate { Title = item.Title, Institution = item.InstitutionName, HasCertificate = item.CertificateFileId != null }) ?? Array.Empty<CourseCertificate>(),
                    DateTime.Today);
                ShowProfile(snapshot);
                ShowAttempts(history);
                Insights.ShowFacts(OverviewFacts.Build(entity, history, DateTime.Today));
            }
            catch (Exception ex)
            {
                Evaluations.ClearItems();
                Evaluations.AddItem("آزمون‌ها", ex.Message, PackIconKind.Alert, Brushes.Transparent, "");
            }
        }

        private static string? ToPersianDate(DateTime? date)
        {
            if (date == null || date.Value == default || date.Value.Year < 1700) return null;
            var calendar = new PersianCalendar();
            var value = date.Value;
            return $"{calendar.GetYear(value):0000}/{calendar.GetMonth(value):00}/{calendar.GetDayOfMonth(value):00}";
        }

        private void ShowProfile(ProfileSnapshot snapshot)
        {
            Competencies.SetData(
                snapshot.Competencies.Select(item => item.Name).ToArray(),
                snapshot.Competencies.Select(item => (double)item.Count).ToArray());
            Performance.SetData(
                snapshot.Performance.Select(item => item.Label).ToArray(),
                snapshot.Performance.Select(item => (double)item.Count).ToArray());

            Strengths.ClearAll();
            if (snapshot.Strengths.Count == 0 && snapshot.Improvements.Count == 0)
                Strengths.AddStrength(snapshot.HasSelfAssessment ? "موردی در آستانهٔ قوت یا بهبود نیست." : "خودارزیابی ثبت نشده است.");
            foreach (var item in snapshot.Strengths) Strengths.AddStrength(item);
            foreach (var item in snapshot.Improvements) Strengths.AddArea(item);

            Achievements.ClearItems();
            if (snapshot.Achievements.Count == 0)
                Achievements.AddItem("—", "دستاوردی ثبت نشده", "نتیجهٔ آزمونی برای این کارمند نیست.", "•", Colors.Transparent);
            foreach (var item in snapshot.Achievements)
                Achievements.AddItem(item.SubmittedAt?.ToString("yyyy/MM/dd") ?? "—", item.Title, $"{item.TotalScore:0.#} · {item.LevelLabel}", "•", Colors.Transparent);

            SkillsCard.ShowSkills(snapshot.Skills.Select(item => (item.Name, item.Count)).ToList());
            Certificates.ShowCertificates(snapshot.Certificates.Select(item => (item.Title, item.Institution ?? "—")).ToList());
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