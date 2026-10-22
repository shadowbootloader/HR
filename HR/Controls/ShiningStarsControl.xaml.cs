using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HR.Controls
{
    /// <summary>
    /// Interaction logic for ShiningStarsControl.xaml
    /// </summary>
    public partial class ShiningStarsControl : UserControl
    {
        public ShiningStarsControl()
        {
            InitializeComponent();
            
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var sb = (Storyboard)FindResource("ShiningStarsStoryboard");
            sb.Begin();
        }
    }
}
