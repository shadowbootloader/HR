// AssessmentFooterStatsControl.xaml.cs
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;

namespace HR.Controls.Assessment
{
    public partial class AssessmentFooterStatsControl : UserControl
    {
        public AssessmentFooterStatsControl()
        {
            InitializeComponent();
            Loaded += (_, __) => Refresh();
            SizeChanged += (_, __) => DrawRing();
        }

        public static readonly DependencyProperty OverallPercentProperty =
            DependencyProperty.Register(nameof(OverallPercent), typeof(int), typeof(AssessmentFooterStatsControl),
                new PropertyMetadata(38, OnChanged));
        public int OverallPercent
        {
            get => (int)GetValue(OverallPercentProperty);
            set => SetValue(OverallPercentProperty, value);
        }

        public static readonly DependencyProperty TotalTestsProperty =
            DependencyProperty.Register(nameof(TotalTests), typeof(int), typeof(AssessmentFooterStatsControl),
                new PropertyMetadata(28, OnChanged));
        public int TotalTests
        {
            get => (int)GetValue(TotalTestsProperty);
            set => SetValue(TotalTestsProperty, value);
        }

        public static readonly DependencyProperty PsychologyCountProperty =
            DependencyProperty.Register(nameof(PsychologyCount), typeof(int), typeof(AssessmentFooterStatsControl),
                new PropertyMetadata(8, OnChanged));
        public int PsychologyCount
        {
            get => (int)GetValue(PsychologyCountProperty);
            set => SetValue(PsychologyCountProperty, value);
        }

        public static readonly DependencyProperty JobCountProperty =
            DependencyProperty.Register(nameof(JobCount), typeof(int), typeof(AssessmentFooterStatsControl),
                new PropertyMetadata(0, OnChanged));
        public int JobCount
        {
            get => (int)GetValue(JobCountProperty);
            set => SetValue(JobCountProperty, value);
        }

        public static readonly DependencyProperty SkillCountProperty =
            DependencyProperty.Register(nameof(SkillCount), typeof(int), typeof(AssessmentFooterStatsControl),
                new PropertyMetadata(0, OnChanged));
        public int SkillCount
        {
            get => (int)GetValue(SkillCountProperty);
            set => SetValue(SkillCountProperty, value);
        }

        public static readonly DependencyProperty OutlierCountProperty =
            DependencyProperty.Register(nameof(OutlierCount), typeof(int), typeof(AssessmentFooterStatsControl),
                new PropertyMetadata(3, OnChanged));
        public int OutlierCount
        {
            get => (int)GetValue(OutlierCountProperty);
            set => SetValue(OutlierCountProperty, value);
        }

        public static readonly DependencyProperty LockedCountProperty =
            DependencyProperty.Register(nameof(LockedCount), typeof(int), typeof(AssessmentFooterStatsControl),
                new PropertyMetadata(4, OnChanged));
        public int LockedCount
        {
            get => (int)GetValue(LockedCountProperty);
            set => SetValue(LockedCountProperty, value);
        }

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AssessmentFooterStatsControl c) c.Refresh();
        }

        private void Refresh()
        {
            if (TxtPercent == null) return;
            TxtPercent.Text = ToFa(OverallPercent) + "٪";
            TxtTotal.Text = ToFa(TotalTests);
            TxtPsychology.Text = ToFa(PsychologyCount);
            TxtJob.Text = JobCount > 0 ? ToFa(JobCount) : "";
            TxtSkill.Text = SkillCount > 0 ? ToFa(SkillCount) : "";
            TxtOutlier.Text = ToFa(OutlierCount);
            TxtLocked.Text = ToFa(LockedCount);
            DrawRing();
        }

        private void DrawRing()
        {
            if (RingPath == null) return;

            const double size = 48;
            const double stroke = 5;
            double radius = (size - stroke) / 2.0;
            double cx = size / 2.0;
            double cy = size / 2.0;

            double pct = Math.Max(0, Math.Min(100, OverallPercent)) / 100.0;
            if (pct <= 0)
            {
                RingPath.Data = Geometry.Empty;
                return;
            }

            // از بالای دایره، ساعت‌گرد
            double startAngle = -90;
            double endAngle = startAngle + 360 * pct;
            var geo = new PathGeometry();
            var fig = new PathFigure
            {
                StartPoint = PointOnCircle(cx, cy, radius, startAngle),
                IsClosed = false
            };
            fig.Segments.Add(new ArcSegment
            {
                Point = PointOnCircle(cx, cy, radius, endAngle),
                Size = new Size(radius, radius),
                IsLargeArc = pct > 0.5,
                SweepDirection = SweepDirection.Clockwise
            });
            geo.Figures.Add(fig);
            RingPath.Data = geo;
        }

        private static Point PointOnCircle(double cx, double cy, double r, double angleDeg)
        {
            double rad = angleDeg * Math.PI / 180.0;
            return new Point(cx + r * Math.Cos(rad), cy + r * Math.Sin(rad));
        }

        private static string ToFa(int n)
        {
            const string fa = "۰۱۲۳۴۵۶۷۸۹";
            return string.Concat(n.ToString(CultureInfo.InvariantCulture).Select(c =>
                char.IsDigit(c) ? fa[c - '0'] : c));
        }
    }
}