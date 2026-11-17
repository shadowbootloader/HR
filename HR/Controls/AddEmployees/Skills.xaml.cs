using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;
using HR.Services.Data;

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

            SkillCombo.DisplayMemberPath = nameof(ComboItem.Text);
            SkillCombo.SelectedValuePath = nameof(ComboItem.Value);

            Loaded += Skills_Loaded;
        }

        private async void Skills_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Skills_Loaded;

            SkillCombo.Items.Clear();
            await LookupBinder.FillAsync<SkillLevel>(SkillCombo, LookupKeys.SkillLevel);
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
                Level = SkillCombo.SelectedValue is SkillLevel level ? level : SkillLevel.Average,
                LevelTitle = LookupBinder.SelectedTitle(SkillCombo)
            });

            SoftwareNameTB.Clear();
            if (SkillCombo.Items.Count > 1) SkillCombo.SelectedIndex = 1;
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