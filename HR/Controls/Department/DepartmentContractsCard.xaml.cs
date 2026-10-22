using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace HR.Controls.Department
{
    public partial class DepartmentContractsCard : UserControl
    {
        public DepartmentContractsCard()
        {
            InitializeComponent();

            // نمونه مطابق عکس/لیست تو
            CompletedCount = 18;      // تکمیل شده
            ExpiringCount = 4;        // در حال اتمام
            CancelledCount = 4;       // لغو شده
            ReviewingCount = 3;       // در حال بررسی
            InitialApprovalCount = 5; // تایید اولیه
            ProblemCount = 2;         // دارای مشکل

            RebuildChart();
        }

        // تکمیل شده — Lime
        public static readonly DependencyProperty CompletedCountProperty =
            DependencyProperty.Register(nameof(CompletedCount), typeof(int), typeof(DepartmentContractsCard),
                new PropertyMetadata(0, OnCountsChanged));
        public int CompletedCount
        {
            get => (int)GetValue(CompletedCountProperty);
            set => SetValue(CompletedCountProperty, value);
        }

        // در حال اتمام — Yellow
        public static readonly DependencyProperty ExpiringCountProperty =
            DependencyProperty.Register(nameof(ExpiringCount), typeof(int), typeof(DepartmentContractsCard),
                new PropertyMetadata(0, OnCountsChanged));
        public int ExpiringCount
        {
            get => (int)GetValue(ExpiringCountProperty);
            set => SetValue(ExpiringCountProperty, value);
        }

        // لغو شده — Red
        public static readonly DependencyProperty CancelledCountProperty =
            DependencyProperty.Register(nameof(CancelledCount), typeof(int), typeof(DepartmentContractsCard),
                new PropertyMetadata(0, OnCountsChanged));
        public int CancelledCount
        {
            get => (int)GetValue(CancelledCountProperty);
            set => SetValue(CancelledCountProperty, value);
        }

        // در حال بررسی — HotPink
        public static readonly DependencyProperty ReviewingCountProperty =
            DependencyProperty.Register(nameof(ReviewingCount), typeof(int), typeof(DepartmentContractsCard),
                new PropertyMetadata(0, OnCountsChanged));
        public int ReviewingCount
        {
            get => (int)GetValue(ReviewingCountProperty);
            set => SetValue(ReviewingCountProperty, value);
        }

        // تایید اولیه — LightGreen
        public static readonly DependencyProperty InitialApprovalCountProperty =
            DependencyProperty.Register(nameof(InitialApprovalCount), typeof(int), typeof(DepartmentContractsCard),
                new PropertyMetadata(0, OnCountsChanged));
        public int InitialApprovalCount
        {
            get => (int)GetValue(InitialApprovalCountProperty);
            set => SetValue(InitialApprovalCountProperty, value);
        }

        // دارای مشکل — OrangeRed
        public static readonly DependencyProperty ProblemCountProperty =
            DependencyProperty.Register(nameof(ProblemCount), typeof(int), typeof(DepartmentContractsCard),
                new PropertyMetadata(0, OnCountsChanged));
        public int ProblemCount
        {
            get => (int)GetValue(ProblemCountProperty);
            set => SetValue(ProblemCountProperty, value);
        }

        public static readonly DependencyProperty TotalCountProperty =
            DependencyProperty.Register(nameof(TotalCount), typeof(int), typeof(DepartmentContractsCard),
                new PropertyMetadata(0));
        public int TotalCount
        {
            get => (int)GetValue(TotalCountProperty);
            private set => SetValue(TotalCountProperty, value);
        }

        public static readonly DependencyProperty SeriesProperty =
            DependencyProperty.Register(nameof(Series), typeof(IEnumerable<ISeries>), typeof(DepartmentContractsCard),
                new PropertyMetadata(null));
        public IEnumerable<ISeries> Series
        {
            get => (IEnumerable<ISeries>)GetValue(SeriesProperty);
            private set => SetValue(SeriesProperty, value);
        }

        private static void OnCountsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DepartmentContractsCard card)
                card.RebuildChart();
        }

        private void RebuildChart()
        {
            TotalCount = CompletedCount + ExpiringCount + CancelledCount
                       + ReviewingCount + InitialApprovalCount + ProblemCount;

            Series = new ISeries[]
            {
                MakeSlice(CompletedCount, "#32CD32"),      // Lime
                MakeSlice(ExpiringCount, "#FFD700"),       // Yellow
                MakeSlice(CancelledCount, "#FF0000"),      // Red
                MakeSlice(ReviewingCount, "#FF69B4"),      // HotPink
                MakeSlice(InitialApprovalCount, "#90EE90"),// LightGreen
                MakeSlice(ProblemCount, "#FF4500")         // OrangeRed
            };
        }

        private static PieSeries<double> MakeSlice(int value, string color) =>
            new PieSeries<double>
            {
                Values = new double[] { value },
                Fill = new SolidColorPaint(SKColor.Parse(color)),
                InnerRadius = 60,
                MaxRadialColumnWidth = 14,
                Stroke = null
            };
    }
}