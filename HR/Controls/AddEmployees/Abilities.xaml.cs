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
    /// Interaction logic for Abilities.xaml
    /// </summary>
    public partial class Abilities : UserControl
    {
        
        ObservableCollection<AbilityItem> Abillits = new();

        public Abilities()
        {
            InitializeComponent();
            DataContext = this;
            PersonalAbilitiesList.ItemsSource = Abillits;
        }
        
        private void AddAbilityDataBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AbilityNameTB.Text)) return;

            Abillits.Add(new AbilityItem
            {
                AbilityName = AbilityNameTB.Text.Trim(),
                AbilityLevel = (AbilityLevelCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "متوسط",

            });

            AbilityNameTB.Clear();

            AbilityLevelCombo.SelectedIndex = 0;
        }


        private void DeleteAbilityChip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Chip chip && chip.DataContext is AbilityItem item)
                    Abillits.Remove(item);
        }
    }
}
