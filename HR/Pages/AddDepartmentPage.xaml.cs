using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace HR.Pages
{
    /// <summary>
    /// Interaction logic for AddDepartmentPage.xaml
    /// </summary>
    public partial class AddDepartmentPage : Page
    {
        public AddDepartmentPage()
        {
            InitializeComponent();
        }

        private bool _formatting;
        private static readonly CultureInfo Fa = new CultureInfo("fa-IR");

        private void MoneyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_formatting) return;
            if (sender is not TextBox tb) return;

            _formatting = true;

            // فقط رقم (انگلیسی یا فارسی)
            string digits = new string(tb.Text
                .Select(c => c is >= '۰' and <= '۹' ? (char)('0' + (c - '۰')) : c)
                .Where(char.IsDigit)
                .ToArray());

            if (string.IsNullOrEmpty(digits))
            {
                tb.Text = "";
                _formatting = false;
                return;
            }

            if (long.TryParse(digits, out long value))
            {
                // جداکننده هزارگان فارسی: ۱٬۵۰۰٬۰۰۰
                string formatted = value.ToString("N0", Fa);

                int caret = tb.CaretIndex;
                int oldLen = tb.Text.Length;

                tb.Text = formatted;
                tb.CaretIndex = Math.Max(0, caret + (tb.Text.Length - oldLen));
            }

            _formatting = false;
        }

        private long ParseMoney(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;

            string digits = new string(text
                .Select(c => c is >= '۰' and <= '۹' ? (char)('0' + (c - '۰')) : c)
                .Where(char.IsDigit)
                .ToArray());

            return long.TryParse(digits, out long v) ? v : 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddOfficeBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveOfficeBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveDepartmentBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
