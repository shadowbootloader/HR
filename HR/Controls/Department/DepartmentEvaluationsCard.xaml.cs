using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace HR.Controls.Department
{
    public partial class DepartmentEvaluationsCard : UserControl
    {
        public DepartmentEvaluationsCard()
        {
            InitializeComponent();
            CompletedCount = 18;
            InProgressCount = 10;
            WaitingCount = 4;
            Rebuild();
        }

        public static readonly DependencyProperty CompletedCountProperty =
            DependencyProperty.Register(nameof(CompletedCount), typeof(int), typeof(DepartmentEvaluationsCard),
                new PropertyMetadata(0, OnChanged));
        public int CompletedCount
        {
            get => (int)GetValue(CompletedCountProperty);
            set => SetValue(CompletedCountProperty, value);
        }

        public static readonly DependencyProperty InProgressCountProperty =
            DependencyProperty.Register(nameof(InProgressCount), typeof(int), typeof(DepartmentEvaluationsCard),
                new PropertyMetadata(0, OnChanged));
        public int InProgressCount
        {
            get => (int)GetValue(InProgressCountProperty);
            set => SetValue(InProgressCountProperty, value);
        }

        public static readonly DependencyProperty WaitingCountProperty =
            DependencyProperty.Register(nameof(WaitingCount), typeof(int), typeof(DepartmentEvaluationsCard),
                new PropertyMetadata(0, OnChanged));
        public int WaitingCount
        {
            get => (int)GetValue(WaitingCountProperty);
            set => SetValue(WaitingCountProperty, value);
        }

        public static readonly DependencyProperty TotalCountProperty =
            DependencyProperty.Register(nameof(TotalCount), typeof(int), typeof(DepartmentEvaluationsCard),
                new PropertyMetadata(0));
        public int TotalCount
        {
            get => (int)GetValue(TotalCountProperty);
            private set => SetValue(TotalCountProperty, value);
        }

        public static readonly DependencyProperty SeriesProperty =
            DependencyProperty.Register(nameof(Series), typeof(IEnumerable<ISeries>), typeof(DepartmentEvaluationsCard),
                new PropertyMetadata(null));
        public IEnumerable<ISeries> Series
        {
            get => (IEnumerable<ISeries>)GetValue(SeriesProperty);
            private set => SetValue(SeriesProperty, value);
        }

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DepartmentEvaluationsCard c) c.Rebuild();
        }

        private void Rebuild()
        {
            TotalCount = CompletedCount + InProgressCount + WaitingCount;

            Series = new ISeries[]
            {
                Slice(CompletedCount, "#FF00FF00"),
                Slice(InProgressCount, "#FF22D3EE"),
                Slice(WaitingCount, "#ffff00")
            };
        }

        private static PieSeries<double> Slice(int value, string color) =>
            new PieSeries<double>
            {
                Values = new double[] { value },
                Fill = new SolidColorPaint(SKColor.Parse(color)),
                InnerRadius = 55,
                MaxRadialColumnWidth = 14,
                Stroke = null
            };
    }
}