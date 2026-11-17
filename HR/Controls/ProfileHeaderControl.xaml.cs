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
    /// Interaction logic for ProfileHeaderControl.xaml
    /// </summary>
    public partial class ProfileHeaderControl : UserControl
    {
        public ProfileHeaderControl()
        {
            InitializeComponent();
        }

        public void Bind(string? fullName, string? position, string? department, string? location)
        {
            EmpFullName.Text = string.IsNullOrWhiteSpace(fullName) ? "—" : fullName;
            EmpGroup.Text = string.IsNullOrWhiteSpace(position) ? "—" : position;
            EmpFeild.Text = string.IsNullOrWhiteSpace(department) ? "—" : department;
            EmpLocation.Text = string.IsNullOrWhiteSpace(location) ? string.Empty : location;
        }
    }
}
