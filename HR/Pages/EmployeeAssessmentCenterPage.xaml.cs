using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HR.Controls.Assessment;
using HR.Models;
using MaterialDesignThemes.Wpf;

namespace HR.Pages
{
    public partial class EmployeeAssessmentCenterPage : Page
    {
        public ObservableCollection<AssessmentActivityItem> Activities { get; } = new();
        private readonly IAssessmentCatalog _catalog = new InMemoryAssessmentCatalog();
        private readonly List<AssessmentTestRow> _allRows = new();
        private int _pageIndex;
        private const int PageSize = 6;

        public EmployeeAssessmentCenterPage()
        {
            InitializeComponent();
            Hero.Medals = new ObservableCollection<MedalItem>
            {
                new MedalItem
                {
                    Title = "تحلیل‌گر داده",
                    Points = 150,
                    Description = "تکمیل مسیر تخصصی داده",
                    ToneBrush = (Brush)new BrushConverter().ConvertFrom("#F5C542"),
                    IconKind = PackIconKind.ChartBar
                },
                new MedalItem
                {
                    Title = "تیم‌ساز",
                    Points = 200,
                    Description = "همکاری و رهبری تیمی",
                    ToneBrush = (Brush)new BrushConverter().ConvertFrom("#93C5FD"),
                    IconKind = PackIconKind.AccountGroup
                },
                new MedalItem
                {
                    Title = "رشد فردی",
                    Points = 150,
                    Description = "پیشرفت مداوم مهارت‌ها",
                    ToneBrush = (Brush)new BrushConverter().ConvertFrom("#D97706"),
                    IconKind = PackIconKind.TrendingUp
                },
                new MedalItem
                {
                    Title = "نوآور",
                    Points = 200,
                    Description = "ایده‌های اجرایی نو",
                    ToneBrush = (Brush)new BrushConverter().ConvertFrom("#A78BFA"),
                    IconKind = PackIconKind.LightbulbOn
                },
            };

            LoadSample();
            UpdateFooterStats();

            SidePanel.Xp = 750;
            SidePanel.XpMax = 1000;
            SidePanel.NextMedalRemain = 250;
            SidePanel.NextGoalTitle = "تحلیل‌گر داده";
            SidePanel.NextGoalDesc = "تکمیل ۴ آزمون تخصصی با نمره بالای ۸۰";
            SidePanel.NextGoalPoints = 250;

            SidePanel.Medals = new ObservableCollection<SideMedalItem>
            {
                new() { Title = "تحلیل‌گر داده", Points = 150, IconKind = PackIconKind.Medal, ToneBrush = Brush("#F5C542") },
                new() { Title = "تیم‌ساز", Points = 200, IconKind = PackIconKind.Star, ToneBrush = Brush("#38BDF8") },
                new() { Title = "رشد فردی", Points = 150, IconKind = PackIconKind.Star, ToneBrush = Brush("#D97706") },
                new() { Title = "نوآور", Points = 200, IconKind = PackIconKind.LightbulbOn, ToneBrush = Brush("#A78BFA") },
            };
            
                        SidePanel.Activities = new ObservableCollection<SideActivityItem>
            {
                new() { Time = "۱۴:۳۵", Title = "تست شخصیت‌شناسی MBTI تکمیل شد", Detail = "امتیاز +۱۵۰", IconKind = PackIconKind.CheckCircle, IconBrush = Brush("#A78BFA") },
                new() { Time = "۱۰:۲۱", Title = "داده پرت در تست تصمیم‌گیری ثبت شد", Detail = "نیاز به بررسی", IconKind = PackIconKind.AlertCircle, IconBrush = Brush("#FB7185") },
                new() { Time = "۰۹:۴۷", Title = "مدال تحلیل‌گر داده اعطا شد", Detail = "+۲۵۰ امتیاز", IconKind = PackIconKind.Medal, IconBrush = Brush("#F5C542") },
                new() { Time = "دیروز", Title = "تست مهارت‌های ارتباطی شروع شد", IconKind = PackIconKind.Cog, IconBrush = Brush("#22D3EE") },
                new() { Time = "دیروز", Title = "ارزیابی مدیر تکمیل شد", Detail = "+۸۰ امتیاز", IconKind = PackIconKind.Briefcase, IconBrush = Brush("#38BDF8") },
            };

            static Brush Brush(string hex) => (Brush)new BrushConverter().ConvertFrom(hex);
        }

