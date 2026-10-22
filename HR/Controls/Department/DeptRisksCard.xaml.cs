using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using MaterialDesignThemes.Wpf;
using static System.Net.Mime.MediaTypeNames;
using static HR.Controls.Department.DepartmentKpiCard;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace HR.Controls.Department
{
    public partial class DepartmentRisksCard : UserControl
    {
        public enum RiskNeonType
        {
            
            Yellow,    
            Red,  // بنفش
            Green    // سبز
        }

        public ObservableCollection<DeptRiskItem> Risks { get; set; } = new();

        public DepartmentRisksCard()
        {
            InitializeComponent();
            Items.ItemsSource = Risks;
            LoadSample();
            
        }

        private void LoadSample()
        {
            Risks.Add(new DeptRiskItem
            {
                Title = "کمبود نیروی متخصص در حوزه IT",
                Severity = "بالا",
                Owner = "مهدی کاظمی",
                OwnerRole = "معاون دپارتمان",
                
            });
            Risks.Add(new DeptRiskItem
            {
                Title = "تاخیر در تأمین بودجه پروژه‌ها",
                Severity = "متوسط",
                Owner = "سارا احمدی",
                OwnerRole = "مدیر دپارتمان",
                
            });
            Risks.Add(new DeptRiskItem
            {
                Title = "افزایش نرخ ترک خدمت",
                Severity = "متوسط",
                Owner = "الهام رضایی",
                OwnerRole = "کارشناس منابع انسانی",
                
            });
            Risks.Add(new DeptRiskItem
            {
                Title = "مشکلات سیستم اتوماسیون اداری",
                Severity = "پایین",
                Owner = "علی رستمی",
                OwnerRole = "کارشناس فناوری اطلاعات",
                
            });
        }



        public  void ApplySeverityStyle(string severity, PackIcon icon, TextBlock text)
        {
            string iconKey = severity switch
            {
                "بالا" => "RedNeonIcon",
                "متوسط" => "YellowNeonIcon",
                "پایین" => "LightSeaGreenNeonIcon",
                _ => "LightSeaGreenNeonIcon"
            };

            string textKey = severity switch
            {
                "بالا" => "RedNeonText",
                "متوسط" => "YellowNeonText",
                "پایین" => "LightSeaGreenNeonText",
                _ => "LightSeaGreenNeonText"
            };

            if (icon != null)
                icon.Style = (Style)FindResource(iconKey);

            if (text != null)
                text.Style = (Style)FindResource(textKey);
        }

        private void StackPanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is not StackPanel panel) return;
            if (panel.DataContext is not DeptRiskItem item) return;

            var icon = panel.Children.OfType<PackIcon>().FirstOrDefault();
            var text = panel.Children.OfType<TextBlock>().FirstOrDefault();

            ApplySeverityStyle(item.Severity, icon, text);
        }
    }

    public class DeptRiskItem
    {
        public string Title { get; set; }
        public string Severity { get; set; }
        public string Owner { get; set; }
        public string OwnerRole { get; set; }
        
    }
}