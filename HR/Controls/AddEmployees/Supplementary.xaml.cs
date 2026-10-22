using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;

namespace HR.Controls.AddEmployees
{
    public partial class Supplementary : UserControl, IEmployeeSection
    {
        private string _resumePath = string.Empty;

        public string SectionTitle => "اطلاعات تکمیلی";

        public Supplementary()
        {
            InitializeComponent();

            OragPlaceCombo.ItemsSource = Lookups.Positions;
            InsuranceTypeCombo.ItemsSource = Lookups.InsuranceTypes;
            CooperationTypeCombo.ItemsSource = Lookups.CooperationTypes;

            OragPlaceCombo.SelectedIndex = 0;
            InsuranceTypeCombo.SelectedIndex = 0;
            CooperationTypeCombo.SelectedIndex = 0;
        }

        private void CVImageBtn_Click(object sender, RoutedEventArgs e)
        {
            var picked = ImagePicker.Pick();
            if (picked == null) return;

            _resumePath = picked;
            ImagePicker.Preview(CVImage, _resumePath);
        }

        public void LoadFrom(EmployeeProfile profile)
        {
            var info = profile.Supplementary;

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

            info.Post = NullIfEmpty(SelectedPostTB.Text);
            info.Position = OragPlaceCombo.Text?.Trim();
            info.InsuranceType = InsuranceTypeCombo.Text?.Trim();
            info.CooperationType = CooperationTypeCombo.Text?.Trim();

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
    }
}