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
    /// Interaction logic for WorkInterests.xaml
    /// </summary>
    public partial class WorkInterests : UserControl
    {
        ObservableCollection<InterestingItem> WorkInt = new();
        public WorkInterests()
        {
            InitializeComponent();
            DataContext = this;
            PersonalWorkInterestsList.ItemsSource = WorkInt;
        }

        private void AddWorkInterestDataBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InterestNameTB.Text)) return;

            WorkInt.Add(new InterestingItem
            {
                InterestingItemName = InterestNameTB.Text.Trim(),
                InterestingItemLevel = (InterestLevelCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "متوسط",

            });

            InterestNameTB.Clear();

            InterestLevelCombo.SelectedIndex = 0;
        }

        private void DeleteInterestingChip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Chip btn && btn.DataContext is InterestingItem item)
                WorkInt.Remove(item);
        }
    }
}
