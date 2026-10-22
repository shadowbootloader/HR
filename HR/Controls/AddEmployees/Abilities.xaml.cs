using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using System.Linq;

namespace HR.Controls.AddEmployees
{
    public partial class Abilities : UserControl, IEmployeeSection
    {
        private readonly ObservableCollection<AbilityRecord> _items = new();

        public string SectionTitle => "توانایی‌ها";

        public Abilities()
        {
            InitializeComponent();

            PersonalAbilitiesList.ItemsSource = _items;

            AbilityLevelCombo.SelectedIndex = 1;
        }

        private void AddAbilityDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var name = AbilityNameTB.Text?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                Warn("نام توانایی را وارد کنید.");
                AbilityNameTB.Focus();
                return;
            }

            if (_items.Any(x => x.Title == name))
            {
                Warn($"«{name}» قبلاً اضافه شده است.");
                return;
            }

            _items.Add(new AbilityRecord
            {
                Title = name,
                Level = (AbilityLevelCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() switch
                {
                    "ضعیف" => SkillLevel.Weak,
                    "متوسط" => SkillLevel.Average,
                    "خوب" => SkillLevel.Good,
                    _ => SkillLevel.Excellent
                }
            });

            AbilityNameTB.Clear();
            AbilityLevelCombo.SelectedIndex = 1;
            AbilityNameTB.Focus();
        }

        private void DeleteAbilityChip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MaterialDesignThemes.Wpf.Chip chip &&
                chip.DataContext is AbilityRecord item)
            {
                _items.Remove(item);
            }
        }

        public void LoadFrom(EmployeeProfile profile)
        {
            _items.Clear();
            foreach (var item in profile.Abilities)
                _items.Add(item);
        }

        public void SaveTo(EmployeeProfile profile)
        {
            profile.Abilities.Clear();
            foreach (var item in _items)
                profile.Abilities.Add(item);
        }

        public bool Validate(out string message) { message = string.Empty; return true; }

        private static void Warn(string text)
            => MessageBox.Show(text, "توانایی‌ها", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}