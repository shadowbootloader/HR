using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;

namespace HR.Controls.AddEmployees
{
    public partial class Recruitment : UserControl, IEmployeeSection
    {
        private readonly SectionList<RecruitmentRecord> _list = new();

        public string SectionTitle => "استخدام";

        public Recruitment()
        {
            InitializeComponent();
            _list.Attach(PersonalEmploymentList);

            Employment_OrganTypeCombo.ItemsSource = Lookups.InstitutionTypes;
            Employment_OrganTypeCombo.SelectedIndex = 0;
        }

        private void AddEmploymentDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var name = Employment_OrganNameTB.Text?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                Warn("نام مرکز را وارد کنید.");
                Employment_OrganNameTB.Focus();
                return;
            }

            _list.Items.Add(new RecruitmentRecord
            {
                InstitutionName = name,
                InstitutionType = Employment_OrganTypeCombo.Text?.Trim() ?? string.Empty,
                Result = NullIfEmpty(Employment_ResaltTB.Text),
                Note = NullIfEmpty(Employment_NoteTB.Text)
            });

            ClearForm();
        }

        private void DeleteEmploymentDataBtn_Click(object sender, RoutedEventArgs e) => _list.DeleteSelected();
        private void PersonalEmploymentList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        public void LoadFrom(EmployeeProfile profile) => _list.Replace(profile.Recruitments);
        public void SaveTo(EmployeeProfile profile) => _list.CopyTo(profile.Recruitments);

        public bool Validate(out string message) { message = string.Empty; return true; }

        private void ClearForm()
        {
            Employment_OrganNameTB.Clear();
            Employment_ResaltTB.Clear();
            Employment_NoteTB.Clear();
            Employment_OrganTypeCombo.SelectedIndex = 0;
            Employment_OrganNameTB.Focus();
        }

        private static string? NullIfEmpty(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        private static void Warn(string t)
            => MessageBox.Show(t, "استخدام", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}