        private void LoadSample()
        {
            _allRows.Clear();
            _allRows.Add(CreateRow("شخصیت‌شناسی MBTI", "روانشناسی", "روانشناسی", PackIconKind.Brain, "#A78BFA", "completed", 100, "۱۴۰۴/۰۲/۱۲", "mbti"));
            _allRows.Add(CreateRow("ارزیابی مدیر", "شغلی و موقعیتی", "شغلی", PackIconKind.Briefcase, "#38BDF8", "in-progress", 60, "۱۴۰۴/۰۲/۱۰", "manager-eval"));
            _allRows.Add(CreateRow("تست مهارت‌های ارتباطی", "مهارتی", "مهارتی", PackIconKind.Cog, "#22D3EE", "completed", 100, "۱۴۰۴/۰۲/۰۸", "comm-skills"));
            _allRows.Add(CreateRow("تست تصمیم‌گیری", "موقعیتی", "شغلی", PackIconKind.Briefcase, "#38BDF8", "outlier", 70, "۱۴۰۴/۰۲/۰۵", "decision", 2));
            _allRows.Add(CreateRow("تست خودکارآمدی", "روانشناسی", "روانشناسی", PackIconKind.Brain, "#A78BFA", "locked", 0, "پس از تکمیل مدیر فعال می‌شود", "self-efficacy"));
            _allRows.Add(CreateRow("ارزیابی انضباط سازمانی", "انضباطی", "انضباطی", PackIconKind.ShieldCheck, "#2DD4BF", "completed", 100, "۱۴۰۴/۰۲/۰۴", "discipline"));
            _allRows.Add(CreateRow("تست حل مسئله", "موقعیتی", "موقعیتی", PackIconKind.Star, "#60A5FA", "locked", 0, "پس از تکمیل ارزیابی مدیر فعال می‌شود", "problem-solving"));
            _allRows.Add(CreateRow("تست خلاقیت", "توانمندی‌ها", "توانمندی‌ها", PackIconKind.Star, "#22D3EE", "completed", 90, "۱۴۰۴/۰۲/۰۱", "creativity"));
            _allRows.Add(CreateRow("ارزیابی ۳۶۰ درجه", "شغلی", "شغلی", PackIconKind.Briefcase, "#38BDF8", "in-progress", 35, "۱۴۰۴/۰۱/۲۹", "360"));
            _allRows.Add(CreateRow("تست انگیزش شغلی", "روانشناسی", "روانشناسی", PackIconKind.Brain, "#A78BFA", "locked", 0, "پس از تکمیل ارزیابی مدیر فعال می‌شود", "motivation"));

            Activities.Clear();
            Activities.Add(new AssessmentActivityItem { Time = "۱۴:۲۵", Title = "تست شخصیت‌شناسی MBTI تکمیل شد" });
            Activities.Add(new AssessmentActivityItem { Time = "۱۰:۱۲", Title = "داده پرت در تست تصمیم‌گیری ثبت شد" });
            Activities.Add(new AssessmentActivityItem { Time = "۰۹:۴۰", Title = "مدال تحلیل‌گر داده اعطا شد" });
            Activities.Add(new AssessmentActivityItem { Time = "دیروز", Title = "تست مهارت‌های ارتباطی شروع شد" });
            Activities.Add(new AssessmentActivityItem { Time = "دیروز", Title = "ارزیابی مدیر تکمیل شد" });
            //ActivityList.ItemsSource = Activities;

            _pageIndex = 0;
            RefreshList();
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
                CategoryBrush = (Brush)new BrushConverter().ConvertFrom(categoryColor),
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
            FooterStats.OverallPercent = 38;
            FooterStats.TotalTests = _allRows.Count;
            FooterStats.PsychologyCount = _allRows.Count(r => r.CategoryText == "روانشناسی");
            FooterStats.JobCount = _allRows.Count(r => r.CategoryText == "شغلی");
            FooterStats.SkillCount = _allRows.Count(r => r.CategoryText == "مهارتی");
            FooterStats.OutlierCount = _allRows.Count(r => r.Status == "outlier");
            FooterStats.LockedCount = _allRows.Count(r => r.Status == "locked");
        }

        private void NewRow_RowClick(object sender, RoutedEventArgs e)
        {
            AssessmentTestRow Target = (AssessmentTestRow)sender;
            if (string.Equals(Target.Status, "locked", System.StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("این آزمون هنوز قفل است.");
                return;
            }
            var def = _catalog.Get(Target.DefinitionId);
            if (def == null)
            {
                MessageBox.Show("تعریف این آزمون پیدا نشد: " + Target.DefinitionId);
                return;
            }

            var session = new AssessmentSession
            {
                DefinitionId = def.Id,
                EmployeeId = "emp-1", // فعلاً ثابت؛ بعداً شناسه علی فاطمی
                Status = AssessmentStatus.NotStarted
            };

            // اگر Page داخل Frame است:
            NavigationService?.Navigate(new AssessmentTestShellPage(def, session));
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

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
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

        private void BtnNewTest_Click(object sender, RoutedEventArgs e) { }

        private void ProfileHeroControl_NextMedalClick(object sender, RoutedEventArgs e) { }

        private static string ToFa(int n)
        {
            const string fa = "۰۱۲۳۴۵۶۷۸۹";
            return string.Concat(n.ToString().Select(c => fa[c - '0']));
        }
    }

    public class AssessmentActivityItem
    {
        public string Time { get; set; }
        public string Title { get; set; }
    }
}