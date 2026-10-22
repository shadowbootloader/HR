using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HR.Controls
{
    public partial class AchievementsFeedbackCard : UserControl
    {
        public AchievementsFeedbackCard()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadDefaultItems();
        }

        private void LoadDefaultItems()
        {
            AddItem("مه ۲۰۲۴", "دریافت جایزه «اجراکننده برتر»", "به خاطر رهبری و نتایج استثنایی", "⭐", Color.FromRgb(0x24, 0x1F, 0x12));
            AddItem("آوریل ۲۰۲۴", "بازخورد مثبت ۳۶۰ درجه", "همکاری عالی و مدیریت ذینفعان", "👥", Color.FromRgb(0x24, 0x1F, 0x12));
            AddItem("فوریه ۲۰۲۴", "تکمیل برنامه توسعه رهبری", "تقویت قابلیت‌های رهبری", "🎓", Color.FromRgb(0x24, 0x1F, 0x12));
            //AddItem("دسامبر ۲۰۲۳", "جایزه تعالی پروژه", "رهبری موفق پروژه تحول منابع انسانی", "🏆", Color.FromRgb(0x5A, 0x4A, 0x2A));
        }

        public void AddItem(string date, string title, string description, string icon, Color iconBg)
        {
            ItemsPanel.Children.Add(new AchievementItem
            {
                Date = "مه ۲۰۲۴",
                Title = "دریافت جایزه «اجراکننده برتر»",
                Description = "به خاطر رهبری و نتایج استثنایی",
                IconSource = new BitmapImage(),
                IconBackground = new SolidColorBrush(Color.FromRgb(0x24, 0x1F, 0x12)),
                ShowBottomLine = true
            });
        }

        public void ClearItems() => ItemsPanel.Children.Clear();
    }
}