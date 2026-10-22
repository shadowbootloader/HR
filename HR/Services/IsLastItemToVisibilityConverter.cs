using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace HR.Services
{
    public class IsLastItemToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return Visibility.Visible;

            if (values[0] is not ItemsControl itemsControl) return Visibility.Visible;

            var item = values[1];
            int index = itemsControl.Items.IndexOf(item);
            int count = itemsControl.Items.Count;

            // آخرین آیتم → خط مخفی
            if (index >= 0 && index == count - 1)
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}