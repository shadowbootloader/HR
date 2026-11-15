using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HR.Data.Entities;
using HR.Models;
using HR.Services;
using HR.Services.Data;

namespace HR.Controls.AddEmployees
{
    public partial class Citizenship : UserControl, IEmployeeSection
    {
        private readonly SectionList<CitizenshipRecord> _list = new();
        private readonly List<Geo> _countries = new();

        public string SectionTitle => "تابعیت";

        public Citizenship()
        {
            InitializeComponent();
            _list.Attach(PersonalCitizenshipList);

            Loaded += Citizenship_Loaded;
        }

        private async void Citizenship_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Citizenship_Loaded;

            try
            {
                using var geo = new GeoService();
                var countries = await geo.GetCountriesAsync();
                if (countries.Count == 0) return;

                _countries.Clear();
                _countries.AddRange(countries);

                Citizenship_CountryNameCombo.ItemsSource = countries.Select(c => c.Name).ToList();
                Citizenship_CountryNameCombo.SelectedIndex = 0;
            }
            catch
            {
                // دیتابیس در دسترس نیست → لیست خالی می‌ماند
            }
        }

        private void AddCitizenshipDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var country = Citizenship_CountryNameCombo.SelectedItem as string
                          ?? Citizenship_CountryNameCombo.Text?.Trim();

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
            if (Citizenship_CountryNameCombo.Items.Count > 0)
                Citizenship_CountryNameCombo.SelectedIndex = 0;
        }

        private static string? NullIfEmpty(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        private static void Warn(string t)
            => MessageBox.Show(t, "تابعیت", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}