using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;
using HR.Services.Data;

namespace HR.Controls.AddEmployees
{
    public partial class TripAbroad : UserControl, IEmployeeSection
    {
        private readonly SectionList<TripAbroadRecord> _list = new();

        public string SectionTitle => "سفرهای خارجی";

        public TripAbroad()
        {
            InitializeComponent();
            _list.Attach(PersonalForenTravelList);
            Loaded += TripAbroad_Loaded;
        }

        private async void TripAbroad_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= TripAbroad_Loaded;

            try
            {
                using var geo = new GeoService();
                var countries = await geo.GetCountriesAsync();
                var names = countries.Select(c => c.Name).ToList();
                if (names.Count == 0) names.Add("—");

                OriginCountryCombo.ItemsSource = names;
                DestinationCountryCombo.ItemsSource = names.ToList();
                OriginCountryCombo.SelectedIndex = 0;
                DestinationCountryCombo.SelectedIndex = 0;

                await LookupBinder.FillTitlesAsync(TransportCombo, LookupKeys.TransportType, addEmptyItem: true);
            }
            catch
            {
                OriginCountryCombo.ItemsSource = new[] { "—" };
                DestinationCountryCombo.ItemsSource = new[] { "—" };
                TransportCombo.ItemsSource = new[] { "—" };
                OriginCountryCombo.SelectedIndex = 0;
                DestinationCountryCombo.SelectedIndex = 0;
                TransportCombo.SelectedIndex = 0;
            }
        }

        private void AddForenTravelDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var origin = OriginCountryCombo.SelectedItem as string ?? OriginCountryCombo.Text;
            var dest = DestinationCountryCombo.SelectedItem as string ?? DestinationCountryCombo.Text;
            var transport = TransportCombo.SelectedItem as string ?? TransportCombo.Text;

            if (string.IsNullOrWhiteSpace(origin) || origin == "—")
            {
                Warn("کشور مبدأ را انتخاب کنید.");
                return;
            }
            if (string.IsNullOrWhiteSpace(dest) || dest == "—")
            {
                Warn("کشور مقصد را انتخاب کنید.");
                return;
            }

            var depart = TravelStartDateDP.SelectedDate;
            var back = TravelEndDateDP.SelectedDate;
            if (depart.HasValue && back.HasValue && back < depart)
            {
                Warn("تاریخ بازگشت نمی‌تواند قبل از تاریخ رفت باشد.");
                return;
            }

            _list.Items.Add(new TripAbroadRecord
            {
                OriginCountry = origin.Trim(),
                DestinationCountry = dest.Trim(),
                Transport = string.IsNullOrWhiteSpace(transport) || transport == "—" ? string.Empty : transport.Trim(),
                Reason = ReasonNameTB.Text?.Trim() ?? string.Empty,
                DepartDate = depart,
                ReturnDate = back,
                Companions = NullIfEmpty(CompanionsTB.Text),
                Note = NullIfEmpty(Travle_NoteTB.Text)
            });

            ClearForm();
        }

        private void DeleteForenTravelDataBtn_Click(object sender, RoutedEventArgs e) => _list.DeleteSelected();

        public void LoadFrom(EmployeeProfile profile) => _list.Replace(profile.TripsAbroad);
        public void SaveTo(EmployeeProfile profile) => _list.CopyTo(profile.TripsAbroad);

        public bool Validate(out string message)
        {
            foreach (var record in _list.Items)
            {
                if (record.DepartDate.HasValue && record.ReturnDate.HasValue && record.ReturnDate < record.DepartDate)
                {
                    message = $"بازه تاریخ نامعتبر در ردیف {record.Row}";
                    return false;
                }
            }
            message = string.Empty;
            return true;
        }

        private void ClearForm()
        {
            ReasonNameTB.Clear();
            CompanionsTB.Clear();
            Travle_NoteTB.Clear();
            TravelStartDateDP.SelectedDate = null;
            TravelEndDateDP.SelectedDate = null;
            OriginCountryCombo.SelectedIndex = 0;
            DestinationCountryCombo.SelectedIndex = 0;
            TransportCombo.SelectedIndex = 0;
            ReasonNameTB.Focus();
        }

        private static string? NullIfEmpty(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        private static void Warn(string t)
            => MessageBox.Show(t, "سفرهای خارجی", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
