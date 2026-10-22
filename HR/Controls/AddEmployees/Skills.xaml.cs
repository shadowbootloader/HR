using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using HR.Models;
using MaterialDesignThemes.Wpf;

namespace HR.Controls.AddEmployees
{
    /// <summary>
    /// Interaction logic for Skills.xaml
    /// </summary>
    public partial class Skills : UserControl
    {
        ObservableCollection<SoftwareSkillItem> Softwares = new();
        public Skills()
        {
            InitializeComponent();
            DataContext = this;
            PersonalSoftwareList.ItemsSource = Softwares;
        }
        private void AddSoftwareDataBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SoftwareNameTB.Text)) return;

            Softwares.Add(new SoftwareSkillItem
            {
                SoftwareName = SoftwareNameTB.Text.Trim(),
                SkillLevel = (SkillCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "متوسط",

            });

            SoftwareNameTB.Clear();

            SkillCombo.SelectedIndex = 0;
        }


        private void DeleteSoftwareChip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Chip btn && btn.DataContext is SoftwareSkillItem item)
                Softwares.Remove(item);
        }
    }
}
