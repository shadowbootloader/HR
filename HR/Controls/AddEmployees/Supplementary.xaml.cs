using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;
using HR.Services.Data;

namespace HR.Controls.AddEmployees
{
    public partial class Supplementary : UserControl, IEmployeeSection
    {
        private string _resumePath = string.Empty;

        public string SectionTitle => "اطلاعات تکمیلی";

        public Supplementary()
        {
            InitializeComponent();

            OragPlaceCombo.ItemsSource = new[] { "—" }.Concat(Lookups.Positions).ToList();
            OragPlaceCombo.SelectedIndex = 0;

            Loaded += Supplementary_Loaded;
        }

        private async void Supplementary_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Supplementary_Loaded;

            try
            {
                var cache = await LookupCache.GetAsync();

                var insurance = cache[LookupKeys.InsuranceType].Select(x => x.Title).ToList();
                if (insurance.Count > 0) InsuranceTypeCombo.ItemsSource = new[] { "—" }.Concat(insurance).ToList();

                var cooperation = cache[LookupKeys.CooperationType].Select(x => x.Title).ToList();
                if (cooperation.Count > 0) CooperationTypeCombo.ItemsSource = new[] { "—" }.Concat(cooperation).ToList();

                var departments = cache[LookupKeys.Department].Select(x => x.Title).ToList();
                DepartmentCombo.ItemsSource = new[] { "—" }.Concat(departments).ToList();
            }
            catch
            {
                // دیتابیس در دسترس نیست → لیست پیش‌فرض خالی می‌ماند
            }

            if (InsuranceTypeCombo.Items.Count > 0) InsuranceTypeCombo.SelectedIndex = 0;
            if (CooperationTypeCombo.Items.Count > 0) CooperationTypeCombo.SelectedIndex = 0;
            if (DepartmentCombo.Items.Count > 0) DepartmentCombo.SelectedIndex = 0;
        }

        private void CVImageBtn_Click(object sender, RoutedEventArgs e)
        {
            var picked = ImagePicker.PickDocument();
            if (picked == null) return;

            _resumePath = picked;
            ImagePicker.Preview(CVImage, _resumePath);
        }

        public void LoadFrom(EmployeeProfile profile)
        {
            var info = profile.Supplementary;

            PersonnelCodeTB.Text = profile.PersonnelCode;
            SelectText(DepartmentCombo, profile.DepartmentName);
            SelectedPostTB.Text = info.Post ?? string.Empty;
            SelectText(OragPlaceCombo, info.Position);
            SelectText(InsuranceTypeCombo, info.InsuranceType);
            SelectText(CooperationTypeCombo, info.CooperationType);

            CooprationStartDateDP.SelectedDate = info.StartDate;
            CooprationEndDateDP.SelectedDate = info.EndDate;

            SalaryTB.Text = info.Salary?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

            _resumePath = info.ResumePath;
            ImagePicker.Preview(CVImage, _resumePath);
        }

        public void SaveTo(EmployeeProfile profile)
        {
            var info = profile.Supplementary;

            profile.PersonnelCode = PersonnelCodeTB.Text?.Trim() ?? string.Empty;
            profile.DepartmentName = BlankDash(DepartmentCombo.Text);
            info.Post = NullIfEmpty(SelectedPostTB.Text);
            info.Position = BlankDash(OragPlaceCombo.Text);
            profile.Position = info.Post ?? info.Position;
            info.InsuranceType = BlankDash(InsuranceTypeCombo.Text);
            info.CooperationType = BlankDash(CooperationTypeCombo.Text);

            info.StartDate = CooprationStartDateDP.SelectedDate;
            info.EndDate = CooprationEndDateDP.SelectedDate;

            info.Salary = decimal.TryParse(SalaryTB.Text?.Trim(),
                NumberStyles.Number, CultureInfo.InvariantCulture, out var salary)
                ? salary
                : null;

            info.ResumePath = _resumePath;
        }

        public bool Validate(out string message)
        {
            var start = CooprationStartDateDP.SelectedDate;
            var end = CooprationEndDateDP.SelectedDate;

            if (start.HasValue && end.HasValue && end < start)
            {
                message = "تاریخ پایان همکاری نمی‌تواند قبل از تاریخ شروع باشد.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(SalaryTB.Text) &&
                !decimal.TryParse(SalaryTB.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out _))
            {
                message = "حقوق باید عددی باشد.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private static void SelectText(ComboBox combo, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) { combo.SelectedIndex = 0; return; }

            foreach (var item in combo.Items)
            {
                if (string.Equals(item?.ToString(), value, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedItem = item;
                    return;
                }
            }

            combo.Text = value;
        }

        private static string? NullIfEmpty(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        private static string? BlankDash(string? value)
        {
            var text = value?.Trim();
            return string.IsNullOrWhiteSpace(text) || text == "—" ? null : text;
        }
    }
}