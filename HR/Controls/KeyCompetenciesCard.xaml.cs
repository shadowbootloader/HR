using System.Linq;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;

namespace HR.Controls
{
    public partial class KeyCompetenciesCard : UserControl
    {
        public KeyCompetenciesCard()
        {
            InitializeComponent();
        }

        public void SetData(string[] labels, double[] values)
        {
            if (labels.Length == 0 || values.Length == 0)
            {
                RadarChart.Series = Array.Empty<ISeries>();
                return;
            }

            // ========== سری اصلی داده ==========
            var mainSeries = new PolarLineSeries<double>
            {
                Values = values,
                IsClosed = true,

                // ظاهر خط و پر شدن
                Fill = new SolidColorPaint(SKColor.Parse("#404FC3F7")),
                Stroke = new SolidColorPaint(SKColor.Parse("#4FC3F7")) { StrokeThickness = 2.8f },
                LineSmoothness = 0,
                // نقطه روی هر مقدار
                GeometrySize = 11,
                GeometryFill = new SolidColorPaint(SKColor.Parse("#4FC3F7")),
                GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },

                // نمایش دقیق مقدار روی هر نقطه (مثل ۴.۶)
                DataLabelsSize = 16,
                DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#4FC3F7")),
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.End,
                DataLabelsRotation = 0,
                DataLabelsPadding = new LiveChartsCore.Drawing.Padding(5),
                DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("0.0")
            };

            // ========== حلقه‌های شش‌ضلعی به جای دایره ==========
            var polygonRings = new List<ISeries>();
            double[] ringLevels = { 25, 50, 75, 100 };

            foreach (var level in ringLevels)
            {
                var ringValues = Enumerable.Repeat(level, Math.Max(labels.Length, 1)).ToArray();

                polygonRings.Add(new PolarLineSeries<double>
                {
                    Values = ringValues,
                    IsClosed = true,
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColor.Parse("#2A3548")) { StrokeThickness = 1 },
                    GeometrySize = 0,
                    LineSmoothness = 0          // کاملاً تیز و چندضلعی
                });
            }

            // ترکیب: اول حلقه‌ها (پس‌زمینه) بعد سری اصلی
            var allSeries = new List<ISeries>();
            allSeries.AddRange(polygonRings);
            allSeries.Add(mainSeries);

            RadarChart.Series = allSeries.ToArray();
            RadarChart.DrawMargin = new LiveChartsCore.Measure.Margin(10,40,20,20);
            RadarChart.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            RadarChart.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            
            // ========== محور زاویه‌ای (نام‌ها) ==========
            RadarChart.AngleAxes = new[]
            {
        new PolarAxis
        {
            Labels = labels,
            LabelsRotation = LiveCharts.TangentAngle,
            TextSize = 13,
            
            LabelsPaint = new SolidColorPaint(SKColors.White),
            LabelsPadding = new LiveChartsCore.Drawing.Padding(0),
            LabelsBackground = new LiveChartsCore.Drawing.LvcColor(0, 0, 0, 0),
            LabelsVerticalAlignment = LiveChartsCore.Drawing.Align.End,
            SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#2A3548")) { StrokeThickness = 1 }
        }
    };

            // ========== محور شعاعی ==========
            RadarChart.RadiusAxes = new[]
            {
        new PolarAxis
        {
            MinLimit = 0,
            MaxLimit = 100,
            ShowSeparatorLines = false,   // دایره‌ها را کاملاً خاموش می‌کند
            LabelsPaint = new SolidColorPaint(SKColors.Transparent),
            TextSize = 0,
            LabelsBackground = new LiveChartsCore.Drawing.LvcColor(0, 0, 0, 0),
            SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#2A3548")) { StrokeThickness = 1 }
        }
    };
        }
    }
}