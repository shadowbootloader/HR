using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace HR.Controls.Department
{
    public partial class DepartmentFinanceCard : UserControl
    {
        public DepartmentFinanceCard()
        {
            InitializeComponent();

            // نمونه مطابق عکس
            Budget = 1_200_000_000;
            Spent = 804_000_000;
            Rebuild();
        }

        public static readonly DependencyProperty BudgetProperty =
            DependencyProperty.Register(nameof(Budget), typeof(long), typeof(DepartmentFinanceCard),
                new PropertyMetadata(0L, OnChanged));
        public long Budget
        {
            get => (long)GetValue(BudgetProperty);
            set => SetValue(BudgetProperty, value);
        }

        public static readonly DependencyProperty SpentProperty =
            DependencyProperty.Register(nameof(Spent), typeof(long), typeof(DepartmentFinanceCard),
                new PropertyMetadata(0L, OnChanged));
        public long Spent
        {
            get => (long)GetValue(SpentProperty);
            set => SetValue(SpentProperty, value);
        }

        public static readonly DependencyProperty BudgetTextProperty =
            DependencyProperty.Register(nameof(BudgetText), typeof(string), typeof(DepartmentFinanceCard),
                new PropertyMetadata("۰"));
        public string BudgetText
        {
            get => (string)GetValue(BudgetTextProperty);
             set => SetValue(BudgetTextProperty, value);
        }

        public static readonly DependencyProperty SpentTextProperty =
            DependencyProperty.Register(nameof(SpentText), typeof(string), typeof(DepartmentFinanceCard),
                new PropertyMetadata("۰"));
        public string SpentText
        {
            get => (string)GetValue(SpentTextProperty);
             set => SetValue(SpentTextProperty, value);
        }

        public static readonly DependencyProperty RemainingTextProperty =
            DependencyProperty.Register(nameof(RemainingText), typeof(string), typeof(DepartmentFinanceCard),
                new PropertyMetadata("۰"));
        public string RemainingText
        {
            get => (string)GetValue(RemainingTextProperty);
             set => SetValue(RemainingTextProperty, value);
        }

        public static readonly DependencyProperty UsedPercentProperty =
            DependencyProperty.Register(nameof(UsedPercent), typeof(double), typeof(DepartmentFinanceCard),
                new PropertyMetadata(0.0));
        public double UsedPercent
        {
            get => (double)GetValue(UsedPercentProperty);
            set => SetValue(UsedPercentProperty, value);
        }

        public static readonly DependencyProperty SeriesProperty =
            DependencyProperty.Register(nameof(Series), typeof(IEnumerable<ISeries>), typeof(DepartmentFinanceCard),
                new PropertyMetadata(null));
        public IEnumerable<ISeries> Series
        {
            get => (IEnumerable<ISeries>)GetValue(SeriesProperty);
             set => SetValue(SeriesProperty, value);
        }

         static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DepartmentFinanceCard card)
                card.Rebuild();
        }

         void Rebuild()
        {
            long remaining = Budget - Spent;
            if (remaining < 0) remaining = 0;

            UsedPercent = Budget > 0 ? (Spent * 100.0 / Budget) : 0;

            BudgetText = Budget.ToString("N0");
            SpentText = Spent.ToString("N0");
            RemainingText = remaining.ToString("N0");

            double used = UsedPercent;
            double free = 100 - used;
            if (free < 0) free = 0;

            Series = new ISeries[]
            {
                // بخش مصرف‌شده
                new PieSeries<double>
                {
                    Values = new[] { used },
                    Fill = new SolidColorPaint(SKColor.Parse("#2DD4BF")),
                    InnerRadius = 55,
                    MaxRadialColumnWidth = 14,
                    Stroke = null
                },
                // باقیمانده (خاکستری تیره)
                new PieSeries<double>
                {
                    Values = new[] { free },
                    Fill = new SolidColorPaint(SKColor.Parse("#1E293B")),
                    InnerRadius = 55,
                    MaxRadialColumnWidth = 14,
                    Stroke = null
                }
            };
        }
    }
}