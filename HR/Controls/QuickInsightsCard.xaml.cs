using System.Windows.Controls;
using System.Windows.Media;

namespace HR.Controls
{
    public partial class QuickInsightsCard : UserControl
    {
        public QuickInsightsCard()
        {
            InitializeComponent();
        }

        public void ShowFacts(params (string Title, string Description)[] facts)
        {
            ItemsPanel.Children.Clear();
            foreach (var fact in facts)
            {
                if (string.IsNullOrWhiteSpace(fact.Description) || fact.Description == "—") continue;
                AddItem(fact.Title, fact.Description, "•", new SolidColorBrush(Color.FromRgb(0x2A, 0x4A, 0x6B)));
            }

            if (ItemsPanel.Children.Count == 0)
                AddItem("پرونده", "اطلاعاتی برای نمایش ثبت نشده است.", "•", new SolidColorBrush(Color.FromRgb(0x3B, 0x4A, 0x6B)));
        }

        // متد عمومی برای اضافه کردن آیتم از بیرون
        public void AddItem(string title, string description, string icon, Brush iconBackground, string? changeText = null)
        {
            var item = new InsightItem
            {
                Title = title,
                Description = description,
                Icon = icon,
                IconBackground = iconBackground,
                ChangeText = changeText ?? string.Empty
            };

            ItemsPanel.Children.Add(item);
        }
    }
}