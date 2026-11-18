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
            Projects.CollectionChanged += (_, _) => RefreshEmpty();
            RefreshEmpty();
        }

        private void RefreshEmpty()
        {
            if (EmptyText == null) return;
            EmptyText.Visibility = Projects.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

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

            if (icon != null && text != null)
                ApplySeverityStyle(item.Color, icon, text);
        }
    }

    public class DeptProjectItem
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Progress { get; set; }
        public string DueDate { get; set; } = string.Empty;

        public ProjectNeonType Color{ get; set; }

        public Brush ProgressBrush { get; set; } = Brushes.Transparent;
    }
}