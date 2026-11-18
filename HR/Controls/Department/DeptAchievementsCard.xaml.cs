using System.Collections.ObjectModel;
using System.Windows;
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
            Achievements.CollectionChanged += (_, _) => RefreshEmpty();
            RefreshEmpty();
        }

        private void RefreshEmpty()
        {
            if (EmptyText == null) return;
            EmptyText.Visibility = Achievements.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class DeptAchievementItem
    {
        public string Title { get; set; } = string.Empty;
        public string SubTitle { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public PackIconKind IconKind { get; set; }
        public Brush AccentBrush { get; set; } = Brushes.Transparent;
    }
}