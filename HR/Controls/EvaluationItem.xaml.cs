using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace HR.Controls
{
    public partial class EvaluationItem : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(EvaluationItem),
                new PropertyMetadata("", (d, e) => ((EvaluationItem)d).TitleText.Text = e.NewValue?.ToString() ?? ""));

        public static readonly DependencyProperty DateProperty =
            DependencyProperty.Register(nameof(Date), typeof(string), typeof(EvaluationItem),
                new PropertyMetadata("", (d, e) => ((EvaluationItem)d).DateText.Text = e.NewValue?.ToString() ?? ""));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(PackIconKind), typeof(EvaluationItem),
                new PropertyMetadata(PackIconKind.ClipboardCheckMultipleOutline, (d, e) => ((EvaluationItem)d).IconText.Kind = (PackIconKind)(e.NewValue ?? PackIconKind.ClipboardCheckMultipleOutline)));

        public static readonly DependencyProperty IconBackgroundProperty =
            DependencyProperty.Register(nameof(IconBackground), typeof(Brush), typeof(EvaluationItem),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x3B, 0x4A, 0x6B)),
                    (d, e) => ((EvaluationItem)d).IconBorder.Background = (Brush)e.NewValue));

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(nameof(Status), typeof(string), typeof(EvaluationItem),
                new PropertyMetadata("تکمیل شده", OnStatusChanged));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Date
        {
            get => (string)GetValue(DateProperty);
            set => SetValue(DateProperty, value);
        }

        public PackIconKind Icon
        {
            get => (PackIconKind)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public Brush IconBackground
        {
            get => (Brush)GetValue(IconBackgroundProperty);
            set => SetValue(IconBackgroundProperty, value);
        }

        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public EvaluationItem()
        {
            InitializeComponent();
        }

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (EvaluationItem)d;
            string status = e.NewValue?.ToString() ?? "تکمیل شده";

            item.StatusText.Text = status;

            if (status.Contains("در حال انجام") || status.Contains("In Progress"))
            {
                // زرد
                item.StatusBorder.Background = new SolidColorBrush(Color.FromRgb(0x3A, 0x2F, 0x1E));
                item.StatusText.Foreground = new SolidColorBrush(Color.FromRgb(0xFB, 0xBF, 0x24));
            }
            else
            {
                // سبز
                item.StatusBorder.Background = new SolidColorBrush(Color.FromRgb(0x1E, 0x3A, 0x2F));
                item.StatusText.Foreground = new SolidColorBrush(Color.FromRgb(0x4A, 0xDE, 0x80));
            }
        }
    }
}