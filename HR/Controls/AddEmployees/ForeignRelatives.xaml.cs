using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;
using HR.Services.Data;

namespace HR.Controls.AddEmployees
{
    public partial class ForeignRelatives : UserControl, IEmployeeSection
    {
        private readonly SectionList<ForeignRelativeRecord> _list = new();

        public string SectionTitle => "بستگان خارجی";

        public ForeignRelatives()
        {
            InitializeComponent();
            _list.Attach(PersonalForeignFamilyList);

            Loaded += ForeignRelatives_Loaded;
        }

        private async void ForeignRelatives_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ForeignRelatives_Loaded;

            try
            {
                using var geo = new GeoService();
                var countries = await geo.GetCountriesAsync();
                if (countries.Count == 0) return;

                ForeignFamily_CountryNameCombo.ItemsSource = countries.Select(c => c.Name).ToList();
                ForeignFamily_CountryNameCombo.SelectedIndex = 0;
            }
            catch
            {
                // دیتابیس در دسترس نیست → لیست خالی می‌ماند
            }
        }

        private void AddForeignFamilyDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var name = ForeignFamily_FamilyNameCombo.Text?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                Warn("نام و نام خانوادگی را وارد کنید.");
                return;
            }

            _list.Commit(new ForeignRelativeRecord
            {
                FullName = name,
                Country = ForeignFamily_CountryNameCombo.SelectedItem as string
                          ?? ForeignFamily_CountryNameCombo.Text?.Trim()
                          ?? string.Empty,
                DepartureDate = ForeignFamily_StartDateDP.SelectedDate,
                Reason = NullIfEmpty(ForeignFamily_ReasonTB.Text),
                Note = NullIfEmpty(ForeignFamily_NoteTB.Text)
            });

            ClearForm();
        }

        private void DeleteForeignFamilyDataBtn_Click(object sender, RoutedEventArgs e) => _list.DeleteSelected();
        private void PersonalForeignFamilyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PersonalForeignFamilyList.SelectedItem is not ForeignRelativeRecord item) return;
            ForeignFamily_FamilyNameCombo.Text = item.FullName;
            LookupBinder.Select(ForeignFamily_CountryNameCombo, item.Country);
            ForeignFamily_StartDateDP.SelectedDate = item.DepartureDate;
            ForeignFamily_ReasonTB.Text = item.Reason ?? string.Empty;
            ForeignFamily_NoteTB.Text = item.Note ?? string.Empty;
        }

        public void LoadFrom(EmployeeProfile profile) => _list.Replace(profile.ForeignRelatives);
        public void SaveTo(EmployeeProfile profile) => _list.CopyTo(profile.ForeignRelatives);

        public bool Validate(out string message) { message = string.Empty; return true; }

        private void ClearForm()
        {
            ForeignFamily_FamilyNameCombo.Text = string.Empty;
            ForeignFamily_ReasonTB.Clear();
            ForeignFamily_NoteTB.Clear();
            ForeignFamily_StartDateDP.SelectedDate = null;
            if (ForeignFamily_CountryNameCombo.Items.Count > 0)
                ForeignFamily_CountryNameCombo.SelectedIndex = 0;
        }

        private static string? NullIfEmpty(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        private static void Warn(string t)
            => MessageBox.Show(t, "بستگان خارجی", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}