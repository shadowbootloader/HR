using System.Windows.Controls;
using System.Windows.Media;

namespace HR.Controls
{
    public partial class StrengthsAreasCard : UserControl
    {
        public StrengthsAreasCard()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadDefaultItems();
        }

        private void LoadDefaultItems()
        {
            // نقاط قوت (سبز)
            AddStrength("رهبر استثنایی افراد");
            AddStrength("ذهنیت استراتژیک");
            AddStrength("نتیجه‌گرا");
            AddStrength("اعتبار بالا");

            // نقاط قابل بهبود (زرد/نارنجی)
            AddArea("بهتر تفویض اختیار کند");
            AddArea("مهارت‌های تحلیل داده را عمیق‌تر کند");
            AddArea("حضور اجرایی خود را افزایش دهد");
        }

        // متد عمومی برای اضافه کردن نقطه قوت
        public void AddStrength(string title)
        {
            var item = new StrengthAreaItem
            {
                Title = title,
                DotColor = new SolidColorBrush(Color.FromRgb(0x4A, 0xDE, 0x80)) // سبز
            };
            StrengthsPanel.Children.Add(item);
        }

        // متد عمومی برای اضافه کردن نقطه قابل بهبود
        public void AddArea(string title)
        {
            var item = new StrengthAreaItem
            {
                Title = title,
                DotColor = new SolidColorBrush(Color.FromRgb(0xFB, 0xBF, 0x24)) // زرد
            };
            AreasPanel.Children.Add(item);
        }

        // پاک کردن همه آیتم‌ها
        public void ClearAll()
        {
            StrengthsPanel.Children.Clear();
            AreasPanel.Children.Clear();
        }
    }
}