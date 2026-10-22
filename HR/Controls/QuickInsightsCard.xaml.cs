using System.Windows.Controls;
using System.Windows.Media;

namespace HR.Controls
{
    public partial class QuickInsightsCard : UserControl
    {
        public QuickInsightsCard()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadItems();
        }

        private void LoadItems()
        {
            ItemsPanel.Children.Clear();

            // آیتم ۱
            ItemsPanel.Children.Add(new InsightItem
            {
                Title = "نقطه قوت شناخته‌شده",
                Description = "همیشه به خاطر ساختن تیم‌های با عملکرد بالا تحسین می‌شود.",
                Icon = "👤",
                IconBackground = new SolidColorBrush(Color.FromRgb(0x3B, 0x4A, 0x6B))
            });

            // آیتم ۲
            ItemsPanel.Children.Add(new InsightItem
            {
                Title = "امتیاز تأثیرگذاری",
                Description = "تأثیر بالا روی مشارکت و حفظ نیروی تیم.",
                Icon = "📊",
                IconBackground = new SolidColorBrush(Color.FromRgb(0x2A, 0x4A, 0x6B)),
                ChangeText = "+۱۲٪ نسبت به ارزیابی قبلی"
            });

            // آیتم ۳
            ItemsPanel.Children.Add(new InsightItem
            {
                Title = "روند رشد",
                Description = "مسیر صعودی قوی در عملکرد.",
                Icon = "📈",
                IconBackground = new SolidColorBrush(Color.FromRgb(0x2A, 0x5A, 0x4A)),
                ChangeText = "+۸٪ نسبت به فصل قبل"
            });
        }

        // متد عمومی برای اضافه کردن آیتم از بیرون
        public void AddItem(string title, string description, string icon, Brush iconBackground, string changeText = null)
        {
            var item = new InsightItem
            {
                Title = title,
                Description = description,
                Icon = icon,
                IconBackground = iconBackground,
                ChangeText = changeText
            };

            ItemsPanel.Children.Add(item);
        }
    }
}