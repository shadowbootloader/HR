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

namespace HR.Pages
{
    /// <summary>
    /// Interaction logic for DepartmentDetailPage.xaml
    /// </summary>
    public partial class DepartmentDetailPage : Page
    {
        public DepartmentDetailPage()
        {
            InitializeComponent();
        }

        private void EditDepartment_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeptTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (PanelSummary == null) return;

            //PanelSummary.Visibility = Visibility.Collapsed;
            //PanelEmployees.Visibility = Visibility.Collapsed;
            //PanelOrg.Visibility = Visibility.Collapsed;
            //PanelProjects.Visibility = Visibility.Collapsed;
            //PanelFinance.Visibility = Visibility.Collapsed;
            //PanelReports.Visibility = Visibility.Collapsed;

            //switch (DeptTabs.SelectedIndex)
            //{
            //    case 0: PanelSummary.Visibility = Visibility.Visible; break;
            //    case 1: PanelEmployees.Visibility = Visibility.Visible; break;
            //    case 2: PanelOrg.Visibility = Visibility.Visible; break;
            //    case 3: PanelProjects.Visibility = Visibility.Visible; break;
            //    case 4: PanelFinance.Visibility = Visibility.Visible; break;
            //    case 5: PanelReports.Visibility = Visibility.Visible; break;
            //}
        }
    }
}
