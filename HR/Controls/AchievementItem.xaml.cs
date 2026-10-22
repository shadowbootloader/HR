using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf;

namespace HR.Controls
{
    public partial class AchievementItem : UserControl
    {
        public static readonly DependencyProperty DateProperty =
            DependencyProperty.Register(nameof(Date), typeof(string), typeof(AchievementItem),
                new PropertyMetadata("", (d, e) => ((AchievementItem)d).DateText.Text = e.NewValue?.ToString() ?? ""));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(AchievementItem),
                new PropertyMetadata("", (d, e) => ((AchievementItem)d).TitleText.Text = e.NewValue?.ToString() ?? ""));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(AchievementItem),
                new PropertyMetadata("", (d, e) => ((AchievementItem)d).DescriptionText.Text = e.NewValue?.ToString() ?? ""));

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(nameof(IconSource), typeof(ImageSource), typeof(AchievementItem),
                new PropertyMetadata(null, (d, e) => ((AchievementItem)d).IconImage.Kind = PackIconKind.TrophyOutline));

        public static readonly DependencyProperty IconBackgroundProperty =
            DependencyProperty.Register(nameof(IconBackground), typeof(Brush), typeof(AchievementItem),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x24, 0x1F, 0x12)),
                    (d, e) => ((AchievementItem)d).IconBorder.Background = (Brush)e.NewValue));

        public static readonly DependencyProperty ShowBottomLineProperty =
            DependencyProperty.Register(nameof(ShowBottomLine), typeof(bool), typeof(AchievementItem),
                new PropertyMetadata(true, (d, e) =>
                {
                    ((AchievementItem)d).BottomLine.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
                }));

        public string Date { get => (string)GetValue(DateProperty); set => SetValue(DateProperty, value); }
        public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
        public string Description { get => (string)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }
        public ImageSource IconSource { get => (ImageSource)GetValue(IconSourceProperty); set => SetValue(IconSourceProperty, value); }
        public Brush IconBackground { get => (Brush)GetValue(IconBackgroundProperty); set => SetValue(IconBackgroundProperty, value); }
        public bool ShowBottomLine { get => (bool)GetValue(ShowBottomLineProperty); set => SetValue(ShowBottomLineProperty, value); }

        public AchievementItem()
        {
            InitializeComponent();
        }
    }
}