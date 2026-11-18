using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HR.Controls
{
    /// <summary>
    /// Interaction logic for OveralPerformance.xaml
    /// </summary>
    public partial class OveralPerformance : UserControl
    {
        public OveralPerformance()
        {
            InitializeComponent();
        }
        /// <summary>میانگین نمرهٔ آزمون‌ها (۰ تا ۱۰۰)، سطح آخرین آزمون و تعداد آزمون‌ها.</summary>
        public void Show(double? averagePercent, string? levelLabel, int attemptCount)
        {
            if (averagePercent == null || attemptCount == 0)
            {
                PerformanceRing.Value = 0;
                PerformsncePercentTB.Text = "—";
                LevelTB.Text = "—";
                StatusTB.Text = "بدون آزمون";
                return;
            }

            var value = Math.Clamp(averagePercent.Value, 0, 100);
            PerformanceRing.Value = value;
            PerformsncePercentTB.Text = $"{value:0}%";
            LevelTB.Text = string.IsNullOrWhiteSpace(levelLabel) ? "—" : levelLabel;
            StatusTB.Text = $"{attemptCount} آزمون";
        }

    }
}
