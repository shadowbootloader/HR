using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using HR.Models;

namespace HR.Controls.AddEmployees
{
    public partial class Skills : UserControl, IEmployeeSection
    {
        private readonly ObservableCollection<SoftwareSkillRecord> _softwares = new();

        public string SectionTitle => "نرم‌افزارها";

        public Skills()
        {
            InitializeComponent();

            PersonalSoftwareList.ItemsSource = _softwares;

            SkillCombo.SelectedIndex = 1;
        }

        private void AddSoftwareDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var name = SoftwareNameTB.Text?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("نام نرم‌افزار را وارد کنید.", "نرم‌افزارها",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                SoftwareNameTB.Focus();
                return;
            }

            if (_softwares.Any(x => x.SoftwareName == name))
            {
                MessageBox.Show($"«{name}» قبلاً اضافه شده است.", "نرم‌افزارها",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _softwares.Add(new SoftwareSkillRecord
            {
                SoftwareName = name,
                Level = (SkillCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() switch
                {
                    "ضعیف" => SkillLevel.Weak,
                    "متوسط" => SkillLevel.Average,
                    "خوب" => SkillLevel.Good,
                    _ => SkillLevel.Excellent
                }
            });

            SoftwareNameTB.Clear();
            SkillCombo.SelectedIndex = 1;
            SoftwareNameTB.Focus();
        }

        private void DeleteSoftwareChip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MaterialDesignThemes.Wpf.Chip chip &&
                chip.DataContext is SoftwareSkillRecord item)
            {
                _softwares.Remove(item);
            }
        }

        // ==================== IEmployeeSection ====================

        public void LoadFrom(EmployeeProfile profile)
        {
            _softwares.Clear();
            foreach (var item in profile.SoftwareSkills)
                _softwares.Add(item);
        }

        public void SaveTo(EmployeeProfile profile)
        {
            profile.SoftwareSkills.Clear();
            foreach (var item in _softwares)
                profile.SoftwareSkills.Add(item);
        }

        public bool Validate(out string message)
        {
            message = string.Empty;
            return true;
        }
    }
}