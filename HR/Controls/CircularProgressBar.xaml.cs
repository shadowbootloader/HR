using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HR.Controls
{
    public partial class CircularProgressBar : UserControl
    {
        #region Dependency Properties

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(double),
                typeof(CircularProgressBar),
                new PropertyMetadata(0.0, OnValueChanged));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                nameof(Maximum),
                typeof(double),
                typeof(CircularProgressBar),
                new PropertyMetadata(100.0, OnValueChanged));

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
                nameof(StrokeThickness),
                typeof(double),
                typeof(CircularProgressBar),
                new PropertyMetadata(16.0, OnValueChanged));

        public static readonly DependencyProperty TrackColorProperty =
            DependencyProperty.Register(
                nameof(TrackColor),
                typeof(Brush),
                typeof(CircularProgressBar),
                new PropertyMetadata(
                    new SolidColorBrush(Color.FromRgb(0xD0, 0xD0, 0xD0)),
                    OnValueChanged));

        #endregion


        #region Properties

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public Brush TrackColor
        {
            get => (Brush)GetValue(TrackColorProperty);
            set => SetValue(TrackColorProperty, value);
        }

        /*
         * این property عمداً DependencyProperty نیست.
         *
         * دلیل:
         * XAML به شکل زیر collection را مستقیماً پر می‌کند:
         *
         * <CircularProgressBar.GradientStops>
         *     <GradientStop .../>
         * </CircularProgressBar.GradientStops>
         *
         * بنابراین collection باید از ابتدا وجود داشته باشد.
         */
        public GradientStopCollection GradientStops { get; }
            = new GradientStopCollection();

        public string PercentageText
        {
            get
            {
                if (Maximum <= 0)
                    return "0%";

                double percentage = (Value / Maximum) * 100.0;

                percentage = Math.Clamp(percentage, 0, 100);

                return $"{Math.Round(percentage)}%";
            }
        }

        #endregion


        #region Constructor

        public CircularProgressBar()
        {
            InitializeComponent();

            Loaded += CircularProgressBar_Loaded;
            SizeChanged += CircularProgressBar_SizeChanged;

            GradientStops.Changed += GradientStops_Changed;
        }

        #endregion


        #region Events

        private void CircularProgressBar_Loaded(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void CircularProgressBar_SizeChanged(
            object sender,
            SizeChangedEventArgs e)
        {
            Update();
        }

        private void GradientStops_Changed(object? sender, EventArgs e)
        {
            Update();
        }

        #endregion


        #region Dependency Property Changed

        private static void OnValueChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is CircularProgressBar bar)
            {
                bar.Update();

                bar.OnPropertyChanged(nameof(PercentageText));
            }
        }

        #endregion


        #region Update

        private void Update()
        {
            if (!IsLoaded)
                return;

            if (ActualWidth <= 0 || ActualHeight <= 0)
                return;

            if (progressCanvas == null || trackPath == null)
                return;

            double size = Math.Min(ActualWidth, ActualHeight);

            double thickness = Math.Max(0, StrokeThickness);

            double radius = (size - thickness) / 2.0;

            if (radius <= 1)
                return;

            double cx = ActualWidth / 2.0;
            double cy = ActualHeight / 2.0;


            // =========================================================
            // Track - دایره کامل
            // =========================================================

            double trackStartAngle = -90.0;
            double trackEndAngle = 269.999;

            var trackGeometry = new PathGeometry();

            var trackFigure = new PathFigure
            {
                StartPoint = GetPoint(
                    cx,
                    cy,
                    radius,
                    trackStartAngle),

                IsClosed = false,
                IsFilled = false
            };

            trackFigure.Segments.Add(
                new ArcSegment
                {
                    Point = GetPoint(
                        cx,
                        cy,
                        radius,
                        trackEndAngle),

                    Size = new Size(radius, radius),

                    IsLargeArc = true,

                    SweepDirection = SweepDirection.Clockwise
                });

            trackGeometry.Figures.Add(trackFigure);

            trackPath.Data = trackGeometry;


            // =========================================================
            // Progress
            // =========================================================

            progressCanvas.Children.Clear();

            if (Maximum <= 0)
                return;

            double normalizedValue = Value / Maximum;

            normalizedValue = Math.Clamp(
                normalizedValue,
                0.0,
                1.0);

            if (normalizedValue <= 0)
                return;

            double progressAngle = normalizedValue * 360.0;


            // برای جلوگیری از مشکل ArcSegment در 360 درجه
            if (progressAngle >= 360.0)
                progressAngle = 359.999;


            // هر Segment تقریباً 3 درجه
            int segments = Math.Max(
                16,
                (int)Math.Ceiling(progressAngle / 3.0));


            for (int i = 0; i < segments; i++)
            {
                double startAngle =
                    -90.0 +
                    (progressAngle * i / segments);

                double endAngle =
                    -90.0 +
                    (progressAngle * (i + 1) / segments);


                // موقعیت گرادینت
                double t;

                if (segments <= 1)
                {
                    t = 0;
                }
                else
                {
                    t = (double)i / (segments - 1);
                }


                Color color = GetColorAt(t);


                // =====================================================
                // Segment
                // =====================================================

                var figure = new PathFigure
                {
                    StartPoint = GetPoint(
                        cx,
                        cy,
                        radius,
                        startAngle),

                    IsClosed = false,
                    IsFilled = false
                };


                figure.Segments.Add(
                    new ArcSegment
                    {
                        Point = GetPoint(
                            cx,
                            cy,
                            radius,
                            endAngle),

                        Size = new Size(radius, radius),

                        IsLargeArc =
                            (endAngle - startAngle) > 180,

                        SweepDirection =
                            SweepDirection.Clockwise
                    });


                var geometry = new PathGeometry();

                geometry.Figures.Add(figure);


                var segmentPath = new Path
                {
                    Data = geometry,

                    Stroke =
                        new SolidColorBrush(color),

                    StrokeThickness =
                        thickness,

                    StrokeStartLineCap =
                        PenLineCap.Round,

                    StrokeEndLineCap =
                        PenLineCap.Round,

                    Fill = Brushes.Transparent,

                    IsHitTestVisible = false
                };


                progressCanvas.Children.Add(segmentPath);
            }
        }

        #endregion


        #region Geometry

        private static Point GetPoint(
            double cx,
            double cy,
            double radius,
            double angleDegrees)
        {
            double radians =
                angleDegrees * Math.PI / 180.0;

            return new Point(
                cx + radius * Math.Cos(radians),
                cy + radius * Math.Sin(radians));
        }

        #endregion


        #region Gradient

        private Color GetColorAt(double t)
        {
            if (GradientStops == null ||
                GradientStops.Count == 0)
            {
                return Colors.Gray;
            }

            if (GradientStops.Count == 1)
            {
                return GradientStops[0].Color;
            }


            t = Math.Clamp(t, 0.0, 1.0);


            GradientStop left =
                GradientStops[0];

            GradientStop right =
                GradientStops[GradientStops.Count - 1];


            // پیدا کردن دو GradientStop
            // که t بین آنها قرار دارد

            for (int i = 0;
                 i < GradientStops.Count - 1;
                 i++)
            {
                GradientStop current =
                    GradientStops[i];

                GradientStop next =
                    GradientStops[i + 1];


                if (t >= current.Offset &&
                    t <= next.Offset)
                {
                    left = current;
                    right = next;

                    break;
                }
            }


            // قبل از اولین Stop
            if (t <= GradientStops[0].Offset)
            {
                return GradientStops[0].Color;
            }


            // بعد از آخرین Stop
            GradientStop last =
                GradientStops[GradientStops.Count - 1];

            if (t >= last.Offset)
            {
                return last.Color;
            }


            double range =
                right.Offset - left.Offset;


            double localT;

            if (Math.Abs(range) < double.Epsilon)
            {
                localT = 0;
            }
            else
            {
                localT =
                    (t - left.Offset) / range;
            }


            return LerpColor(
                left.Color,
                right.Color,
                localT);
        }


        private static Color LerpColor(
            Color a,
            Color b,
            double t)
        {
            t = Math.Clamp(t, 0.0, 1.0);


            byte alpha =
                (byte)(a.A +
                       (b.A - a.A) * t);

            byte red =
                (byte)(a.R +
                       (b.R - a.R) * t);

            byte green =
                (byte)(a.G +
                       (b.G - a.G) * t);

            byte blue =
                (byte)(a.B +
                       (b.B - a.B) * t);


            return Color.FromArgb(
                alpha,
                red,
                green,
                blue);
        }

        #endregion


        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;


        protected virtual void OnPropertyChanged(
            string propertyName)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}