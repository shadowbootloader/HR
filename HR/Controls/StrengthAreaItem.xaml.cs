using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HR.Controls
{
    public partial class StrengthAreaItem : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(StrengthAreaItem),
                new PropertyMetadata("", (d, e) => ((StrengthAreaItem)d).TitleText.Text = e.NewValue?.ToString() ?? ""));

        public static readonly DependencyProperty DotColorProperty =
            DependencyProperty.Register(nameof(DotColor), typeof(Brush), typeof(StrengthAreaItem),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x4A, 0xDE, 0x80)),
                    (d, e) => ((StrengthAreaItem)d).DotText.Foreground = (Brush)e.NewValue));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public Brush DotColor
        {
            get => (Brush)GetValue(DotColorProperty);
            set => SetValue(DotColorProperty, value);
        }

        public StrengthAreaItem()
        {
            InitializeComponent();
        }
    }
}