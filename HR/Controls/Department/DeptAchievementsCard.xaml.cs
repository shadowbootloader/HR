using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace HR.Controls.Department
{
    public partial class DeptAchievementsCard : UserControl
    {
        public ObservableCollection<DeptAchievementItem> Achievements { get; set; } = new();

        public DeptAchievementsCard()
        {
            InitializeComponent();
            Items.ItemsSource = Achievements;

            // نمونه
            Achievements.Add(new DeptAchievementItem
            {
                Title = "کاهش ۳۰٪ زمان جذب نیرو",
                SubTitle = "بهبود فرآیندهای جذب و استخدام",
                Date = "۱۴۰۴/۰۵/۲۸",
                IconKind = PackIconKind.Medal,
                AccentBrush = Brush("#FBBF24")
            });
            Achievements.Add(new DeptAchievementItem
            {
                Title = "اجرای موفق طرح آموزش مهارت‌های نرم",
                SubTitle = "مشارکت بیش از 90% کارکنان",
                Date = "۱۴۰۴/۰۴/۲۰",
                IconKind = PackIconKind.Star,
                AccentBrush = Brush("#22D3EE")
            });
            Achievements.Add(new DeptAchievementItem
            {
                Title = "کسب رتبه برتر رضایت کارکنان",
                SubTitle = "بر اساس امکانات دریافتی دپارتمانو استاندارد ها",
                Date = "۱۴۰۴/۰۳/۱۵",
                IconKind = PackIconKind.Trophy,
                AccentBrush = Brush("#A78BFA")
            });
            Achievements.Add(new DeptAchievementItem
            {
                Title = "کسب رتبه برتر رضایت کارکنان",
                SubTitle = "بر اساس امکانات دریافتی دپارتمانو استاندارد ها",
                Date = "۱۴۰۴/۰۳/۱۵",
                IconKind = PackIconKind.Fire,
                AccentBrush = Brush("#FF0000")
            });
        }

        private static SolidColorBrush Brush(string hex) =>
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
    }

    public class DeptAchievementItem
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Date { get; set; }
        public PackIconKind IconKind { get; set; }
        public Brush AccentBrush { get; set; }
    }
}