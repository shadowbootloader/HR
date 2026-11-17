using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;
using HR.Services.Data;

namespace HR.Controls.AddEmployees
{
    public partial class BasicInfo : UserControl, IEmployeeSection
    {
        private string _photo = string.Empty;
        private string _nationalCardImage = string.Empty;
        private string _idCardImage = string.Empty;
        private string _militaryImage = string.Empty;

        public string SectionTitle => "اطلاعات شخصی";

        public BasicInfo()
        {
            InitializeComponent();
            FillCombos();
        }

        private void FillCombos()
        {
            Setup(PersonalMaritalStatusTb);
            Setup(PersonalSarbaziTb);

            Setup(PersonalMoafiatStatusTb);
            Setup(PersonalMoafiatTypeTb);

            Loaded += BasicInfo_Loaded;
        }

        private async void BasicInfo_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BasicInfo_Loaded;

            await LookupBinder.FillAsync<MaritalStatus>(PersonalMaritalStatusTb, LookupKeys.MaritalStatus, valueOffset: 1);
            await LookupBinder.FillAsync<MilitaryStatus>(PersonalSarbaziTb, LookupKeys.MilitaryStatus, valueOffset: 1);
            await LookupBinder.FillTitlesAsync(PersonalMoafiatStatusTb, LookupKeys.ExemptionType, addEmptyItem: true);
            await LookupBinder.FillTitlesAsync(PersonalMoafiatTypeTb, LookupKeys.ExemptionReason, addEmptyItem: true);

            if (FindName("PersonalReligionCombo") is ComboBox religionCombo)
                await LookupBinder.FillTitlesAsync(religionCombo, LookupKeys.Religion, addEmptyItem: true);
            if (FindName("PersonalDenominationCombo") is ComboBox denominationCombo)
                await LookupBinder.FillTitlesAsync(denominationCombo, LookupKeys.Denomination, addEmptyItem: true);
        }

        private static void Setup(ComboBox combo)
        {
            combo.DisplayMemberPath = nameof(ComboItem.Text);
            combo.SelectedValuePath = nameof(ComboItem.Value);
            combo.SelectedIndex = 0;
        }

        // ==================== انتخاب تصاویر ====================

        private void UploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            var picked = ImagePicker.Pick();
            if (picked == null) return;

