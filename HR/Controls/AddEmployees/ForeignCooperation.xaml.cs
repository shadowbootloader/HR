using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;
using HR.Services.Data;

namespace HR.Controls.AddEmployees
{
    public partial class ForeignCooperation : UserControl, IEmployeeSection
    {
        private readonly SectionList<ForeignCooperationRecord> _list = new();

        public string SectionTitle => "همکاری خارجی";

        public ForeignCooperation()
        {
            InitializeComponent();
            _list.Attach(PersonalForeignCooperationList);

            Loaded += ForeignCooperation_Loaded;
        }

        private async void ForeignCooperation_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ForeignCooperation_Loaded;

            try
            {
                using var geo = new GeoService();
                var countries = await geo.GetCountriesAsync();

                if (countries.Count > 0)
                {
                    ForeignCooperation_CountryCombo.ItemsSource = countries.Select(c => c.Name).ToList();
                    ForeignCooperation_CountryCombo.SelectedIndex = 0;
                }
            }
            catch
            {
                // دیتابیس در دسترس نیست → لیست خالی می‌ماند
            }

            try
            {
                var types = (await LookupCache.GetAsync())[LookupKeys.CenterType];
                if (types.Count > 0)
                {
                    ForeignCooperation_InstitueTypeCombo.ItemsSource = types.Select(t => t.Title).ToList();
                    ForeignCooperation_InstitueTypeCombo.SelectedIndex = 0;
                }
            }
            catch
            {
                // دیتابیس در دسترس نیست → لیست خالی می‌ماند
            }
        }

        private void AddForeignCooperationDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var person = ForeignCooperation_FamilyNameCombo.Text?.Trim();
            var institute = ForeignCooperation_InstitueNameTB.Text?.Trim();

            if (string.IsNullOrWhiteSpace(person))
            {
                Warn("نام و نام خانوادگی را وارد کنید.");
                return;
            }

            if (string.IsNullOrWhiteSpace(institute))
            {
                Warn("نام مرکز را وارد کنید.");
                ForeignCooperation_InstitueNameTB.Focus();
                return;
            }

            var start = ForeignCooperation_StartDateDP.SelectedDate;
            var end = TForeignCooperation_EndDateDP.SelectedDate;
            if (start.HasValue && end.HasValue && end < start)
            {
                Warn("تاریخ اتمام نمی‌تواند قبل از تاریخ شروع باشد.");
                return;
            }

            _list.Commit(new ForeignCooperationRecord
            {
                PersonName = person,
                Country = ForeignCooperation_CountryCombo.SelectedItem as string
                          ?? ForeignCooperation_CountryCombo.Text?.Trim()
                          ?? string.Empty,
                InstitutionName = institute,
                InstitutionType = ForeignCooperation_InstitueTypeCombo.SelectedItem as string
                                  ?? ForeignCooperation_InstitueTypeCombo.Text?.Trim()
                                  ?? string.Empty,
                StartDate = start,
                EndDate = end,
                Note = NullIfEmpty(ForeignCooperation_NoteTB.Text)
            });

            ClearForm();
        }

        private void DeleteForeignCooperationDataBtn_Click(object sender, RoutedEventArgs e) => _list.DeleteSelected();
        private void PersonalForeignCooperationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PersonalForeignCooperationList.SelectedItem is not ForeignCooperationRecord item) return;
            ForeignCooperation_FamilyNameCombo.Text = item.PersonName;
            LookupBinder.Select(ForeignCooperation_CountryCombo, item.Country);
            ForeignCooperation_InstitueNameTB.Text = item.InstitutionName;
            LookupBinder.Select(ForeignCooperation_InstitueTypeCombo, item.InstitutionType);
            ForeignCooperation_StartDateDP.SelectedDate = item.StartDate;
            TForeignCooperation_EndDateDP.SelectedDate = item.EndDate;
            ForeignCooperation_NoteTB.Text = item.Note ?? string.Empty;
        }

        public void LoadFrom(EmployeeProfile profile) => _list.Replace(profile.ForeignCooperations);
        public void SaveTo(EmployeeProfile profile) => _list.CopyTo(profile.ForeignCooperations);

        public bool Validate(out string message)
        {
            foreach (var record in _list.Items)
            {
                if (record.StartDate.HasValue && record.EndDate.HasValue && record.EndDate < record.StartDate)
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
            ForeignCooperation_FamilyNameCombo.Text = string.Empty;
            ForeignCooperation_InstitueNameTB.Clear();
            ForeignCooperation_NoteTB.Clear();
            ForeignCooperation_StartDateDP.SelectedDate = null;
            TForeignCooperation_EndDateDP.SelectedDate = null;
            if (ForeignCooperation_CountryCombo.Items.Count > 0) ForeignCooperation_CountryCombo.SelectedIndex = 0;
            if (ForeignCooperation_InstitueTypeCombo.Items.Count > 0) ForeignCooperation_InstitueTypeCombo.SelectedIndex = 0;
        }

        private static string? NullIfEmpty(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        private static void Warn(string t)
            => MessageBox.Show(t, "همکاری خارجی", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}