using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HR.Data.Entities;
using HR.Models;
using HR.Services;
using HR.Services.Data;

namespace HR.Controls.AddEmployees
{
    public partial class EducationInfo : UserControl, IEmployeeSection
    {
        private readonly ObservableCollection<EducationRecord> _items = new();
        private readonly List<Geo> _provinces = new();
        private string _certificateImage = string.Empty;

        public string SectionTitle => "مشخصات تحصیلی";

        public EducationInfo()
        {
            InitializeComponent();

            PersonalEducationsList.ItemsSource = _items;
            _items.CollectionChanged += (_, __) => RowNumbering.Renumber(_items);

            Setup(EducationalStatusCombo);
            Setup(GradeCombo);

            EducationalCityNameTB.ItemsSource = new[] { "—" };
            EducationalCityNameTB.SelectedIndex = 0;

            EducationStateNameTB.SelectionChanged += EducationStateNameTB_SelectionChanged;

            Loaded += EducationInfo_Loaded;
        }

        private async void EducationInfo_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= EducationInfo_Loaded;

            await LookupBinder.FillAsync<EducationStatusKind>(EducationalStatusCombo, LookupKeys.EducationStatus);
            await LookupBinder.FillAsync<DegreeLevel>(GradeCombo, LookupKeys.EducationDegree);

            await FillProvincesAsync();
        }

