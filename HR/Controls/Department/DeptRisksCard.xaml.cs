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
            Risks.CollectionChanged += (_, _) => RefreshEmpty();
            RefreshEmpty();
        }

        private void RefreshEmpty()
        {
            if (EmptyText == null) return;
            EmptyText.Visibility = Risks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
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

            if (icon != null && text != null)
                ApplySeverityStyle(item.Severity, icon, text);
        }
    }

    public class DeptRiskItem
    {
        public string Title { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string OwnerRole { get; set; } = string.Empty;
        
    }
}