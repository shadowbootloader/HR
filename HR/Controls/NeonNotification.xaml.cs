using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;

namespace HR.Controls
{
    public partial class NeonNotification : UserControl
    {
        public enum NotificationType
        {
            Success,
            Error,
            Warning,
            Info
        }

        private readonly DispatcherTimer _timer;

        private double _duration = 4;

        public event EventHandler Closed;

        public NeonNotification()
        {
            InitializeComponent();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(_duration)
            };

            _timer.Tick += Timer_Tick;
        }


        public void Show(
            string title,
            string message,
            NotificationType type,
            double duration = 4)
        {
            TitleText.Text = title;
            MessageText.Text = message;

            _duration = duration;

            SetNotificationType(type);

            _timer.Stop();
            _timer.Interval = TimeSpan.FromSeconds(duration);

            ProgressBar.BeginAnimation(
                WidthProperty,
                null);

            ProgressBar.Width = 0;

            var progressAnimation = new DoubleAnimation
            {
                From = 0,
                To = 320,
                Duration = TimeSpan.FromSeconds(duration),
                EasingFunction = new QuadraticEase
                {
                    EasingMode = EasingMode.EaseIn
                }
            };

            ProgressBar.BeginAnimation(
                WidthProperty,
                progressAnimation);

            BeginStoryboard("ShowAnimation");

            _timer.Start();
        }


        private void SetNotificationType(NotificationType type)
        {
            string color;
            PackIconKind icon;

            switch (type)
            {
                case NotificationType.Success:

                    color = "#00E5D4";
                    icon = PackIconKind.CheckCircleOutline;

                    break;

                case NotificationType.Error:

                    color = "#FF4D6D";
                    icon = PackIconKind.AlertCircleOutline;

                    break;

                case NotificationType.Warning:

                    color = "#F59E0B";
                    icon = PackIconKind.AlertOutline;

                    break;

                default:

                    color = "#38BDF8";
                    icon = PackIconKind.InformationOutline;

                    break;
            }

            var brush = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(color));

            AccentBar.Background = brush;
            NotificationIcon.Foreground = brush;
            ProgressBar.Background = brush;

            IconGlow.Fill = new SolidColorBrush(
                Color.FromArgb(
                    35,
                    brush.Color.R,
                    brush.Color.G,
                    brush.Color.B));

            if (IconGlow.Effect is DropShadowEffect effect)
            {
                effect.Color = brush.Color;
            }

            NotificationIcon.Kind = icon;
        }


        private void Timer_Tick(
            object sender,
            EventArgs e)
        {
            _timer.Stop();

            Hide();
        }


        private void CloseButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            _timer.Stop();

            Hide();
        }


        public void Hide()
        {
            _timer.Stop();

            var storyboard =
                (Storyboard)FindResource("HideAnimation");

            storyboard = storyboard.Clone();

            storyboard.Completed += (_, _) =>
            {
                Visibility = Visibility.Collapsed;

                Closed?.Invoke(this, EventArgs.Empty);
            };

            storyboard.Begin(this);
        }


        private void BeginStoryboard(string name)
        {
            Visibility = Visibility.Visible;

            var storyboard =
                (Storyboard)FindResource(name);

            storyboard = storyboard.Clone();

            storyboard.Begin(this);
        }
    }
}