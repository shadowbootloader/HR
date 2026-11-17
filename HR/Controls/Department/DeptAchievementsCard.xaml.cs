using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace HR.Controls.Department
{
    public partial class DeptAchievementsCard : UserControl
    {
        public ObservableCollection<DeptAchievementItem> Achievements { get; set; } = new();

        public DeptAchievementsCard()
        {
            InitializeComponent();
            Items.ItemsSource = Achievements;
        }
    }

    public class DeptAchievementItem
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Date { get; set; }
        public PackIconKind IconKind { get; set; }
        public Brush AccentBrush { get; set; }
    }
}