        private async void EducationStateNameTB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await FillCitiesAsync();
        }

        // ==================== موقعیت جغرافیایی ====================

        private async System.Threading.Tasks.Task FillProvincesAsync()
        {
            try
            {
                using var geo = new GeoService();
                var provinces = await geo.GetProvincesAsync();
                if (provinces.Count == 0) return;

                _provinces.Clear();
                _provinces.AddRange(provinces);

                EducationStateNameTB.ItemsSource = provinces.Select(p => p.Name).ToList();
                EducationStateNameTB.SelectedIndex = 0;
            }
            catch
            {
                // دیتابیس در دسترس نیست → لیست خالی می‌ماند
            }
        }

        private async System.Threading.Tasks.Task FillCitiesAsync()
        {
            var province = _provinces.FirstOrDefault(p =>
                p.Name == EducationStateNameTB.SelectedItem as string);

            if (province == null)
            {
                EducationalCityNameTB.ItemsSource = new[] { "—" };
                EducationalCityNameTB.SelectedIndex = 0;
                return;
            }

            try
            {
                using var geo = new GeoService();
                var cities = await geo.GetCitiesAsync(province.GeoId);

                var names = cities.Select(c => c.Name).ToList();
                EducationalCityNameTB.ItemsSource = names.Count > 0 ? names : new List<string> { "—" };
                EducationalCityNameTB.SelectedIndex = 0;
            }
            catch
            {
                EducationalCityNameTB.ItemsSource = new[] { "—" };
                EducationalCityNameTB.SelectedIndex = 0;
            }
        }

        private static void Setup(ComboBox combo)
        {
            combo.DisplayMemberPath = nameof(ComboItem.Text);
            combo.SelectedValuePath = nameof(ComboItem.Value);
            combo.SelectedIndex = 0;
        }

        // ==================== رویدادها ====================

        private void AddEducationDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var field = FieldNameTB.Text?.Trim();
            var university = UniversityNameTB.Text?.Trim();

            if (string.IsNullOrWhiteSpace(field))
            {
                Warn("رشته تحصیلی را وارد کنید.");
                FieldNameTB.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(university))
            {
                Warn("نام موسسه را وارد کنید.");
                UniversityNameTB.Focus();
                return;
            }

            var start = EducationStartDateDP.SelectedDate;
            var end = EducationEndDateDP.SelectedDate;
            if (start == null)
            {
                Warn("تاریخ شروع تحصیل را انتخاب کنید.");
                return;
            }

            if (start.HasValue && end.HasValue && end < start)
            {
                Warn("تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد.");
                return;
            }

            if (!double.TryParse(EducationalMarksAvgTB.Text?.Trim(), out var parsed) || parsed < 0 || parsed > 20)
            {
                Warn("معدل باید عددی بین ۰ تا ۲۰ باشد.");
                EducationalMarksAvgTB.Focus();
                return;
            }

            SectionList<EducationRecord>.Commit(_items, PersonalEducationsList, new EducationRecord
            {
                Degree = GradeCombo.SelectedValue is DegreeLevel degree ? degree : DegreeLevel.Bachelor,
                DegreeTitle = LookupBinder.SelectedTitle(GradeCombo),
                StatusTitle = LookupBinder.SelectedTitle(EducationalStatusCombo),
                Field = field,
                University = university,
                State = EducationStateNameTB.SelectedItem as string,
                City = EducationalCityNameTB.SelectedItem as string,
                Status = EducationalStatusCombo.SelectedValue is EducationStatusKind status
                    ? status
                    : EducationStatusKind.Studying,
                Gpa = parsed,
                StartDate = start,
                EndDate = end,
                ThesisTitle = NullIfEmpty(ResaleTB.Text),
                CertificateImagePath = _certificateImage
            });

            ClearForm();
        }

        private void DeleteEducationDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = PersonalEducationsList.SelectedItems.Cast<EducationRecord>().ToList();
            foreach (var item in selected)
                _items.Remove(item);
        }

        private void EducationalCertificateImageBtn_Click(object sender, RoutedEventArgs e)
        {
            var picked = ImagePicker.Pick();
            if (picked == null) return;

            _certificateImage = picked;
            ImagePicker.Preview(EducationalCertificateImage, _certificateImage);
        }

        private async void PersonalEducationsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PersonalEducationsList.SelectedItem is not EducationRecord item) return;

            GradeCombo.SelectedValue = item.Degree;
            if (!string.IsNullOrWhiteSpace(item.DegreeTitle))
                LookupBinder.Select(GradeCombo, item.DegreeTitle);
            FieldNameTB.Text = item.Field;
            UniversityNameTB.Text = item.University;
            EducationStateNameTB.SelectionChanged -= EducationStateNameTB_SelectionChanged;
            LookupBinder.Select(EducationStateNameTB, item.State);
            EducationStateNameTB.SelectionChanged += EducationStateNameTB_SelectionChanged;
            await FillCitiesAsync();
            LookupBinder.Select(EducationalCityNameTB, item.City);
            EducationalStatusCombo.SelectedValue = item.Status;
            if (!string.IsNullOrWhiteSpace(item.StatusTitle))
                LookupBinder.Select(EducationalStatusCombo, item.StatusTitle);
            EducationalMarksAvgTB.Text = item.Gpa?.ToString() ?? string.Empty;
            EducationStartDateDP.SelectedDate = item.StartDate;
            EducationEndDateDP.SelectedDate = item.EndDate;
            ResaleTB.Text = item.ThesisTitle ?? string.Empty;
            _certificateImage = item.CertificateImagePath;
            ImagePicker.Preview(EducationalCertificateImage, _certificateImage);
        }
        private void PersonalEducationsList_SizeChanged(object sender, SizeChangedEventArgs e) { }

        // ==================== IEmployeeSection ====================

        public void LoadFrom(EmployeeProfile profile)
        {
            _items.Clear();
            foreach (var record in profile.Educations)
                _items.Add(record);

            RowNumbering.Renumber(_items);
            _certificateImage = string.Empty;
            ImagePicker.Preview(EducationalCertificateImage, null);
        }

        public void SaveTo(EmployeeProfile profile)
        {
            profile.Educations.Clear();
            foreach (var record in _items)
                profile.Educations.Add(record);
        }

        public bool Validate(out string message)
        {
            foreach (var record in _items)
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

        // ==================== کمکی‌ها ====================

        private void ClearForm()
        {
            FieldNameTB.Clear();
            UniversityNameTB.Clear();
            EducationalMarksAvgTB.Clear();
            ResaleTB.Clear();
            EducationStartDateDP.SelectedDate = null;
            EducationEndDateDP.SelectedDate = null;
            EducationalStatusCombo.SelectedIndex = 0;
            GradeCombo.SelectedIndex = 0;
            _certificateImage = string.Empty;
            ImagePicker.Preview(EducationalCertificateImage, null);
            FieldNameTB.Focus();
        }

        private static string? NullIfEmpty(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static void Warn(string text)
            => MessageBox.Show(text, "مشخصات تحصیلی", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}