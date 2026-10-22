using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HR.Controls
{
    public partial class InsightItem : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(InsightItem),
                new PropertyMetadata("", (d, e) => ((InsightItem)d).TitleText.Text = e.NewValue?.ToString() ?? ""));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(InsightItem),
                new PropertyMetadata("", (d, e) => ((InsightItem)d).DescriptionText.Text = e.NewValue?.ToString() ?? ""));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(string), typeof(InsightItem),
                new PropertyMetadata("👤", (d, e) => ((InsightItem)d).IconText.Text = e.NewValue?.ToString() ?? "👤"));

        public static readonly DependencyProperty IconBackgroundProperty =
            DependencyProperty.Register(nameof(IconBackground), typeof(Brush), typeof(InsightItem),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x3B, 0x4A, 0x6B)),
                    (d, e) => ((InsightItem)d).IconBorder.Background = (Brush)e.NewValue));

        public static readonly DependencyProperty ChangeTextProperty =
            DependencyProperty.Register(nameof(ChangeText), typeof(string), typeof(InsightItem),
                new PropertyMetadata("", OnChangeTextChanged));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public Brush IconBackground
        {
            get => (Brush)GetValue(IconBackgroundProperty);
            set => SetValue(IconBackgroundProperty, value);
        }

        public string ChangeText
        {
            get => (string)GetValue(ChangeTextProperty);
            set => SetValue(ChangeTextProperty, value);
        }

        public InsightItem()
        {
            InitializeComponent();
        }

        private static void OnChangeTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (InsightItem)d;
            string value = e.NewValue?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(value))
            {
                item.ChangeTextTB.Visibility = Visibility.Collapsed;
            }
            else
            {
                item.ChangeTextTB.Text = value;
                item.ChangeTextTB.Visibility = Visibility.Visible;
            }
        }
    }
}