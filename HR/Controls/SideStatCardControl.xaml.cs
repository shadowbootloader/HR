using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace HR.Controls
{
    public partial class SideStatCardControl : UserControl
    {
        public SideStatCardControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(SideStatCardControl), new PropertyMetadata("عنوان"));
        public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

        public static readonly DependencyProperty ValueTextProperty =
            DependencyProperty.Register(nameof(ValueText), typeof(string), typeof(SideStatCardControl), new PropertyMetadata("0"));
        public string ValueText { get => (string)GetValue(ValueTextProperty); set => SetValue(ValueTextProperty, value); }

        public static readonly DependencyProperty SubTextProperty =
            DependencyProperty.Register(nameof(SubText), typeof(string), typeof(SideStatCardControl), new PropertyMetadata(""));
        public string SubText { get => (string)GetValue(SubTextProperty); set => SetValue(SubTextProperty, value); }

        public static readonly DependencyProperty AccentBrushProperty =
            DependencyProperty.Register(nameof(AccentBrush), typeof(Brush), typeof(SideStatCardControl),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00E5D4"))));
        public Brush AccentBrush { get => (Brush)GetValue(AccentBrushProperty); set => SetValue(AccentBrushProperty, value); }

        public static readonly DependencyProperty IconBackgroundProperty =
            DependencyProperty.Register(nameof(IconBackground), typeof(Brush), typeof(SideStatCardControl),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#083344"))));
        public Brush IconBackground { get => (Brush)GetValue(IconBackgroundProperty); set => SetValue(IconBackgroundProperty, value); }

        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.Register(nameof(IconKind), typeof(PackIconKind), typeof(SideStatCardControl),
                new PropertyMetadata(PackIconKind.ChartLine));
        public PackIconKind IconKind { get => (PackIconKind)GetValue(IconKindProperty); set => SetValue(IconKindProperty, value); }

        public static readonly DependencyProperty SparkPointsProperty =
            DependencyProperty.Register(nameof(SparkPoints), typeof(PointCollection), typeof(SideStatCardControl),
                new PropertyMetadata(new PointCollection(new[]
                {
                    new Point(0, 28), new Point(30, 20), new Point(60, 24),
                    new Point(90, 12), new Point(120, 18), new Point(150, 8), new Point(180, 14), new Point(205,23), new Point(231,1),new Point(238,15)
                })));
        public PointCollection SparkPoints
        {
            get => (PointCollection)GetValue(SparkPointsProperty);
            set => SetValue(SparkPointsProperty, value);
        }
    }
}