            _photo = picked;
            if (PhotoBrushTarget is Image photoTarget)
                ImagePicker.Preview(photoTarget, _photo);
            PhotoEllipse.Visibility = Visibility.Visible;
        }

        /// <summary>ImageBrush جایی برای تنظیم Source ندارد، پس یک Image پشت آن می‌گذاریم.</summary>
        private Image? PhotoBrushTarget => FindName("PhotoPreviewImage") as Image;

        private void NationalCardImageBtn_Click(object sender, RoutedEventArgs e)
        {
            var picked = ImagePicker.PickDocument();
            if (picked == null) return;

            _nationalCardImage = picked;
            ImagePicker.Preview(NationalCardImage, _nationalCardImage);
        }

        private void IDImageBtn_Click(object sender, RoutedEventArgs e)
        {
            var picked = ImagePicker.PickDocument();
            if (picked == null) return;

            _idCardImage = picked;
            ImagePicker.Preview(IDImage, _idCardImage);
        }

        private void SarbaziImageBtn_Click(object sender, RoutedEventArgs e)
        {
            var picked = ImagePicker.PickDocument();
            if (picked == null) return;

            _militaryImage = picked;
            ImagePicker.Preview(SarbaziImage, _militaryImage);
        }

        // ==================== IEmployeeSection ====================

        public void LoadFrom(EmployeeProfile profile)
        {
            PersonalFirstNameTb.Text = profile.FirstName;
            PersonalLastnameTb.Text = profile.LastName;
            PersonalFatherNameTb.Text = profile.FatherName;
            PersonalNationalCodeTb.Text = profile.NationalCode;
            PersonalIDTb.Text = profile.IdNumber;
            PersonalBirthPlaceTb.Text = profile.BirthPlace;
            PersonalMahaleSodurTb.Text = profile.IdIssuePlace;

            PersonalBirthDateDP.SelectedDate = profile.BirthDate;
            PersonalSodorDateDP.SelectedDate = profile.IdIssueDate;

            PersonalGenderList.SelectedIndex = profile.Gender == Gender.Female ? 1 : 0;

            SelectText(PersonalMaritalStatusTb, profile.MaritalStatusTitle);
            SelectText(PersonalSarbaziTb, profile.MilitaryStatusTitle);

            SelectText(PersonalMoafiatStatusTb, profile.ExemptionType);
            SelectText(PersonalMoafiatTypeTb, profile.ExemptionReason);
            SelectText(PersonalReligionCombo, profile.Religion);
            SelectText(PersonalDenominationCombo, profile.Denomination);
            PersonalEmailTb.Text = profile.Email;
            PersonalInsuranceTb.Text = profile.InsuranceNumber;
            PersonalResidenceTb.Text = profile.Residence;
            PersonalAddressTitleTb.Text = profile.AddressTitle;
            PersonalAddressTb.Text = profile.AddressText;
            PersonalPostalTb.Text = profile.PostalCode ?? string.Empty;

            _photo = profile.PhotoPath;
            _nationalCardImage = profile.NationalCardImagePath;
            _idCardImage = profile.IdCardImagePath;
            _militaryImage = profile.MilitaryImagePath;

            if (PhotoBrushTarget is Image loadedPhoto)
                ImagePicker.Preview(loadedPhoto, _photo);
            PhotoEllipse.Visibility = string.IsNullOrEmpty(_photo) ? Visibility.Collapsed : Visibility.Visible;

            ImagePicker.Preview(NationalCardImage, _nationalCardImage);
            ImagePicker.Preview(IDImage, _idCardImage);
            ImagePicker.Preview(SarbaziImage, _militaryImage);
        }

        public void SaveTo(EmployeeProfile profile)
        {
            profile.FirstName = PersonalFirstNameTb.Text.Trim();
            profile.LastName = PersonalLastnameTb.Text.Trim();
            profile.FatherName = PersonalFatherNameTb.Text.Trim();
            profile.NationalCode = PersonalNationalCodeTb.Text.Trim();
            profile.IdNumber = PersonalIDTb.Text.Trim();
            profile.BirthPlace = PersonalBirthPlaceTb.Text.Trim();
            profile.IdIssuePlace = PersonalMahaleSodurTb.Text.Trim();

            profile.BirthDate = PersonalBirthDateDP.SelectedDate;
            profile.IdIssueDate = PersonalSodorDateDP.SelectedDate;

            profile.Gender = PersonalGenderList.SelectedIndex == 1 ? Gender.Female : Gender.Male;
            profile.MaritalStatus = ValueOf(PersonalMaritalStatusTb, MaritalStatus.Unspecified);
            profile.MilitaryStatus = ValueOf(PersonalSarbaziTb, MilitaryStatus.Unspecified);
            profile.ExemptionType = ComboText(PersonalMoafiatStatusTb);
            profile.ExemptionReason = ComboText(PersonalMoafiatTypeTb);
            profile.Religion = ComboText(PersonalReligionCombo);
            profile.Denomination = ComboText(PersonalDenominationCombo);
            profile.MaritalStatusTitle = LookupBinder.SelectedTitle(PersonalMaritalStatusTb);
            profile.MilitaryStatusTitle = LookupBinder.SelectedTitle(PersonalSarbaziTb);
            profile.Email = PersonalEmailTb.Text.Trim();
            profile.InsuranceNumber = PersonalInsuranceTb.Text.Trim();
            profile.Residence = PersonalResidenceTb.Text.Trim();
            profile.AddressTitle = PersonalAddressTitleTb.Text.Trim();
            profile.AddressText = PersonalAddressTb.Text.Trim();
            profile.PostalCode = string.IsNullOrWhiteSpace(PersonalPostalTb.Text) ? null : PersonalPostalTb.Text.Trim();

            profile.PhotoPath = _photo;
            profile.NationalCardImagePath = _nationalCardImage;
            profile.IdCardImagePath = _idCardImage;
            profile.MilitaryImagePath = _militaryImage;
        }

        public bool Validate(out string message)
        {
            if (string.IsNullOrWhiteSpace(PersonalFirstNameTb.Text))
            { message = "نام را وارد کنید."; return false; }

            if (string.IsNullOrWhiteSpace(PersonalLastnameTb.Text))
            { message = "نام خانوادگی را وارد کنید."; return false; }

            var nationalCode = PersonalNationalCodeTb.Text?.Trim() ?? string.Empty;
            if (!NationalCode.IsValid(nationalCode))
            { message = "کد ملی نامعتبر است."; return false; }

            var idNumber = PersonalIDTb.Text?.Trim() ?? string.Empty;
            if (!Regex.IsMatch(idNumber, @"^\d{1,10}$"))
            { message = "شماره شناسنامه باید عددی و حداکثر ۱۰ رقم باشد."; return false; }

            if (PersonalBirthDateDP.SelectedDate == null)
            { message = "تاریخ تولد را انتخاب کنید."; return false; }

            if (PersonalSodorDateDP.SelectedDate == null)
            { message = "تاریخ صدور را انتخاب کنید."; return false; }

            var email = PersonalEmailTb.Text?.Trim() ?? string.Empty;
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            { message = "ایمیل معتبر وارد کنید."; return false; }

            var insurance = PersonalInsuranceTb.Text?.Trim() ?? string.Empty;
            if (!Regex.IsMatch(insurance, @"^\d{1,10}$"))
            { message = "شماره بیمه باید عددی و حداکثر ۱۰ رقم باشد."; return false; }

            var hasAddress = !string.IsNullOrWhiteSpace(PersonalAddressTb.Text);
            if (hasAddress && string.IsNullOrWhiteSpace(PersonalAddressTitleTb.Text))
            { message = "عنوان نشانی را وارد کنید."; return false; }

            var postal = PersonalPostalTb.Text?.Trim() ?? string.Empty;
            if (postal.Length > 0 && !Regex.IsMatch(postal, @"^\d{10}$"))
            { message = "کد پستی باید ۱۰ رقم باشد."; return false; }

            message = string.Empty;
            return true;
        }

        // ==================== کمکی‌ها ====================

        private static void SelectByValue<T>(ComboBox combo, T value) where T : struct
        {
            foreach (var item in combo.Items)
            {
                if (item is ComboItem ci && ci.Value is T typed && typed.Equals(value))
                {
                    combo.SelectedItem = ci;
                    return;
                }
            }
            combo.SelectedIndex = 0;
        }

        private static T ValueOf<T>(ComboBox combo, T fallback) where T : struct
            => combo.SelectedValue is T value ? value : fallback;


        private static void SelectText(ComboBox combo, string? value)
        {
            if (combo == null) return;
            var text = value?.Trim();
            if (string.IsNullOrEmpty(text) || text == "—")
            {
                if (combo.Items.Count > 0) combo.SelectedIndex = 0;
                return;
            }

            foreach (var item in combo.Items)
            {
                var label = item switch
                {
                    string s => s,
                    ComboBoxItem cbi => cbi.Content?.ToString(),
                    ComboItem ci => ci.Text,
                    _ => item?.ToString()
                };
                if (string.Equals(label, text, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedItem = item;
                    return;
                }
            }

            combo.Text = text;
        }

        private static string ComboText(ComboBox combo)
        {
            if (combo == null) return string.Empty;
            if (combo.SelectedItem is string s) return s.Trim();
            if (combo.SelectedItem is ComboBoxItem cbi) return cbi.Content?.ToString()?.Trim() ?? string.Empty;
            if (combo.SelectedItem is ComboItem ci) return ci.Text?.Trim() ?? string.Empty;
            return combo.Text?.Trim() ?? string.Empty;
        }

    }
}
