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
            var status = e.NewValue?.ToString() ?? string.Empty;
            var (background, foreground, border, text) = status switch
            {
                "فعال" or "Active" => ("#14532D", "#22C55E", "#22C55E", "فعال"),
                "تازه‌وارد" => ("#1E3A8A", "#60A5FA", "#3B82F6", "تازه‌وارد"),
                "مرخصی" or "On Leave" => ("#713F12", "#F59E0B", "#F59E0B", "مرخصی"),
                "غیرفعال" => ("#1E293B", "#94A3B8", "#475569", "غیرفعال"),
                "حذف‌شده" => ("#450A0A", "#F87171", "#F87171", "حذف‌شده"),
                _ => ("#1E293B", "#E2E8F0", "#334155", string.IsNullOrWhiteSpace(status) ? "—" : status)
            };

            ctrl.BadgeBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(background));
            ctrl.BadgeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(foreground));
            ctrl.BadgeBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(border));
            ctrl.BadgeBorder.BorderThickness = new Thickness(1);
            ctrl.BadgeText.Text = text;
        }
    }
}
