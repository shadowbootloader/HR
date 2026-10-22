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
            Loaded += (s, e) => LoadDefaultItems();
        }

        private void LoadDefaultItems()
        {
            AddItem(
                title: "بازخورد ۳۶۰ درجه",
                date: "آوریل ۲۰۲۴",
                icon: PackIconKind.ClipboardCheckMultipleOutline,
                iconBackground: new SolidColorBrush(Color.FromRgb(0x1E, 0x16, 0x30)),
                status: "تکمیل شده"
            );

            AddItem(
                title: "بررسی عملکرد میان‌سال",
                date: "ژانویه ۲۰۲۴",
                icon: PackIconKind.ClipboardCheckMultipleOutline,
                iconBackground: new SolidColorBrush(Color.FromRgb(0x1E, 0x16, 0x30)),
                status: "تکمیل شده"
            );

            AddItem(
                title: "بررسی اهداف فصل دوم",
                date: "ژوئن ۲۰۲۴",
                icon: PackIconKind.ClipboardCheckMultipleOutline,
                iconBackground: new SolidColorBrush(Color.FromRgb(0x1E, 0x16, 0x30)),
                status: "در حال انجام"
            );
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