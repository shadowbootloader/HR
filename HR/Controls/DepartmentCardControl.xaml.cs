using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace HR.Controls
{
    public partial class DepartmentCardControl : UserControl
    {

        public event RoutedEventHandler CardClick;


        public enum CircleColor
        {
            LawnGreen,Red, Blue
        }
        public DepartmentCardControl()
        {
            InitializeComponent();
            this.MouseLeftButtonUp += DepartmentCardControl_MouseLeftButtonUp;
            this.Cursor = System.Windows.Input.Cursors.Hand;

        }

        private void DepartmentCardControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // اگر روی دکمه داخلی کلیک شد و نمی‌خواهی حباب شود، همان‌جا e.Handled = true کن
            CardClick?.Invoke(this, new RoutedEventArgs());
        }

        public static readonly DependencyProperty CompletedEvaluationsProperty =
    DependencyProperty.Register(nameof(CompletedEvaluations), typeof(int), typeof(DepartmentCardControl), new PropertyMetadata(0));

        public int CompletedEvaluations
        {
            get => (int)GetValue(CompletedEvaluationsProperty);
            set => SetValue(CompletedEvaluationsProperty, value);
        }

        public static readonly DependencyProperty TotalEvaluationsProperty =
            DependencyProperty.Register(nameof(TotalEvaluations), typeof(int), typeof(DepartmentCardControl), new PropertyMetadata(0));

        public int TotalEvaluations
        {
            get => (int)GetValue(TotalEvaluationsProperty);
            set => SetValue(TotalEvaluationsProperty, value);
        }



        public static readonly DependencyProperty DepartmentNameProperty =
            DependencyProperty.Register(nameof(DepartmentName), typeof(string), typeof(DepartmentCardControl),
                new PropertyMetadata("دپارتمان"));

        public string DepartmentName
        {
            get => (string)GetValue(DepartmentNameProperty);
            set => SetValue(DepartmentNameProperty, value);
        }

        public static readonly DependencyProperty ManagerNameProperty =
            DependencyProperty.Register(nameof(ManagerName), typeof(string), typeof(DepartmentCardControl),
                new PropertyMetadata("—"));

        public string ManagerName
        {
            get => (string)GetValue(ManagerNameProperty);
            set => SetValue(ManagerNameProperty, value);
        }

        public static readonly DependencyProperty MembersCountProperty =
            DependencyProperty.Register(nameof(MembersCount), typeof(int), typeof(DepartmentCardControl),
                new PropertyMetadata(0));

        public int MembersCount
        {
            get => (int)GetValue(MembersCountProperty);
            set => SetValue(MembersCountProperty, value);
        }

        public static readonly DependencyProperty OpenProjectsProperty =
            DependencyProperty.Register(nameof(OpenProjects), typeof(int), typeof(DepartmentCardControl),
                new PropertyMetadata(0));

        public int OpenProjects
        {
            get => (int)GetValue(OpenProjectsProperty);
            set => SetValue(OpenProjectsProperty, value);
        }

        public static readonly DependencyProperty PerformanceValueProperty =
            DependencyProperty.Register(nameof(PerformanceValue), typeof(double), typeof(DepartmentCardControl),
                new PropertyMetadata(0.0));

        public double PerformanceValue
        {
            get => (double)GetValue(PerformanceValueProperty);
            set => SetValue(PerformanceValueProperty, value);
        }

        public static readonly DependencyProperty EvaluationProgressProperty =
            DependencyProperty.Register(nameof(EvaluationProgress), typeof(double), typeof(DepartmentCardControl),
                new PropertyMetadata(0.0));

        public double EvaluationProgress
        {
            get => (double)GetValue(EvaluationProgressProperty);
            set => SetValue(EvaluationProgressProperty, value);
        }

        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register(nameof(StatusText), typeof(string), typeof(DepartmentCardControl),
                new PropertyMetadata("پایدار"));

        public string StatusText
        {
            get => (string)GetValue(StatusTextProperty);
            set => SetValue(StatusTextProperty, value);
        }

        public static readonly DependencyProperty StatusBackgroundProperty =
            DependencyProperty.Register(nameof(StatusBackground), typeof(Brush), typeof(DepartmentCardControl),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#14532D"))));

        public Brush StatusBackground
        {
            get => (Brush)GetValue(StatusBackgroundProperty);
            set => SetValue(StatusBackgroundProperty, value);
        }

        public static readonly DependencyProperty StatusForegroundProperty =
            DependencyProperty.Register(nameof(StatusForeground), typeof(Brush), typeof(DepartmentCardControl),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#86EFAC"))));


        public static readonly DependencyProperty StatusCircleProperty =
    DependencyProperty.Register(
        nameof(StatusCircle),
        typeof(CircleColor),
        typeof(DepartmentCardControl),
        new PropertyMetadata(CircleColor.LawnGreen, OnStatusCircleChanged));

        public CircleColor StatusCircle
        {
            get => (CircleColor)GetValue(StatusCircleProperty);
            set => SetValue(StatusCircleProperty, value);
        }

        private static void OnStatusCircleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DepartmentCardControl;
            if (control?.LiteCircle == null) return;

            ApplyCircleStyle(control, (CircleColor)e.NewValue);
        }

        private static void ApplyCircleStyle(DepartmentCardControl control, CircleColor color)
        {
            switch (color)
            {
                case CircleColor.LawnGreen:
                    control.LiteCircle.Style = (Style)control.FindResource("LawnGreenNeonIcon");
                    break;
                case CircleColor.Red:
                    control.LiteCircle.Style = (Style)control.FindResource("RedNeonIcon");
                    break;
                case CircleColor.Blue:
                    control.LiteCircle.Style = (Style)control.FindResource("BlueNeonIcon");
                    break;
            }
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            // برای اطمینان در زمان لود هم اعمال کن
            ApplyCircleStyle(this, StatusCircle);
        }

        public Brush StatusForeground
        {
            get => (Brush)GetValue(StatusForegroundProperty);
            set => SetValue(StatusForegroundProperty, value);
        }

        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.Register(nameof(IconKind), typeof(PackIconKind), typeof(DepartmentCardControl),
                new PropertyMetadata(PackIconKind.OfficeBuilding));

        public PackIconKind IconKind
        {
            get => (PackIconKind)GetValue(IconKindProperty);
            set => SetValue(IconKindProperty, value);
        }

        public static readonly DependencyProperty ProjectsCountProperty =
    DependencyProperty.Register(nameof(ProjectsCount), typeof(int), typeof(DepartmentCardControl), new PropertyMetadata(0));
        public int ProjectsCount { get => (int)GetValue(ProjectsCountProperty); set => SetValue(ProjectsCountProperty, value); }

        public static readonly DependencyProperty ProjectProgressProperty =
            DependencyProperty.Register(nameof(ProjectProgress), typeof(double), typeof(DepartmentCardControl), new PropertyMetadata(0.0));
        public double ProjectProgress { get => (double)GetValue(ProjectProgressProperty); set => SetValue(ProjectProgressProperty, value); }

        
    }
}