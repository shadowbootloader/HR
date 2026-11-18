using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using HR.Services;
using HR.Services.Org;

namespace HR.Controls
{
    /// <summary>چارت بالا به پایین با خط رابط، قابل بزرگ‌نمایی داخل ScrollViewer والد.</summary>
    public class OrgChartBoard : UserControl
    {
        private readonly ScaleTransform _zoom = new(1, 1);
        private readonly Grid _host = new();

        public OrgChartBoard()
        {
            FlowDirection = FlowDirection.LeftToRight;
            var scroller = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = _host
            };
            _host.LayoutTransform = _zoom;
            _host.HorizontalAlignment = HorizontalAlignment.Center;
            Content = scroller;
        }

        public void SetZoom(double value)
        {
            var scale = value <= 0 ? 1 : value;
            _zoom.ScaleX = scale;
            _zoom.ScaleY = scale;
        }

        public void Show(OrgBox? root)
        {
            _host.Children.Clear();
            if (root == null || root.Kind == "empty")
            {
                _host.Children.Add(new TextBlock
                {
                    Text = root?.Subtitle ?? "دپارتمانی برای رسم چارت ثبت نشده است",
                    Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)),
                    FontSize = 16,
                    Margin = new Thickness(24)
                });
                return;
            }

            _host.Children.Add(Draw(root));
        }

        private static UIElement Draw(OrgBox node)
        {
            var card = Card(node);
            if (node.Children.Count == 0) return card;

            var children = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            foreach (var child in node.Children)
            {
                var column = new StackPanel { Margin = new Thickness(8, 0, 8, 0) };
                column.Children.Add(Stem());
                column.Children.Add(Draw(child));
                children.Children.Add(column);
            }

            var tree = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            tree.Children.Add(card);
            tree.Children.Add(Stem());
            if (node.Children.Count > 1)
            {
                tree.Children.Add(new Border
                {
                    Height = 2,
                    Background = Line(),
                    Margin = new Thickness(48, 0, 48, 0)
                });
            }
            tree.Children.Add(children);
            return tree;
        }

        private static Border Card(OrgBox node)
        {
            var person = node.Kind == "person";
            var photo = ImagePicker.Load(node.LogoPath);
            var mark = new Ellipse
            {
                Width = 36,
                Height = 36,
                Stroke = Line(),
                StrokeThickness = 1,
                Fill = photo == null
                    ? new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B))
                    : new ImageBrush(photo) { Stretch = Stretch.UniformToFill, AlignmentX = AlignmentX.Center, AlignmentY = AlignmentY.Center },
                Margin = new Thickness(0, 0, 10, 0)
            };
            var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center, FlowDirection = FlowDirection.RightToLeft };
            text.Children.Add(new TextBlock
            {
                Text = node.Title,
                Foreground = Brushes.White,
                FontSize = person ? 14 : 16,
                FontWeight = FontWeights.SemiBold
            });
            text.Children.Add(new TextBlock
            {
                Text = node.Subtitle,
                Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)),
                FontSize = 12
            });
            var row = new StackPanel { Orientation = Orientation.Horizontal, FlowDirection = FlowDirection.LeftToRight };
            row.Children.Add(mark);
            row.Children.Add(text);
            return new Border
            {
                Child = row,
                Background = new SolidColorBrush(person ? Color.FromRgb(0x0F, 0x17, 0x2A) : Color.FromRgb(0x11, 0x18, 0x27)),
                BorderBrush = new SolidColorBrush(person ? Color.FromRgb(0x33, 0x41, 0x55) : Color.FromRgb(0x38, 0xBD, 0xF8)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10, 8, 10, 8),
                Margin = new Thickness(4),
                MinWidth = person ? 140 : 180
            };
        }

        private static Rectangle Stem()
            => new()
            {
                Width = 2,
                Height = 16,
                Fill = Line(),
                HorizontalAlignment = HorizontalAlignment.Center
            };

        private static SolidColorBrush Line() => new(Color.FromRgb(0x33, 0x41, 0x55));
    }
}
