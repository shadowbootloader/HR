// AssessmentTestRowControl.xaml.cs
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace HR.Controls.Assessment
{
    public partial class AssessmentTestRow : UserControl
    {

        // وضعیت
        const string CompletedBg1 = "#062F28";
        const string CompletedBg2 = "#0A4A3A";
        const string CompletedFg = "#34D399";   // متن + آیکون
        const string CompletedBorder = "#00FF00";

        const string ProgressBg1 = "#0A2A3D";
        const string ProgressBg2 = "#0C4A6E";
        const string ProgressFg = "#22D3EE";
        const string ProgressBorder = "#FF005689";

        const string OutlierBg1 = "#333B0A16";
        const string OutlierBg2 = "#335C0F24";
        const string OutlierFg = "#FB7185";
        const string OutlierBorder = "#E11D48";

        const string LockedBg1 = "#151C28";
        const string LockedBg2 = "#1E293B";
        const string LockedFg = "#94A3B8";
        const string LockedBorder = "#B2BEB5";

        // دسته
        const string CatPsychology = "#A78BFA";
        const string CatJob = "#38BDF8";
        const string CatSkill = "#22D3EE";
        const string CatDiscipline = "#2DD4BF";
        const string CatSituational = "#60A5FA";
        const string CatAbility = "#22D3EE";


        public AssessmentTestRow()
        {
            InitializeComponent();
            Loaded += (_, __) => RefreshVisuals();
            SizeChanged += (_, __) => UpdateProgressWidth();
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty DefinitionIdProperty =
            DependencyProperty.Register(nameof(DefinitionId), typeof(string),
                typeof(AssessmentTestRow), new PropertyMetadata(""));

        public string DefinitionId
        {
            get => (string)GetValue(DefinitionIdProperty);
            set => SetValue(DefinitionIdProperty, value);
        }

        // ── رویداد کلیک ردیف ────────────────────────────────────
        public static readonly RoutedEvent RowClickEvent =
            EventManager.RegisterRoutedEvent(
                nameof(RowClick), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(AssessmentTestRow));

        public event RoutedEventHandler RowClick
        {
            add => AddHandler(RowClickEvent, value);
            remove => RemoveHandler(RowClickEvent, value);
        }

        private void OnRowClick(object sender, MouseButtonEventArgs e)
            => RaiseEvent(new RoutedEventArgs(RowClickEvent, this));

        // ── Dependency Properties ───────────────────────────────
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string),
                typeof(AssessmentTestRow), new PropertyMetadata(""));
        //public string Title
        //{
        //    get => (string)GetValue(TitleProperty);
        //    set => SetValue(TitleProperty, value);
        //}

        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register(nameof(Subtitle), typeof(string),
                typeof(AssessmentTestRow), new PropertyMetadata(""));
        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        /// <summary>completed | in-progress | outlier | locked</summary>
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(nameof(Status), typeof(string),
                typeof(AssessmentTestRow),
                new PropertyMetadata("completed", (_, e) =>
                {
                    if (e.NewValue != null && e.Property.OwnerType == typeof(AssessmentTestRow))
                    { }
                }));
        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public static readonly DependencyProperty CategoryTextProperty =
            DependencyProperty.Register(nameof(CategoryText), typeof(string),
                typeof(AssessmentTestRow), new PropertyMetadata(""));
        public string CategoryText
        {
            get => (string)GetValue(CategoryTextProperty);
            set => SetValue(CategoryTextProperty, value);
        }

        public static readonly DependencyProperty CategoryIconProperty =
            DependencyProperty.Register(nameof(CategoryIcon), typeof(PackIconKind),
                typeof(AssessmentTestRow), new PropertyMetadata(PackIconKind.Brain));
        public PackIconKind CategoryIcon
        {
            get => (PackIconKind)GetValue(CategoryIconProperty);
            set => SetValue(CategoryIconProperty, value);
        }

        public static readonly DependencyProperty CategoryBrushProperty =
            DependencyProperty.Register(nameof(CategoryBrush), typeof(Brush),
                typeof(AssessmentTestRow),
                new PropertyMetadata(Brush("#A78BFA")));
        public Brush CategoryBrush
        {
            get => (Brush)GetValue(CategoryBrushProperty);
            set => SetValue(CategoryBrushProperty, value);
        }

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(int),
                typeof(AssessmentTestRow),
                new PropertyMetadata(0, (d, _) => ((AssessmentTestRow)d).RefreshVisuals()));
        public int Progress
        {
            get => (int)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        public static readonly DependencyProperty LastDateProperty =
            DependencyProperty.Register(nameof(LastDate), typeof(string),
                typeof(AssessmentTestRow), new PropertyMetadata(""));
        public string LastDate
        {
            get => (string)GetValue(LastDateProperty);
            set => SetValue(LastDateProperty, value);
        }

        public static readonly DependencyProperty OutlierCountProperty =
            DependencyProperty.Register(nameof(OutlierCount), typeof(int),
                typeof(AssessmentTestRow),
                new PropertyMetadata(0, (d, _) => ((AssessmentTestRow)d).RefreshVisuals()));
        public int OutlierCount
        {
            get => (int)GetValue(OutlierCountProperty);
            set => SetValue(OutlierCountProperty, value);
        }
        // وقتی Status عوض شد
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == StatusProperty || e.Property == ProgressProperty || e.Property == OutlierCountProperty)
                RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            if (StatusBd == null) return;

            switch ((Status ?? "completed").ToLowerInvariant())
            {
                case "in-progress":
                    SetStatus("در حال انجام", PackIconKind.PlayCircle, ProgressBg1, ProgressBg2, ProgressFg , ProgressBorder);
                    SetProgressBar("#6EE7B7");
                    TrailIco.Kind = PackIconKind.ChevronLeft;
                    TrailIco.Foreground = Brush("#64748B");
                    break;
                case "outlier":
                    SetStatus("دارای داده پرت", PackIconKind.Alert, OutlierBg1, OutlierBg2, OutlierFg, OutlierBorder);
                    SetProgressBar(ProgressFg);
                    TrailIco.Kind = PackIconKind.ChevronLeft;
                    TrailIco.Foreground = Brush("#64748B");
                    break;
                case "locked":
                    SetStatus("قفل شده", PackIconKind.Lock, LockedBg1, LockedBg2, LockedFg, LockedBorder);
                    SetProgressBar("#334155");
                    TrailIco.Kind = PackIconKind.Lock;
                    TrailIco.Foreground = Brush("#64748B");
                    break;
                default:
                    SetStatus("تکمیل شده", PackIconKind.CheckboxMarkedCircle, CompletedBg1, CompletedBg2, CompletedFg, CompletedBorder);
                    SetProgressBar(CompletedFg);
                    TrailIco.Kind = PackIconKind.ChevronLeft;
                    TrailIco.Foreground = Brush("#64748B");
                    break;
            }

            if (OutlierCount > 0)
            {
                OutlierBadge.Visibility = Visibility.Visible;
                OutlierTxt.Text = OutlierCount.ToString();
            }
            else
            {
                OutlierBadge.Visibility = Visibility.Collapsed;
            }

            UpdateProgressWidth();
        }

        //private void SetStatus(string text, PackIconKind icon, string bg, string fg)
        //{
        //    StatusLbl.Text = text;
        //    StatusIco.Kind = icon;
        //    StatusBd.Background = Brush(bg);
        //    var f = Brush(fg);
        //    StatusLbl.Foreground = f;
        //    StatusIco.Foreground = f;
        //}

        //private void SetProgressBar(string colorHex)
        //{
        //    var b = Brush(colorHex);
        //    ProgFill.Background = b;
        //    ProgFill.Effect = new System.Windows.Media.Effects.DropShadowEffect
        //    {
        //        BlurRadius = 6,
        //        ShadowDepth = 0,
        //        Color = ((SolidColorBrush)b).Color,
        //        Opacity = 0.45
        //    };
        //}

        private void UpdateProgressWidth()
        {
            if (ProgTrack == null || ProgFill == null) return;
            var ratio = Math.Max(0, Math.Min(1.0, Progress / 100.0));
            var w = ProgTrack.ActualWidth;
            if (w <= 0) return;
            ProgFill.Width = w * ratio;
        }

        private LinearGradientBrush NeonBg(string c1, string c2)
        {
            var g = new LinearGradientBrush(
                (Color)ColorConverter.ConvertFromString(c1),
                (Color)ColorConverter.ConvertFromString(c2),
                new Point(0, 0.5), new Point(1, 0.5));
            g.Freeze();
            return g;
        }

        private void SetProgressBar(string colorHex , string SecondHex = "#67E8F9")
        {
            var b = Brush(colorHex);
            ProgFill.Background = new LinearGradientBrush(
                (Color)ColorConverter.ConvertFromString(colorHex),
                (Color)ColorConverter.ConvertFromString(SecondHex),
                new Point(0, 0.5), new Point(1, 0.5));

            ProgFill.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 8,
                ShadowDepth = 0,
                Color = ((SolidColorBrush)b).Color,
                Opacity = 0.55
            };
        }

        private void SetStatus(string text, PackIconKind icon, string bg1, string bg2, string fg,string Border)
        {
            StatusLbl.Text = text;
            StatusIco.Kind = icon;

            StatusBd.Background = NeonBg(bg1, bg2);

            var f = Brush(fg);
            StatusLbl.Foreground = f;
            StatusIco.Foreground = f;

            // درخشش نئون دور badge
            StatusBd.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 12,
                ShadowDepth = 0,
                Color = ((SolidColorBrush)f).Color,
                Opacity = 0.45
            };

            // برای داده پرت حاشیه قرمز نئون
            StatusBd.BorderThickness = new Thickness(1.2);

            StatusBd.BorderBrush = Brush(Border);
        }


        private static SolidColorBrush Brush(string hex)
        {
            var b = (SolidColorBrush)new BrushConverter().ConvertFrom(hex);
            b.Freeze();
            return b;
        }
    }
}