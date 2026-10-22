using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HR.Controls
{
    public enum StepSide
    {
        Right,
        Left
    }


    

    public partial class NotchedBorder : UserControl
    {


        public string EmployeeIDStr { get; set; }
        public string BirthDateStr { get; set; }
        public string MarageStatusStr { get; set; }
        public string PhoneNumberStr { get; set; }
        public string JoinDateStr { get; set; }
        public string ManagerStr { get; set; }



        public  void FillNotchedBorder(string EmployeeID , string BirthDate , string MarageStatus , string PhoneNumber , string JoinDate , string Manager)
        {
            //InitializeComponent();
            
            EmployeeIDStr = EmployeeID;
            BirthDateStr = BirthDate;
            MarageStatusStr= MarageStatus;
            PhoneNumberStr = PhoneNumber;
            JoinDateStr = JoinDate;
            ManagerStr = Manager;
        }

        public NotchedBorder()
        {
            InitializeComponent();
            Loaded += (s, e) => UpdateShape();
            SizeChanged += (s, e) => UpdateShape();
            
        }

        // ==================== پراپرتی‌های شکل ====================

        public static readonly DependencyProperty TopRatioProperty =
            DependencyProperty.Register(nameof(TopRatio), typeof(double), typeof(NotchedBorder),
                new PropertyMetadata(0.70, OnShapeChanged));

        public double TopRatio
        {
            get => (double)GetValue(TopRatioProperty);
            set => SetValue(TopRatioProperty, value);
        }

        public static readonly DependencyProperty StepDepthProperty =
            DependencyProperty.Register(nameof(StepDepth), typeof(double), typeof(NotchedBorder),
                new PropertyMetadata(0.18, OnShapeChanged));

        public double StepDepth
        {
            get => (double)GetValue(StepDepthProperty);
            set => SetValue(StepDepthProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(double), typeof(NotchedBorder),
                new PropertyMetadata(14.0, OnShapeChanged));



        public static readonly DependencyProperty StepStartXProperty =
    DependencyProperty.Register(
        nameof(StepStartX),
        typeof(double),
        typeof(NotchedBorder),          // اسم کلاس خودت را بگذار
        new PropertyMetadata(0.0));


        public double StepStartX
        {
            get => (double)GetValue(StepStartXProperty);
            private set => SetValue(StepStartXProperty, value);
        }

        public double CornerRadius
        {
            get => (double)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty StepSideProperty =
            DependencyProperty.Register(nameof(StepSide), typeof(StepSide), typeof(NotchedBorder),
                new PropertyMetadata(StepSide.Right, OnShapeChanged));

        public StepSide StepSide
        {
            get => (StepSide)GetValue(StepSideProperty);
            set => SetValue(StepSideProperty, value);
        }

        // ==================== پراپرتی‌های Border-like ====================

        public static readonly new DependencyProperty BackgroundProperty =
            DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(NotchedBorder),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"))));

        public new Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(NotchedBorder),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"))));

        public Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        public static readonly new DependencyProperty PaddingProperty =
            DependencyProperty.Register(nameof(Padding), typeof(Thickness), typeof(NotchedBorder),
                new PropertyMetadata(new Thickness(16)));

        public new Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        // ==================== متدها ====================

        private static void OnShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NotchedBorder card)
                card.UpdateShape();
        }

        private void UpdateShape()
        {
            if (ShapePath == null) return;

            double w = 1.0;
            double h = 1.0;
            double r = Math.Max(0, CornerRadius / Math.Max(ActualWidth, 1)); // نرمال‌سازی تقریبی
            r = Math.Min(r, 0.15); // محدودیت برای جلوگیری از خراب شدن شکل

            double top = Math.Clamp(TopRatio, 0.3, 0.9);
            double depth = Math.Clamp(StepDepth, 0.08, 0.45);

            var geometry = new PathGeometry();
            var figure = new PathFigure { IsClosed = true };

            if (StepSide == StepSide.Right)
            {
                // پله سمت راست
                figure.StartPoint = new Point(r, 0);

                // بالا
                figure.Segments.Add(new LineSegment(new Point(top - r, 0), true));
                figure.Segments.Add(new ArcSegment(new Point(top, r), new Size(r, r), 0, false, SweepDirection.Clockwise, true));

                // پله
                figure.Segments.Add(new LineSegment(new Point(top, depth - r), true));
                figure.Segments.Add(new ArcSegment(new Point(top + r, depth), new Size(r, r), 0, false, SweepDirection.Counterclockwise, true));
                figure.Segments.Add(new LineSegment(new Point(w - r, depth), true));
                figure.Segments.Add(new ArcSegment(new Point(w, depth + r), new Size(r, r), 0, false, SweepDirection.Clockwise, true));

                // پایین راست
                figure.Segments.Add(new LineSegment(new Point(w, h - r), true));
                figure.Segments.Add(new ArcSegment(new Point(w - r, h), new Size(r, r), 0, false, SweepDirection.Clockwise, true));

                // پایین چپ
                figure.Segments.Add(new LineSegment(new Point(r, h), true));
                figure.Segments.Add(new ArcSegment(new Point(0, h - r), new Size(r, r), 0, false, SweepDirection.Clockwise, true));

                // بالا چپ
                figure.Segments.Add(new LineSegment(new Point(0, r), true));
                figure.Segments.Add(new ArcSegment(new Point(r, 0), new Size(r, r), 0, false, SweepDirection.Clockwise, true));
            }
            else
            {
                // پله سمت چپ (آینه‌ای)
                figure.StartPoint = new Point(1 - top + r, 0);

                figure.Segments.Add(new LineSegment(new Point(w - r, 0), true));
                figure.Segments.Add(new ArcSegment(new Point(w, r), new Size(r, r), 0, false, SweepDirection.Clockwise, true));

                figure.Segments.Add(new LineSegment(new Point(w, h - r), true));
                figure.Segments.Add(new ArcSegment(new Point(w - r, h), new Size(r, r), 0, false, SweepDirection.Clockwise, true));

                figure.Segments.Add(new LineSegment(new Point(r, h), true));
                figure.Segments.Add(new ArcSegment(new Point(0, h - r), new Size(r, r), 0, false, SweepDirection.Clockwise, true));

                figure.Segments.Add(new LineSegment(new Point(0, depth + r), true));
                figure.Segments.Add(new ArcSegment(new Point(r, depth), new Size(r, r), 0, false, SweepDirection.Clockwise, true));

                figure.Segments.Add(new LineSegment(new Point(1 - top - r, depth), true));
                figure.Segments.Add(new ArcSegment(new Point(1 - top, depth - r), new Size(r, r), 0, false, SweepDirection.Counterclockwise, true));

                figure.Segments.Add(new LineSegment(new Point(1 - top, r), true));
                figure.Segments.Add(new ArcSegment(new Point(1 - top + r, 0), new Size(r, r), 0, false, SweepDirection.Clockwise, true));
            }

            geometry.Figures.Add(figure);
            ShapePath.Data = geometry;

            double top1 = Math.Clamp(TopRatio, 0.3, 0.9);

            if (StepSide == StepSide.Right)
                StepStartX = top1 * ActualWidth;
            else
            {
                StepStartX = (1.0 - top1) * ActualWidth;
                StepStartX -= 5;
            }
                

        }
    }
}