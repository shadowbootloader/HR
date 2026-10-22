using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;

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
            PersonalMaritalStatusTb.ItemsSource = new[]
            {
                new ComboItem(MaritalStatus.Single,   "مجرد"),
                new ComboItem(MaritalStatus.Married,  "متأهل"),
                new ComboItem(MaritalStatus.Divorced, "مطلقه"),
                new ComboItem(MaritalStatus.Widowed,  "همسر فوت‌شده"),
            };
            Setup(PersonalMaritalStatusTb);

            PersonalSarbaziTb.ItemsSource = new[]
            {
                new ComboItem(MilitaryStatus.Done,          "پایان خدمت"),
                new ComboItem(MilitaryStatus.InProgress,    "در حال خدمت"),
                new ComboItem(MilitaryStatus.Exempt,        "معاف"),
                new ComboItem(MilitaryStatus.NotApplicable, "مشمول نیست"),
            };
            Setup(PersonalSarbaziTb);

            PersonalMoafiatStatusTb.ItemsSource = new[]
            {
                new ComboItem("دائم", "دائم"),
                new ComboItem("موقت", "موقت"),
                new ComboItem("—",    "ندارد"),
            };
            Setup(PersonalMoafiatStatusTb);

            PersonalMoafiatTypeTb.ItemsSource = new[]
            {
                "پزشکی", "کفالت", "تحصیلی", "فرزند", "سایر"
            };
            PersonalMoafiatTypeTb.SelectedIndex = 0;
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
            ImagePicker.Preview(PhotoBrushTarget, _photo);
            PhotoEllipse.Visibility = Visibility.Visible;
        }

        /// <summary>ImageBrush جایی برای تنظیم Source ندارد، پس یک Image پشت آن می‌گذاریم.</summary>
        private Image? PhotoBrushTarget => FindName("PhotoPreviewImage") as Image;

        private void NationalCardImageBtn_Click(object sender, RoutedEventArgs e)
        {
            var picked = ImagePicker.Pick();
            if (picked == null) return;

            _nationalCardImage = picked;
            ImagePicker.Preview(NationalCardImage, _nationalCardImage);
        }

        private void IDImageBtn_Click(object sender, RoutedEventArgs e)
        {
            var picked = ImagePicker.Pick();
            if (picked == null) return;

            _idCardImage = picked;
            ImagePicker.Preview(IDImage, _idCardImage);
        }

        private void SarbaziImageBtn_Click(object sender, RoutedEventArgs e)
        {
            var picked = ImagePicker.Pick();
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

            SelectByValue(PersonalMaritalStatusTb, profile.MaritalStatus);
            SelectByValue(PersonalSarbaziTb, profile.MilitaryStatus);
            PersonalMoafiatTypeTb.Text = profile.ExemptionReason;

            _photo = profile.PhotoPath;
            _nationalCardImage = profile.NationalCardImagePath;
            _idCardImage = profile.IdCardImagePath;
            _militaryImage = profile.MilitaryImagePath;

            ImagePicker.Preview(PhotoBrushTarget, _photo);
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
            profile.ExemptionReason = PersonalMoafiatTypeTb.Text?.Trim() ?? string.Empty;

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
            if (!Regex.IsMatch(nationalCode, @"^\d{10}$"))
            { message = "کد ملی باید ۱۰ رقم باشد."; return false; }

            var idNumber = PersonalIDTb.Text?.Trim() ?? string.Empty;
            if (!Regex.IsMatch(idNumber, @"^\d{1,10}$"))
            { message = "شماره شناسنامه باید عددی و حداکثر ۱۰ رقم باشد."; return false; }

            if (PersonalBirthDateDP.SelectedDate == null)
            { message = "تاریخ تولد را انتخاب کنید."; return false; }

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
    }
}