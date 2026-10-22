// AssessmentSidePanelControl.xaml.cs
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace HR.Controls.Assessment
{
    public class SideMedalItem
    {
        public string Title { get; set; }
        public int Points { get; set; }
        public PackIconKind IconKind { get; set; }
        public Brush ToneBrush { get; set; }
    }

    public class SideActivityItem
    {
        public string Time { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public PackIconKind IconKind { get; set; }
        public Brush IconBrush { get; set; }
        public Visibility DetailVisibility =>
            string.IsNullOrWhiteSpace(Detail) ? Visibility.Collapsed : Visibility.Visible;
    }

    public partial class AssessmentSidePanelControl : UserControl
    {
        public AssessmentSidePanelControl()
        {
            InitializeComponent();
            Loaded += (_, __) => UpdateBars();
            SizeChanged += (_, __) => UpdateBars();
        }

        public static readonly DependencyProperty MedalsProperty =
            DependencyProperty.Register(nameof(Medals), typeof(ObservableCollection<SideMedalItem>),
                typeof(AssessmentSidePanelControl));
        public ObservableCollection<SideMedalItem> Medals
        {
            get => (ObservableCollection<SideMedalItem>)GetValue(MedalsProperty);
            set => SetValue(MedalsProperty, value);
        }

        public static readonly DependencyProperty ActivitiesProperty =
            DependencyProperty.Register(nameof(Activities), typeof(ObservableCollection<SideActivityItem>),
                typeof(AssessmentSidePanelControl));
        public ObservableCollection<SideActivityItem> Activities
        {
            get => (ObservableCollection<SideActivityItem>)GetValue(ActivitiesProperty);
            set => SetValue(ActivitiesProperty, value);
        }

        public static readonly DependencyProperty XpProperty =
            DependencyProperty.Register(nameof(Xp), typeof(int), typeof(AssessmentSidePanelControl),
                new PropertyMetadata(750, (_, __) => { }));
        public int Xp
        {
            get => (int)GetValue(XpProperty);
            set => SetValue(XpProperty, value);
        }

        public static readonly DependencyProperty XpMaxProperty =
            DependencyProperty.Register(nameof(XpMax), typeof(int), typeof(AssessmentSidePanelControl),
                new PropertyMetadata(1000));
        public int XpMax
        {
            get => (int)GetValue(XpMaxProperty);
            set => SetValue(XpMaxProperty, value);
        }

        public static readonly DependencyProperty NextMedalRemainProperty =
            DependencyProperty.Register(nameof(NextMedalRemain), typeof(int), typeof(AssessmentSidePanelControl),
                new PropertyMetadata(250));
        public int NextMedalRemain
        {
            get => (int)GetValue(NextMedalRemainProperty);
            set => SetValue(NextMedalRemainProperty, value);
        }

        private void OnAssignMedalClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    => AssignMedal?.Invoke(this, new RoutedEventArgs());


        public static readonly DependencyProperty NextGoalTitleProperty =
            DependencyProperty.Register(nameof(NextGoalTitle), typeof(string), typeof(AssessmentSidePanelControl),
                new PropertyMetadata("تحلیل‌گر داده"));
        public string NextGoalTitle
        {
            get => (string)GetValue(NextGoalTitleProperty);
            set => SetValue(NextGoalTitleProperty, value);
        }

        public static readonly DependencyProperty NextGoalDescProperty =
            DependencyProperty.Register(nameof(NextGoalDesc), typeof(string), typeof(AssessmentSidePanelControl),
                new PropertyMetadata("تکمیل ۴ آزمون تخصصی با نمره بالای ۸۰"));
        public string NextGoalDesc
        {
            get => (string)GetValue(NextGoalDescProperty);
            set => SetValue(NextGoalDescProperty, value);
        }

        public static readonly DependencyProperty NextGoalPointsProperty =
            DependencyProperty.Register(nameof(NextGoalPoints), typeof(int), typeof(AssessmentSidePanelControl),
                new PropertyMetadata(250));
        public int NextGoalPoints
        {
            get => (int)GetValue(NextGoalPointsProperty);
            set => SetValue(NextGoalPointsProperty, value);
        }

        public event RoutedEventHandler ViewAllMedals;
        public event RoutedEventHandler AssignMedal;
        public event RoutedEventHandler GoalDetails;
        public event RoutedEventHandler ViewAllActivities;

        private void OnViewAllMedals(object s, RoutedEventArgs e) => ViewAllMedals?.Invoke(this, e);
        private void OnAssignMedal(object s, RoutedEventArgs e) => AssignMedal?.Invoke(this, e);
        private void OnGoalDetails(object s, RoutedEventArgs e) => GoalDetails?.Invoke(this, e);
        private void OnViewAllActivities(object s, RoutedEventArgs e) => ViewAllActivities?.Invoke(this, e);

        private void UpdateBars()
        {
            //if (XpBar == null) return;
            //var parent = XpBar.Parent as FrameworkElement;
            //if (parent == null || parent.ActualWidth <= 0) return;
            //var max = XpMax <= 0 ? 1 : XpMax;
            //var ratio = Math.Max(0, Math.Min(1.0, (double)Xp / max));
            //XpBar.Width = parent.ActualWidth * ratio;

            //خط مسیر: حدوداً تا مرحله نقره‌ای(۵۰٪)
            //if (PathFill != null && PathFill.Parent is FrameworkElement track)
            //    PathFill.Width = track.ActualWidth * 0.45;
        }



        public event RoutedEventHandler StartRecommendedTest;

        private void OnStartRecommendedTest(object sender, RoutedEventArgs e)
            => StartRecommendedTest?.Invoke(this, e);

        // DependencyPropertyهای توصیه
        public static readonly DependencyProperty RecTitleProperty =
            DependencyProperty.Register(nameof(RecTitle), typeof(string), typeof(AssessmentSidePanelControl),
                new PropertyMetadata("ادامه ارزیابی مدیر", OnRecChanged));
        public string RecTitle
        {
            get => (string)GetValue(RecTitleProperty);
            set => SetValue(RecTitleProperty, value);
        }

        public static readonly DependencyProperty RecDescProperty =
            DependencyProperty.Register(nameof(RecDesc), typeof(string), typeof(AssessmentSidePanelControl),
                new PropertyMetadata("۶۰٪ پیشرفت دارد؛ تکمیل آن مسیر تست‌های بعدی را باز می‌کند.", OnRecChanged));
        public string RecDesc
        {
            get => (string)GetValue(RecDescProperty);
            set => SetValue(RecDescProperty, value);
        }

        public static readonly DependencyProperty RecBadgeProperty =
            DependencyProperty.Register(nameof(RecBadge), typeof(string), typeof(AssessmentSidePanelControl),
                new PropertyMetadata("اولویت", OnRecChanged));
        public string RecBadge
        {
            get => (string)GetValue(RecBadgeProperty);
            set => SetValue(RecBadgeProperty, value);
        }

        public static readonly DependencyProperty RecButtonTextProperty =
            DependencyProperty.Register(nameof(RecButtonText), typeof(string), typeof(AssessmentSidePanelControl),
                new PropertyMetadata("ادامه آزمون", OnRecChanged));
        public string RecButtonText
        {
            get => (string)GetValue(RecButtonTextProperty);
            set => SetValue(RecButtonTextProperty, value);
        }

        private static void OnRecChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AssessmentSidePanelControl c) c.ApplyRec();
        }

        private void ApplyRec()
        {
            if (TxtRecTitle == null) return;
            TxtRecTitle.Text = RecTitle;
            TxtRecDesc.Text = RecDesc;
            TxtRecBadge.Text = RecBadge;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == XpProperty || e.Property == XpMaxProperty)
                UpdateBars();
        }
    }
}