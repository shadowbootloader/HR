using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;

namespace HR.Controls.AddEmployees
{
    public partial class Citizenship : UserControl, IEmployeeSection
    {
        private readonly SectionList<CitizenshipRecord> _list = new();

        public string SectionTitle => "تابعیت";

        public Citizenship()
        {
            InitializeComponent();
            _list.Attach(PersonalCitizenshipList);

            Citizenship_CountryNameCombo.ItemsSource = Lookups.Countries;
            Citizenship_CountryNameCombo.SelectedIndex = 0;
        }

        private void AddCitizenshipDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var country = Citizenship_CountryNameCombo.Text?.Trim();

            if (string.IsNullOrWhiteSpace(country))
            {
                Warn("کشور را انتخاب کنید.");
                return;
            }

            _list.Items.Add(new CitizenshipRecord
            {
                Country = country,
                Reason = Citizenship_ReasonTB.Text?.Trim() ?? string.Empty,
                StartDate = Citizenship_StartDateDP.SelectedDate,
                Note = NullIfEmpty(Citizenship_NoteTB.Text)
            });

            ClearForm();
        }

        private void DeleteCitizenshipDataBtn_Click(object sender, RoutedEventArgs e) => _list.DeleteSelected();
        private void PersonalCitizenshipList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        public void LoadFrom(EmployeeProfile profile) => _list.Replace(profile.Citizenships);
        public void SaveTo(EmployeeProfile profile) => _list.CopyTo(profile.Citizenships);

        public bool Validate(out string message) { message = string.Empty; return true; }

        private void ClearForm()
        {
            Citizenship_ReasonTB.Clear();
            Citizenship_NoteTB.Clear();
            Citizenship_StartDateDP.SelectedDate = null;
            Citizenship_CountryNameCombo.SelectedIndex = 0;
        }

        private static string? NullIfEmpty(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        private static void Warn(string t)
            => MessageBox.Show(t, "تابعیت", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}