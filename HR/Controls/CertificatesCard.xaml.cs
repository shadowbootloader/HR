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
        }

        public void ShowCertificates(IReadOnlyList<(string Title, string Detail)> items)
        {
            CertificatesList.Children.Clear();
            if (items.Count == 0)
            {
                CertificatesList.Children.Add(new TextBlock
                {
                    Text = "مدرک دوره‌ای ثبت نشده است.",
                    Foreground = System.Windows.Media.Brushes.White,
                    Margin = new Thickness(8)
                });
                return;
            }

            foreach (var item in items)
                CertificatesList.Children.Add(CreateCertificateItem(item.Title, item.Detail, PackIconKind.FileCertificateOutline));
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