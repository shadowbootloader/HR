using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using System.Linq;

namespace HR.Controls.AddEmployees
{
    public partial class WorkInterests : UserControl, IEmployeeSection
    {
        private readonly ObservableCollection<WorkInterestRecord> _items = new();

        public string SectionTitle => "علایق شغلی";

        public WorkInterests()
        {
            InitializeComponent();

            PersonalWorkInterestsList.ItemsSource = _items;

            InterestLevelCombo.SelectedIndex = 1;
        }

        private void AddWorkInterestDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var name = InterestNameTB.Text?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                Warn("عنوان علاقه شغلی را وارد کنید.");
                InterestNameTB.Focus();
                return;
            }

            if (_items.Any(x => x.Title == name))
            {
                Warn($"«{name}» قبلاً اضافه شده است.");
                return;
            }

            _items.Add(new WorkInterestRecord
            {
                Title = name,
                Level = (InterestLevelCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() switch
                {
                    "کم" => InterestLevel.Low,
                    "متوسط" => InterestLevel.Medium,
                    _ => InterestLevel.High
                }
            });

            InterestNameTB.Clear();
            InterestLevelCombo.SelectedIndex = 1;
            InterestNameTB.Focus();
        }

        private void DeleteInterestingChip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MaterialDesignThemes.Wpf.Chip chip &&
                chip.DataContext is WorkInterestRecord item)
            {
                _items.Remove(item);
            }
        }

        public void LoadFrom(EmployeeProfile profile)
        {
            _items.Clear();
            foreach (var item in profile.WorkInterests)
                _items.Add(item);
        }

        public void SaveTo(EmployeeProfile profile)
        {
            profile.WorkInterests.Clear();
            foreach (var item in _items)
                profile.WorkInterests.Add(item);
        }

        public bool Validate(out string message) { message = string.Empty; return true; }

        private static void Warn(string text)
            => MessageBox.Show(text, "علایق شغلی", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}