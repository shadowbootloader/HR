using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace HR.Controls
{
    public partial class CertificatesCard : UserControl
    {
        public CertificatesCard()
        {
            InitializeComponent();
            Loaded += (s, e) => AddCertificates();
        }

        private void AddCertificates()
        {
            CertificatesList.Children.Clear();

            // داده‌های نمونه (می‌تونی از ViewModel بیاری)
            var certificates = new[]
            {
                ("گواهینامه SHRM-SCP", "2023", PackIconKind.FileCertificateOutline),
                ("مدیریت تغییر Prosci", "2022", PackIconKind.EmailSealOutline),
                ("متخصص آنالیتیکس منابع انسانی", "2022", PackIconKind.Bullseye)
            };

            foreach (var cert in certificates)
            {
                var item = CreateCertificateItem(cert.Item1, cert.Item2, cert.Item3);
                CertificatesList.Children.Add(item);
            }
        }

        private Grid CreateCertificateItem(string title, string year, PackIconKind icon)
        {
            var grid = new Grid { Margin = new System.Windows.Thickness(0, 0, 0, 5) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = System.Windows.GridLength.Auto });

            // سال (سمت چپ در RTL)
            var yearText = new TextBlock
            {
                Text = year,
                FontSize = 13,
                Foreground = System.Windows.Media.Brushes.White,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontWeight = FontWeights.Bold,
                Margin = new System.Windows.Thickness(0, 0, 5, 0)
            };
            System.Windows.Controls.Grid.SetColumn(yearText, 1);
            grid.Children.Add(yearText);

            // عنوان + آیکون (سمت راست در RTL)
            var stack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };

            var iconText = new PackIcon
            {
                Kind = icon,
                Width =20,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new System.Windows.Thickness(0, 0, 10, 0),
                Style= (Style)FindResource("RedNeonIcon")
            };

            var titleText = new TextBlock
            {
                Text = title,
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.White,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };

            stack.Children.Add(iconText);
            stack.Children.Add(titleText);

            System.Windows.Controls.Grid.SetColumn(stack, 0);
            grid.Children.Add(stack);

            return grid;
        }
    }
}