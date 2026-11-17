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
        }

        public void AddItem(string date, string title, string description, string icon, Color iconBg)
        {
            ItemsPanel.Children.Add(new AchievementItem
            {
                Date = date,
                Title = title,
                Description = description,
                IconSource = new BitmapImage(),
                IconBackground = new SolidColorBrush(iconBg),
                ShowBottomLine = true
            });
        }

        public void ClearItems() => ItemsPanel.Children.Clear();
    }
}