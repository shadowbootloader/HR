using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using static HR.Controls.Department.DepartmentProjectsCard;

namespace HR.Controls.Department
{
    public partial class DepartmentProjectsCard : UserControl
    {

        public enum ProjectNeonType
        {
            Cyal,
            Purple,  // بنفش
            Green    // سبز
        }
        public ObservableCollection<DeptProjectItem> Projects { get; set; } = new();

        public DepartmentProjectsCard()
        {
            InitializeComponent();
            Items.ItemsSource = Projects;
            LoadSample();
        }

        private void LoadSample()
        {
            Projects.Add(new DeptProjectItem
            {
                Name = "سیستم منابع انسانی",
                Status = "در حال اجرا",
                Progress = 75,
                DueDate = "۱۴۰۴/۰۸/۱۵",
                Color = ProjectNeonType.Cyal,
                ProgressBrush = Hex("#22D3EE")
            });
            Projects.Add(new DeptProjectItem
            {
                Name = "بهبود فرایند جذب و استخدام",
                Status = "در حال اجرا",
                Progress = 60,
                DueDate = "۱۴۰۴/۰۷/۳۰",
                Color = ProjectNeonType.Cyal,
                ProgressBrush = Hex("#34D399")
            });
            Projects.Add(new DeptProjectItem
            {
                Name = "آموزش و توسعه مهارت‌ها",
                Status = "در حال برنامه‌ریزی",
                Progress = 30,
                DueDate = "۱۴۰۴/۱۰/۰۱",
                Color = ProjectNeonType.Purple,
                ProgressBrush = Hex("#A78BFA")
            });
            Projects.Add(new DeptProjectItem
            {
                Name = "اتوماسیون اداری",
                Status = "تکمیل شده",
                Progress = 100,
                DueDate = "۱۴۰۴/۰۵/۲۰",
                Color = ProjectNeonType.Green,
                ProgressBrush = Hex("#2DD4BF")
            });
        }

        private static SolidColorBrush Hex(string hex) =>
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

        public void ApplySeverityStyle(ProjectNeonType severity, PackIcon icon, TextBlock text)
        {
            string iconKey = severity switch
            {
                ProjectNeonType.Cyal => "BlueNeonIcon",
                ProjectNeonType.Purple => "PurpleNeonIcon",
                ProjectNeonType.Green => "LightSeaGreenNeonIcon",
                _ => "LightSeaGreenNeonIcon"
            };

            string textKey = severity switch
            {
                ProjectNeonType.Cyal => "BlueNeonText",
                ProjectNeonType.Purple => "PurpleNeonText",
                ProjectNeonType.Green => "LightSeaGreenNeonText",
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
            if (panel.DataContext is not DeptProjectItem item) return;

            var icon = panel.Children.OfType<PackIcon>().FirstOrDefault();
            var text = panel.Children.OfType<TextBlock>().FirstOrDefault();

            ApplySeverityStyle(item.Color, icon, text);
        }
    }

    public class DeptProjectItem
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public int Progress { get; set; }
        public string DueDate { get; set; }

        public ProjectNeonType Color{ get; set; }

        public Brush ProgressBrush { get; set; }
    }
}