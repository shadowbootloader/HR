using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace HR.Controls
{
    public partial class RecentEvaluationsCard : UserControl
    {
        public RecentEvaluationsCard()
        {
            InitializeComponent();
        }

        // متد عمومی برای اضافه کردن آیتم
        public void AddItem(string title, string date, PackIconKind icon, Brush iconBackground, string status)
        {
            var item = new EvaluationItem
            {
                Title = title,
                Date = date,
                Icon = icon,
                IconBackground = iconBackground,
                Status = status
            };

            ItemsPanel.Children.Add(item);
        }

        // پاک کردن همه آیتم‌ها
        public void ClearItems()
        {
            ItemsPanel.Children.Clear();
        }
    }
}