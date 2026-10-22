using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;

namespace HR.Controls.AddEmployees
{
    public partial class EducationInfo : UserControl, IEmployeeSection
    {
        private readonly ObservableCollection<EducationRecord> _items = new();
        private string _certificateImage = string.Empty;

        public string SectionTitle => "مشخصات تحصیلی";

        public EducationInfo()
        {
            InitializeComponent();

            PersonalEducationsList.ItemsSource = _items;
            _items.CollectionChanged += (_, __) => RowNumbering.Renumber(_items);

            EducationalStatusCombo.ItemsSource = new[]
            {
                new ComboItem(EducationStatusKind.Studying,   "در حال تحصیل"),
                new ComboItem(EducationStatusKind.Graduated,  "فارغ‌التحصیل"),
                new ComboItem(EducationStatusKind.DroppedOut, "انصراف"),
                new ComboItem(EducationStatusKind.OnHold,     "مرخصی تحصیلی"),
            };
            Setup(EducationalStatusCombo);

            GradeCombo.ItemsSource = new[]
            {
                new ComboItem(DegreeLevel.Diploma,       "دیپلم"),
                new ComboItem(DegreeLevel.Associate,     "کاردانی"),
                new ComboItem(DegreeLevel.Bachelor,      "کارشناسی"),
                new ComboItem(DegreeLevel.Master,        "کارشناسی ارشد"),
                new ComboItem(DegreeLevel.Doctorate,     "دکتری"),
                new ComboItem(DegreeLevel.PostDoctorate, "پس‌دکتری"),
            };
            Setup(GradeCombo);

            EducationStateNameTB.ItemsSource = Provinces;
            EducationStateNameTB.SelectedIndex = 0;
            EducationalCityNameTB.ItemsSource = new[] { "—" };
            EducationalCityNameTB.SelectedIndex = 0;
        }

        private static readonly string[] Provinces =
        {
            "تهران", "البرز", "اصفهان", "فارس", "خراسان رضوی", "آذربایجان شرقی",
            "آذربایجان غربی", "خوزستان", "گیلان", "مازندران", "کرمان", "یزد",
            "قم", "قزوین", "مرکزی", "همدان", "کرمانشاه", "گلستان", "سایر"
        };

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
            if (start.HasValue && end.HasValue && end < start)
            {
                Warn("تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد.");
                return;
            }

            double? gpa = null;
            if (!string.IsNullOrWhiteSpace(EducationalMarksAvgTB.Text))
            {
                if (!double.TryParse(EducationalMarksAvgTB.Text.Trim(), out var parsed) || parsed < 0 || parsed > 20)
                {
                    Warn("معدل باید عددی بین ۰ تا ۲۰ باشد.");
                    EducationalMarksAvgTB.Focus();
                    return;
                }
                gpa = parsed;
            }

            _items.Add(new EducationRecord
            {
                Degree = GradeCombo.SelectedValue is DegreeLevel degree ? degree : DegreeLevel.Bachelor,
                Field = field,
                University = university,
                State = EducationStateNameTB.SelectedItem as string,
                City = EducationalCityNameTB.SelectedItem as string,
                Status = EducationalStatusCombo.SelectedValue is EducationStatusKind status
                    ? status
                    : EducationStatusKind.Studying,
                Gpa = gpa,
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

        private void PersonalEducationsList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
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