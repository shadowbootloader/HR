using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HR.Controls
{
    public partial class SummaryCardControl : UserControl
    {
        public SummaryCardControl()
        {
            InitializeComponent();
            // برای اینکه Bindingها کار کنند
            //this.DataContext = this;
        }

        // ==================== Title ====================
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(SummaryCardControl),
                new PropertyMetadata("Total Employees"));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        // ==================== Value ====================
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(SummaryCardControl),
                new PropertyMetadata("0"));

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        // ==================== SubText ====================
        public static readonly DependencyProperty SubTextProperty =
            DependencyProperty.Register(nameof(SubText), typeof(string), typeof(SummaryCardControl),
                new PropertyMetadata(""));

        public string SubText
        {
            get => (string)GetValue(SubTextProperty);
            set => SetValue(SubTextProperty, value);
        }

        // ==================== SubTextColor ====================
        public static readonly DependencyProperty SubTextColorProperty =
            DependencyProperty.Register(nameof(SubTextColor), typeof(Brush), typeof(SummaryCardControl),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E"))));

        public Brush SubTextColor
        {
            get => (Brush)GetValue(SubTextColorProperty);
            set => SetValue(SubTextColorProperty, value);
        }

        public static readonly DependencyProperty IconContentProperty =
    DependencyProperty.Register(
        nameof(IconContent),
        typeof(object),
        typeof(SummaryCardControl),
        new PropertyMetadata(null));

        public object IconContent
        {
            get => GetValue(IconContentProperty);
            set => SetValue(IconContentProperty, value);
        }

        // ==================== IconBackground ====================
        public static readonly DependencyProperty IconBackgroundProperty =
            DependencyProperty.Register(nameof(IconBackground), typeof(Brush), typeof(SummaryCardControl),
                new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        public Brush IconBackground
        {
            get => (Brush)GetValue(IconBackgroundProperty);
            set => SetValue(IconBackgroundProperty, value);
        }
    }
}