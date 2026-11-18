using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HR.Controls.Assessment;
using HR.Models;
using HR.Services;
using HR.Services.Assessments;
using HR.Services.Auth;
using HR.Services.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MaterialDesignThemes.Wpf;
using SkiaSharp;

namespace HR.Pages
{
    public partial class EmployeeAssessmentCenterPage : Page
    {
        public ObservableCollection<AssessmentActivityItem> Activities { get; } = new();
        private readonly AssessmentService _assessments = new();
        private readonly IEmployeeService _employees = new EmployeeService();
        private readonly List<AssessmentTestRow> _allRows = new();
        private readonly List<EmployeePick> _people = new();
        private List<AssessmentRecord> _definitions = new();
        private List<AttemptSummary> _history = new();
        private int _pageIndex;
        private const int PageSize = 6;
        private bool _loadingPeople;

        public EmployeeAssessmentCenterPage()
        {
            InitializeComponent();
            Hero.Medals = new ObservableCollection<MedalItem>();
            SidePanel.Medals = new ObservableCollection<SideMedalItem>();
            SidePanel.Activities = new ObservableCollection<SideActivityItem>();
            SidePanel.XpMax = 100;
            if (AuthSession.Current?.CanManagePeople != true)
                BtnNewTest.Visibility = Visibility.Collapsed;
            Loaded += async (_, _) => await LoadPeopleAsync();
            Unloaded += (_, _) =>
            {
                _assessments.Dispose();
                (_employees as IDisposable)?.Dispose();
            };
        }

        private async System.Threading.Tasks.Task LoadPeopleAsync()
        {
            _loadingPeople = true;
            _people.Clear();
            EmployeePicker.ItemsSource = null;
            foreach (var employee in await _employees.GetAllAsync())
            {
                var name = $"{employee.FirstName} {employee.LastName}".Trim();
                var code = employee.Supplement?.PersonnelCode ?? "";
                var department = employee.Supplement?.Department?.Title ?? "";
                _people.Add(new EmployeePick(employee.EmployeeId, string.Join(" · ", new[] { name, code, department }.Where(part => !string.IsNullOrWhiteSpace(part))), $"{name} {code} {department}"));
            }

            EmployeePicker.ItemsSource = _people;
            EmployeePicker.SelectedItem = null;
            _loadingPeople = false;
            ShowNoSelection();
        }

        private void PersonSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var term = PersonSearch.Text?.Trim() ?? string.Empty;
            _loadingPeople = true;
            EmployeePicker.ItemsSource = string.IsNullOrWhiteSpace(term)
                ? _people.ToList()
                : _people.Where(person => person.Search.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
            _loadingPeople = false;
        }

        private void ShowNoSelection()
        {
            _definitions = new();
            _history = new();
            _allRows.Clear();
            RefreshList();
            Hero.EmployeeName = "کارمندی انتخاب نشده";
            Hero.Role = "";
            if (SelectPrompt != null) SelectPrompt.Visibility = Visibility.Visible;
            ScoreChart.Series = Array.Empty<ISeries>();
            BindSide(Array.Empty<AttemptSummary>());
            UpdateFooterStats();
        }

