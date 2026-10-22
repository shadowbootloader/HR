using System.IO;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace HR.Controls
{
    public partial class PerformanceMiniGauge : UserControl
    {
        private static SKTypeface _nazanin;
        public PerformanceMiniGauge()
        {
            InitializeComponent();
            EnsureFont();
            UpdateGauge();
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(double),
                typeof(PerformanceMiniGauge),
                new PropertyMetadata(0.0, OnValueChanged));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                nameof(Label),
                typeof(string),
                typeof(PerformanceMiniGauge),
                new PropertyMetadata(""));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public ISeries[] Series { get; set; }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PerformanceMiniGauge gauge)
                gauge.UpdateGauge();
        }


        private static void EnsureFont()
        {
            if (_nazanin != null) return;

            // مسیر فایل داخل خروجی برنامه
            // Build Action فونت: Content ، Copy to Output Directory: Copy if newer
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "BNaznnBd.ttf");
            // اگر اسم فایل فرق دارد همان را بنویس، مثلاً B Nazanin.ttf

            if (File.Exists(path))
                _nazanin = SKTypeface.FromFile(path);
            else
                _nazanin = SKTypeface.Default;
        }


        private void UpdateGauge()
        {
            // انتخاب رنگ
            SKColor fillColor;
            if (Value >= 90)
                fillColor = SKColor.Parse("#22C55E");      // سبز - Excellent
            else if (Value >= 80)
                fillColor = SKColor.Parse("#3B82F6");      // آبی - Very Good
            else if (Value >= 70)
                fillColor = SKColor.Parse("#F59E0B");      // نارنجی - Good
            else
                fillColor = SKColor.Parse("#EF4444");      // قرمز - Needs Improvement
            Tracker.Fill = new SolidColorPaint(fillColor);

            Tracker.DataLabelsPaint = new SolidColorPaint(SKColors.White)
            {
                SKTypeface = _nazanin
            };
        }
    }
}