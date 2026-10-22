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
    /// Interaction logic for StatusBadgeControl.xaml
    /// </summary>
    public partial class StatusBadgeControl : UserControl
    {
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(nameof(Status), typeof(string), typeof(StatusBadgeControl),
                new PropertyMetadata("", OnStatusChanged));

        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public StatusBadgeControl() => InitializeComponent();

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (StatusBadgeControl)d;
            string status = e.NewValue?.ToString() ?? "Active";

            if (status == "Active")
            {
                ctrl.BadgeBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#14532D"));
                ctrl.BadgeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E"));
                ctrl.BadgeBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E"));
                ctrl.BadgeBorder.BorderThickness = new Thickness(1);
                ctrl.BadgeText.Text = "Active";
            }
            else // On Leave
            {
                ctrl.BadgeBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#713F12"));
                ctrl.BadgeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                ctrl.BadgeBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                ctrl.BadgeBorder.BorderThickness = new Thickness(1);
                ctrl.BadgeText.Text = "On Leave";
            }
        }
    }
}