        private async void EmployeePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loadingPeople) return;
            await ReloadAssessmentsAsync();
        }

        private async System.Threading.Tasks.Task ReloadAssessmentsAsync()
        {
            try
            {
                _definitions = (await _assessments.ListAsync()).ToList();
                var selected = EmployeePicker.SelectedItem as EmployeePick;
                _history = selected == null
                    ? new List<AttemptSummary>()
                    : (await _assessments.HistoryAsync(selected.Id)).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "آزمون‌ها", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var person = EmployeePicker.SelectedItem as EmployeePick;
            if (person == null)
            {
                ShowNoSelection();
                return;
            }
            if (SelectPrompt != null) SelectPrompt.Visibility = Visibility.Collapsed;
            Hero.EmployeeName = person.Name;
            Hero.Role = AuthSession.Current?.RoleLabel ?? "";
            BindRows();
            BindChart(_history);
            BindSide(_history);
            UpdateFooterStats();
        }

        private void BindRows()
        {
            _allRows.Clear();
            foreach (var definition in _definitions)
            {
                var latest = _history.FirstOrDefault(item => item.AssessmentCode == definition.Code);
                var state = AttemptStatus.Of(latest);
                var done = state == "completed";
                _allRows.Add(CreateRow(
                    definition.Title,
                    definition.Description ?? definition.Category,
                    definition.Category,
                    PackIconKind.ClipboardText,
                    "#38BDF8",
                    state,
                    done ? (int)Math.Round(latest!.TotalScore ?? 0) : 0,
                    done ? $"{latest!.SubmittedAt:yyyy/MM/dd} · {latest.LevelLabel}" : string.Empty,
                    definition.Code));
            }

            _pageIndex = 0;
            RefreshList();
        }

        private void BindChart(IReadOnlyList<AttemptSummary> history)
        {
            ScoreChart.Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = history.Take(8).Select(item => item.TotalScore ?? 0).Reverse().ToArray(),
                    Fill = new SolidColorPaint(SKColor.Parse("#38BDF8")),
                    Name = "نمره"
                }
            };
            ScoreChart.XAxes = new[]
            {
                new Axis
                {
                    Labels = history.Take(8).Select(item => item.Title).Reverse().ToArray(),
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8"))
                }
            };
            ScoreChart.YAxes = new[]
            {
                new Axis { MinLimit = 0, MaxLimit = 100, LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")) }
            };
        }

        private void BindSide(IReadOnlyList<AttemptSummary> history)
        {
            var average = history.Count == 0 ? 0 : (int)Math.Round(history.Average(item => item.TotalScore ?? 0));
            SidePanel.Xp = average;
            SidePanel.NextGoalTitle = history.FirstOrDefault()?.Title ?? "آزمونی ثبت نشده";
            SidePanel.NextGoalDesc = history.FirstOrDefault()?.LevelLabel ?? "نتیجه‌ای برای این کارمند نیست";
            SidePanel.NextGoalPoints = (int)Math.Round(history.FirstOrDefault()?.TotalScore ?? 0);
            SidePanel.Activities = new ObservableCollection<SideActivityItem>(history.Take(6).Select(item => new SideActivityItem
            {
                Time = item.SubmittedAt?.ToString("MM/dd HH:mm") ?? "",
                Title = item.Title,
                Detail = item.TotalScore is double score ? score.ToString("0.#") : "",
                IconKind = PackIconKind.ClipboardCheck,
                IconBrush = (Brush)new BrushConverter().ConvertFrom("#38BDF8")!
            }));
        }

        private AssessmentTestRow CreateRow(
            string title, string subtitle, string categoryText,
            PackIconKind categoryIcon, string categoryColor,
            string status, int progress, string lastDate,
            string definitionId,
            int outlierCount = 0)
        {
            AssessmentTestRow NewRow = new AssessmentTestRow
            {
                Title = title,
                Subtitle = subtitle,
                CategoryText = categoryText,
                CategoryIcon = categoryIcon,
                CategoryBrush = new BrushConverter().ConvertFrom(categoryColor) as Brush ?? Brushes.Transparent,
                Status = status,
                Progress = progress,
                LastDate = lastDate,
                OutlierCount = outlierCount,
                DefinitionId = definitionId
            };
            NewRow.RowClick += NewRow_RowClick;
            return NewRow;
        }

        private void UpdateFooterStats()
        {
            if (FooterStats == null) return;
            var scored = _history.Where(item => item.TotalScore.HasValue).Select(item => item.TotalScore!.Value).ToList();
            FooterStats.OverallPercent = scored.Count == 0 ? 0 : (int)Math.Round(scored.Average());
            FooterStats.TotalTests = _allRows.Count;
            FooterStats.PsychologyCount = _allRows.Count(r => r.CategoryText == "روانشناسی");
            FooterStats.JobCount = _allRows.Count(r => r.CategoryText == "شغلی");
            FooterStats.SkillCount = _allRows.Count(r => r.CategoryText == "مهارتی");
            FooterStats.OutlierCount = 0;
            FooterStats.LockedCount = 0;
        }

        private async void NewRow_RowClick(object sender, RoutedEventArgs e)
        {
            if (sender is not AssessmentTestRow target) return;
            if (EmployeePicker.SelectedItem is not EmployeePick person)
            {
                MessageBox.Show("ابتدا کارمند را انتخاب کنید.");
                return;
            }

            var record = _definitions.FirstOrDefault(item => item.Code == target.DefinitionId)
                ?? await _assessments.GetByCodeAsync(target.DefinitionId);
            if (record == null)
            {
                MessageBox.Show("تعریف این آزمون پیدا نشد.");
                return;
            }

            var definition = AssessmentDefinitionMapper.ToDefinition(record);
            var session = new AssessmentSession
            {
                DefinitionId = definition.Id,
                EmployeeId = person.Id.ToString(),
                Status = AssessmentStatus.NotStarted
            };
            var shell = new AssessmentTestShellPage(definition, session);
            shell.Submitted += async (_, submitted) =>
            {
                var result = submitted.Result ?? AssessmentScoringEngine.Compute(definition, submitted);
                await _assessments.SaveAttemptAsync(new AttemptSave
                {
                    AssessmentCode = definition.Id,
                    EmployeeId = person.Id,
                    StartedAt = submitted.StartedAt ?? DateTime.UtcNow,
                    SubmittedAt = submitted.CompletedAt ?? DateTime.UtcNow,
                    TotalScore = result.TotalScore,
                    LevelLabel = result.LevelLabel,
                    Summary = result.Summary,
                    Answers = submitted.Answers.Select(pair => new AnswerSave
                    {
                        QuestionCode = pair.Key,
                        OptionCode = pair.Value.SelectedOptionIds.FirstOrDefault(),
                        NumericValue = pair.Value.Numeric,
                        TextValue = pair.Value.Text,
                        Score = result.QuestionScores.TryGetValue(pair.Key, out var score) ? score : 0
                    }).ToList()
                });
            };
            NavigationService?.Navigate(shell);
        }

        /// <summary>فیلتر + مرتب‌سازی + صفحه</summary>
        private void RefreshList()
        {
            IEnumerable<AssessmentTestRow> q = _allRows;

            // تب دسته
            if (CategoryTabs?.SelectedItem is TabItem tab)
            {
                var cat = ExtractTabTitle(tab);
                if (!string.IsNullOrEmpty(cat) && cat != "همه")
                    q = q.Where(r => r.CategoryText == cat);
            }

            // جستجو
            var term = TestSearchBox?.Text?.Trim();
            if (!string.IsNullOrEmpty(term))
            {
                q = q.Where(r =>
                    (r.Title?.Contains(term) ?? false) ||
                    (r.Subtitle?.Contains(term) ?? false) ||
                    (r.CategoryText?.Contains(term) ?? false));
            }

            // وضعیت
            if (StatusFilter?.SelectedItem is ComboBoxItem st && st.Content is string sc)
            {
                if (sc.Contains("تکمیل")) q = q.Where(r => r.Status == "completed");
                else if (sc.Contains("حال انجام")) q = q.Where(r => r.Status == "in-progress");
                else if (sc.Contains("پرت")) q = q.Where(r => r.Status == "outlier");
                else if (sc.Contains("قفل")) q = q.Where(r => r.Status == "locked");
                // "همه" → بدون فیلتر
            }

            // مرتب‌سازی
            if (SortFilter?.SelectedItem is ComboBoxItem so && so.Content is string ss)
            {
                if (ss.Contains("امتیاز") || ss.Contains("پیشرفت"))
                    q = q.OrderByDescending(r => r.Progress);
                // "جدیدترین" → ترتیب فعلی لیست
            }

            var filtered = q.ToList();
            var pageCount = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)PageSize));
            if (_pageIndex >= pageCount) _pageIndex = pageCount - 1;
            if (_pageIndex < 0) _pageIndex = 0;

            var page = filtered.Skip(_pageIndex * PageSize).Take(PageSize).ToList();

            TestList.Children.Clear();
            foreach (var row in page)
                TestList.Children.Add(row);

            TxtPage.Text = $"{ToFa(_pageIndex + 1)} / {ToFa(pageCount)}";
            TxtCount.Text = $"{ToFa(filtered.Count)} آزمون";
            BtnPrev.IsEnabled = _pageIndex > 0;
            BtnNext.IsEnabled = _pageIndex < pageCount - 1;
        }

        private static string ExtractTabTitle(TabItem tab)
        {
            // اگر Header متن ساده باشد
            if (tab.Header is string s) return s;

            // اگر Header = StackPanel با TextBlock + PackIcon باشد
            if (tab.Header is FrameworkElement fe)
            {
                var tb = FindVisualChild<TextBlock>(fe);
                if (tb != null) return tb.Text;
            }
            return "همه";
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var nested = FindVisualChild<T>(child);
                if (nested != null) return nested;
            }
            return null;
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_pageIndex <= 0) return;
            _pageIndex--;
            RefreshList();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            _pageIndex++;
            RefreshList();
        }

        private void CategoryTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            _pageIndex = 0;
            RefreshList();
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            _pageIndex = 0;
            RefreshList();
        }

        private void SortFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            _pageIndex = 0;
            RefreshList();
        }

        private void TestSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;
            _pageIndex = 0;
            RefreshList();
        }

        private void BtnNewTest_Click(object sender, RoutedEventArgs e)
        {
            if (AuthSession.Current?.CanManagePeople != true)
            {
                MessageBox.Show("ساخت آزمون فقط برای مدیر و منابع انسانی است.");
                return;
            }

            NavigationService?.Navigate(new AssessmentEditorPage());
        }

        private void ProfileHeroControl_NextMedalClick(object sender, RoutedEventArgs e) { }

        private static string ToFa(int n)
        {
            const string fa = "۰۱۲۳۴۵۶۷۸۹";
            return string.Concat(n.ToString().Select(c => fa[c - '0']));
        }
    }

    public class AssessmentActivityItem
    {
        public string Time { get; set; } = "";
        public string Title { get; set; } = "";
    }

    public sealed record EmployeePick(Guid Id, string Name, string Search);
}