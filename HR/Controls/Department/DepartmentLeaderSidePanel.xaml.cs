using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HR.Models;
using MaterialDesignThemes.Wpf;

namespace HR.Controls.Department
{
    public partial class DepartmentLeaderSidePanel : UserControl
    {
        public ObservableCollection<DeptActivityItem> Activities { get; set; } = new();

        public DepartmentLeaderSidePanel()
        {
            InitializeComponent();

            if (ActivityItems != null)
                ActivityItems.ItemsSource = Activities;

            Activities.Clear();
            Activities.Add(new DeptActivityItem
            {
                Title = "جلسه هماهنگی پروژه جذب نیرو",
                Subtitle = "با حضور مدیر دپارتمان و تیم فنی",
                Date = "۱۴۰۴/۰۶/۲۰",
                Time = "۱۱:۴۵",
                IconKind = PackIconKind.Calendar
            });
            Activities.Add(new DeptActivityItem
            {
                Title = "امضای قرارداد همکاری با شرکت آتیه",
                Subtitle = "قرارداد ۲ ساله - بخش آموزش",
                Date = "۱۴۰۴/۰۶/۱۹",
                Time = "۱۵:۳۰",
                IconKind = PackIconKind.FileDocumentOutline
            });
            Activities.Add(new DeptActivityItem
            {
                Title = "برگزاری جلسه ارزیابی عملکرد",
                Subtitle = "با حضور کلیه سرپرستان",
                Date = "۱۴۰۴/۰۶/۱۸",
                Time = "۱۰:۲۰",
                IconKind = PackIconKind.AccountGroupOutline
            });
            

            ActivityItems.ItemsSource = Activities;
        }

        public static readonly DependencyProperty ManagerNameProperty =
            DependencyProperty.Register(nameof(ManagerName), typeof(string), typeof(DepartmentLeaderSidePanel),
                new PropertyMetadata(""));

        public string ManagerName
        {
            get => (string)GetValue(ManagerNameProperty);
            set => SetValue(ManagerNameProperty, value);
        }

        public static readonly DependencyProperty ManagerTitleProperty =
            DependencyProperty.Register(nameof(ManagerTitle), typeof(string), typeof(DepartmentLeaderSidePanel),
                new PropertyMetadata("مدیر دپارتمان"));

        public string ManagerTitle
        {
            get => (string)GetValue(ManagerTitleProperty);
            set => SetValue(ManagerTitleProperty, value);
        }

        public static readonly DependencyProperty PettyCashNameProperty =
    DependencyProperty.Register(nameof(PettyCashName), typeof(string), typeof(DepartmentLeaderSidePanel),
        new PropertyMetadata(""));

        public string PettyCashName
        {
            get => (string)GetValue(PettyCashNameProperty);
            set => SetValue(PettyCashNameProperty, value);
        }

        public static readonly DependencyProperty PettyCashPhotoProperty =
            DependencyProperty.Register(nameof(PettyCashPhoto), typeof(ImageSource), typeof(DepartmentLeaderSidePanel),
                new PropertyMetadata(null, OnPettyCashPhotoChanged));

        public ImageSource PettyCashPhoto
        {
            get => (ImageSource)GetValue(PettyCashPhotoProperty);
            set => SetValue(PettyCashPhotoProperty, value);
        }

        private static void OnPettyCashPhotoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DepartmentLeaderSidePanel p && p.PettyCashBrush != null)
                p.PettyCashBrush.ImageSource = e.NewValue as ImageSource;
        }

        public static readonly DependencyProperty ManagerPhotoProperty =
            DependencyProperty.Register(nameof(ManagerPhoto), typeof(ImageSource), typeof(DepartmentLeaderSidePanel),
                new PropertyMetadata(null));

        public ImageSource ManagerPhoto
        {
            get => (ImageSource)GetValue(ManagerPhotoProperty);
            set => SetValue(ManagerPhotoProperty, value);
        }

        public static readonly DependencyProperty DeputyNameProperty =
            DependencyProperty.Register(nameof(DeputyName), typeof(string), typeof(DepartmentLeaderSidePanel),
                new PropertyMetadata(""));

        public string DeputyName
        {
            get => (string)GetValue(DeputyNameProperty);
            set => SetValue(DeputyNameProperty, value);
        }

        public static readonly DependencyProperty DeputyPhotoProperty =
            DependencyProperty.Register(nameof(DeputyPhoto), typeof(ImageSource), typeof(DepartmentLeaderSidePanel),
                new PropertyMetadata(null));

        public ImageSource DeputyPhoto
        {
            get => (ImageSource)GetValue(DeputyPhotoProperty);
            set => SetValue(DeputyPhotoProperty, value);
        }

        public static readonly DependencyProperty LeadershipScoreProperty =
            DependencyProperty.Register(nameof(LeadershipScore), typeof(double), typeof(DepartmentLeaderSidePanel),
                new PropertyMetadata(0.0));

        public double LeadershipScore
        {
            get => (double)GetValue(LeadershipScoreProperty);
            set => SetValue(LeadershipScoreProperty, value);
        }

        public static readonly DependencyProperty SatisfactionScoreProperty =
            DependencyProperty.Register(nameof(SatisfactionScore), typeof(double), typeof(DepartmentLeaderSidePanel),
                new PropertyMetadata(0.0));

        public double SatisfactionScore
        {
            get => (double)GetValue(SatisfactionScoreProperty);
            set => SetValue(SatisfactionScoreProperty, value);
        }

        public static readonly DependencyProperty GrowthScoreProperty =
            DependencyProperty.Register(nameof(GrowthScore), typeof(double), typeof(DepartmentLeaderSidePanel),
                new PropertyMetadata(0.0));

        public double GrowthScore
        {
            get => (double)GetValue(GrowthScoreProperty);
            set => SetValue(GrowthScoreProperty, value);
        }
    }


}