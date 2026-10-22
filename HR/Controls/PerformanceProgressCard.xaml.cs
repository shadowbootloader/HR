using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace HR.Controls
{
    public partial class PerformanceProgressCard : UserControl
    {
        public PerformanceProgressCard()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadDefaultData();
        }

        private void LoadDefaultData()
        {
            var months = new[] { "ژوئن", "ژوئیه", "اوت", "سپتامبر", "اکتبر", "نوامبر",
                                 "دسامبر", "ژانویه", "فوریه", "مارس", "آوریل", "مه" };

            var values = new double[] { 32, 45, 38, 55, 70, 62, 78, 65, 82, 75, 87, 72 };

            SetData(months, values);
        }

        public void SetData(string[] labels, double[] values)
        {
            ProgressChart.Series = new ISeries[]
{
    new LineSeries<double>
    {
        Values = values,

        // ناحیه زیر نمودار
        Fill = new SolidColorPaint(
            SKColor.Parse("#1800E5D4")),

        // خط نئون
        Stroke = new SolidColorPaint(
            SKColor.Parse("#00E5D4"))
        {
            StrokeThickness = 3f
        },

        // بدون دایره روی نقاط
        GeometrySize = 0,

        // نرمی خط
        LineSmoothness = 0.7
    }
};

            ProgressChart.XAxes = new[]
            {
    new Axis
    {
        Labels = labels,

        // اعداد محور
        LabelsPaint = new SolidColorPaint(
            SKColors.White),

        TextSize = 11,

        // خطوط عمودی بسیار ظریف
        SeparatorsPaint = new SolidColorPaint(
            SKColor.Parse("#19384A"))
        {
            StrokeThickness = 1f
        }
    }
};

            ProgressChart.YAxes = new[]
            {
    new Axis
    {
        MinLimit = 0,
        MaxLimit = 100,

        // اعداد محور
        LabelsPaint = new SolidColorPaint(
            SKColors.White),

        TextSize = 11,

        // خطوط افقی
        SeparatorsPaint = new SolidColorPaint(
            SKColor.Parse("#19384A"))
        {
            StrokeThickness = 1f
        }
    }
};
        }
    }
}