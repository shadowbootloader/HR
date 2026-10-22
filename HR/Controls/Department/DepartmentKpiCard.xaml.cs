using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using MaterialDesignThemes.Wpf;

namespace HR.Controls.Department
{
    public partial class DepartmentKpiCard : UserControl
    {
        public enum KpiNeonType
        {
            None,
            Cyan,    // فیروزه‌ای
            Purple,  // بنفش
            Green    // سبز
        }
        public DepartmentKpiCard()
        {
            InitializeComponent();
            Loaded += (_, __) => ApplyNeon();
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(DepartmentKpiCard), new PropertyMetadata(""));
        public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

        public static readonly DependencyProperty ValueTextProperty =
            DependencyProperty.Register(nameof(ValueText), typeof(string), typeof(DepartmentKpiCard), new PropertyMetadata("0"));
        public string ValueText { get => (string)GetValue(ValueTextProperty); set => SetValue(ValueTextProperty, value); }

        public static readonly DependencyProperty SubTextProperty =
            DependencyProperty.Register(nameof(SubText), typeof(string), typeof(DepartmentKpiCard), new PropertyMetadata(""));
        public string SubText { get => (string)GetValue(SubTextProperty); set => SetValue(SubTextProperty, value); }

        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.Register(nameof(IconKind), typeof(PackIconKind), typeof(DepartmentKpiCard),
                new PropertyMetadata(PackIconKind.ChartBox));
        public PackIconKind IconKind { get => (PackIconKind)GetValue(IconKindProperty); set => SetValue(IconKindProperty, value); }

        public static readonly DependencyProperty NeonTypeProperty =
            DependencyProperty.Register(nameof(NeonType), typeof(KpiNeonType), typeof(DepartmentKpiCard),
                new PropertyMetadata(KpiNeonType.Cyan, (d, e) => ((DepartmentKpiCard)d).ApplyNeon()));

        public KpiNeonType NeonType
        {
            get => (KpiNeonType)GetValue(NeonTypeProperty);
            set => SetValue(NeonTypeProperty, value);
        }

        // رنگ محاسبه‌شده برای Binding در XAML
        private Brush _userAccent;

        public new static readonly DependencyProperty AccentProperty =
            DependencyProperty.Register(nameof(Accent), typeof(Brush), typeof(DepartmentKpiCard),
                new PropertyMetadata(null, (d, e) =>
                {
                    var c = (DepartmentKpiCard)d;
                    c._userAccent = e.NewValue as Brush;
                    if (c.NeonType == KpiNeonType.None)
                        c.ApplyNeon();
                }));

        public Brush Accent
        {
            get => (Brush)GetValue(AccentProperty);
            set => SetValue(AccentProperty, value);
        }
        private void ApplyNeon()
        {
            Color color = NeonType switch
            {
                KpiNeonType.Purple => (Color)ColorConverter.ConvertFromString("#A78BFA"),
                KpiNeonType.Green => (Color)ColorConverter.ConvertFromString("#34D399"),
                KpiNeonType.Cyan => (Color)ColorConverter.ConvertFromString("#22D3EE"),
                _ => (Color)ColorConverter.ConvertFromString("#38BDF8") // None → آبی ساده
            };

            // اگر کاربر Accent را دستی داده و NeonType = None
            if (NeonType == KpiNeonType.None && _userAccent != null)
                Accent = _userAccent;
            else
                Accent = new SolidColorBrush(color);

            if (NeonType == KpiNeonType.None)
            {
                if (IconBorder != null) IconBorder.Effect = null;
                if (IconControl != null) IconControl.Effect = null;
            }
            else
            {
                var glow = new DropShadowEffect
                {
                    Color = color,
                    BlurRadius = 14,
                    ShadowDepth = 0,
                    Opacity = 0.85
                };
                if (IconBorder != null) IconBorder.Effect = glow;
                if (IconControl != null) IconControl.Effect = glow;
            }
        }
    }
}