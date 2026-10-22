using System;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;

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

            OriginCountryCombo.ItemsSource = Lookups.Countries;
            DestinationCountryCombo.ItemsSource = Lookups.Countries;
            TransportCombo.ItemsSource = Lookups.TransportModes;

            OriginCountryCombo.SelectedIndex = 0;
            DestinationCountryCombo.SelectedIndex = 0;
            TransportCombo.SelectedIndex = 0;
        }

        private void AddForenTravelDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var origin = OriginCountryCombo.Text?.Trim();
            var destination = DestinationCountryCombo.Text?.Trim();

            if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
            {
                Warn("کشور مبدأ و مقصد را انتخاب کنید.");
                return;
            }

            var depart = TravelStartDateDP.SelectedDate;
            var back = TravelEndDateDP.SelectedDate;
            if (depart.HasValue && back.HasValue && back < depart)
            {
                Warn("تاریخ برگشت نمی‌تواند قبل از تاریخ رفت باشد.");
                return;
            }

            _list.Items.Add(new TripAbroadRecord
            {
                OriginCountry = origin,
                DestinationCountry = destination,
                Transport = TransportCombo.Text?.Trim() ?? string.Empty,
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