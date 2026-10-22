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

namespace HR.Controls.AddEmployees
{
    /// <summary>
    /// Interaction logic for BasicInfo.xaml
    /// </summary>
    public partial class BasicInfo : UserControl
    {
        public BasicInfo()
        {
            InitializeComponent();
        }
        private void UploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image|*.jpg;*.jpeg;*.png"
            };
            if (dlg.ShowDialog() == true)
            {
                var bmp = new BitmapImage(new Uri(dlg.FileName));
                PhotoBrush.ImageSource = bmp;
                PhotoEllipse.Visibility = Visibility.Visible;
            }
        }

        private void NationalCardImageBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void IDImageBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SarbaziImageBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
