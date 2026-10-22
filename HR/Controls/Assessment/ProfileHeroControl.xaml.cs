// ProfileHeroControl.xaml.cs
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace HR.Controls.Assessment
{
    public class MedalItem
    {
        public string Title { get; set; }
        public int Points { get; set; }
        public string Description { get; set; }
        public Brush ToneBrush { get; set; }
        public PackIconKind IconKind { get; set; } = PackIconKind.Medal;
    }

    public partial class ProfileHeroControl : UserControl
    {
        public ProfileHeroControl()
        {
            InitializeComponent();
            Loaded += (_, __) => UpdateXpBar();
        }

        // ─── RoutedEvent برای مدال بعدی ─────────────────────────
        public static readonly RoutedEvent NextMedalClickEvent =
            EventManager.RegisterRoutedEvent(
                nameof(NextMedalClick),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(ProfileHeroControl));

        public event RoutedEventHandler NextMedalClick
        {
            add => AddHandler(NextMedalClickEvent, value);
            remove => RemoveHandler(NextMedalClickEvent, value);
        }

        private void OnNextMedalButtonClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            RaiseEvent(new RoutedEventArgs(NextMedalClickEvent, this));
        }

        // ─── Dependency Properties ──────────────────────────────
        public static readonly DependencyProperty EmployeeNameProperty =
            DependencyProperty.Register(nameof(EmployeeName), typeof(string), typeof(ProfileHeroControl),
                new PropertyMetadata("علی فاطمی"));
        public string EmployeeName
        {
            get => (string)GetValue(EmployeeNameProperty);
            set => SetValue(EmployeeNameProperty, value);
        }

        public static readonly DependencyProperty RoleProperty =
            DependencyProperty.Register(nameof(Role), typeof(string), typeof(ProfileHeroControl),
                new PropertyMetadata("کارشناس تحلیل داده"));
        public string Role
        {
            get => (string)GetValue(RoleProperty);
            set => SetValue(RoleProperty, value);
        }

        public static readonly DependencyProperty DepartmentProperty =
            DependencyProperty.Register(nameof(Department), typeof(string), typeof(ProfileHeroControl),
                new PropertyMetadata("واحد: فناوری اطلاعات"));
        public string Department
        {
            get => (string)GetValue(DepartmentProperty);
            set => SetValue(DepartmentProperty, value);
        }

        public static readonly DependencyProperty LevelProperty =
            DependencyProperty.Register(nameof(Level), typeof(int), typeof(ProfileHeroControl),
                new PropertyMetadata(3));
        public int Level
        {
            get => (int)GetValue(LevelProperty);
            set => SetValue(LevelProperty, value);
        }

        public static readonly DependencyProperty XpProperty =
            DependencyProperty.Register(nameof(Xp), typeof(int), typeof(ProfileHeroControl),
                new PropertyMetadata(750, OnXpChanged));
        public int Xp
        {
            get => (int)GetValue(XpProperty);
            set => SetValue(XpProperty, value);
        }

        public static readonly DependencyProperty XpMaxProperty =
            DependencyProperty.Register(nameof(XpMax), typeof(int), typeof(ProfileHeroControl),
                new PropertyMetadata(1000, OnXpChanged));
        public int XpMax
        {
            get => (int)GetValue(XpMaxProperty);
            set => SetValue(XpMaxProperty, value);
        }

        public static readonly DependencyProperty NextMedalNameProperty =
            DependencyProperty.Register(nameof(NextMedalName), typeof(string), typeof(ProfileHeroControl),
                new PropertyMetadata("تحلیل‌گر داده"));
        public string NextMedalName
        {
            get => (string)GetValue(NextMedalNameProperty);
            set => SetValue(NextMedalNameProperty, value);
        }

        public static readonly DependencyProperty NextMedalRemainProperty =
            DependencyProperty.Register(nameof(NextMedalRemain), typeof(int), typeof(ProfileHeroControl),
                new PropertyMetadata(250));
        public int NextMedalRemain
        {
            get => (int)GetValue(NextMedalRemainProperty);
            set => SetValue(NextMedalRemainProperty, value);
        }

        public static readonly DependencyProperty AvatarSourceProperty =
            DependencyProperty.Register(nameof(AvatarSource), typeof(ImageSource), typeof(ProfileHeroControl),
                new PropertyMetadata(null));
        public ImageSource AvatarSource
        {
            get => (ImageSource)GetValue(AvatarSourceProperty);
            set => SetValue(AvatarSourceProperty, value);
        }

        public static readonly DependencyProperty MedalsProperty =
            DependencyProperty.Register(nameof(Medals), typeof(ObservableCollection<MedalItem>),
                typeof(ProfileHeroControl), new PropertyMetadata(null));
        public ObservableCollection<MedalItem> Medals
        {
            get => (ObservableCollection<MedalItem>)GetValue(MedalsProperty);
            set => SetValue(MedalsProperty, value);
        }

        // ─── نوار XP ────────────────────────────────────────────
        private static void OnXpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProfileHeroControl c) c.UpdateXpBar();
        }

        private void UpdateXpBar()
        {
            if (XpFill == null || XpTrack == null) return;
            var max = XpMax <= 0 ? 1 : XpMax;
            var ratio = Math.Max(0, Math.Min(1.0, (double)Xp / max));
            XpFill.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (XpTrack.ActualWidth > 0)
                    XpFill.Width = XpTrack.ActualWidth * ratio;
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateXpBar();
        }
    }
}