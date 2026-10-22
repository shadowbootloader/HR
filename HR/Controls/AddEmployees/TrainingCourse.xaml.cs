using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;

namespace HR.Controls.AddEmployees
{
    public partial class TrainingCourse : UserControl, IEmployeeSection
    {
        private readonly ObservableCollection<TrainingCourseRecord> _items = new();
        private string _certificateImage = string.Empty;

        public string SectionTitle => "دوره‌های آموزشی";

        public TrainingCourse()
        {
            InitializeComponent();

            PersonalCourseList.ItemsSource = _items;
            _items.CollectionChanged += (_, __) => RowNumbering.Renumber(_items);
        }

        private void AddCourseDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var title = EducationalInstitutionTitleTB.Text?.Trim();
            var institute = EducationalInstitutionNameTB.Text?.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                Warn("عنوان دوره را وارد کنید.");
                EducationalInstitutionTitleTB.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(institute))
            {
                Warn("نام موسسه / مرکز آموزشی را وارد کنید.");
                EducationalInstitutionNameTB.Focus();
                return;
            }

            _items.Add(new TrainingCourseRecord
            {
                Title = title,
                Institute = institute,
                DurationText = NullIfEmpty(EducationalInstitutionTimeTB.Text),
                Note = NullIfEmpty(EducationalInstitutionNoteTB.Text),
                CertificateImagePath = _certificateImage
            });

            ClearForm();
        }

        private void DeleteCourseDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = PersonalCourseList.SelectedItems.Cast<TrainingCourseRecord>().ToList();
            foreach (var item in selected)
                _items.Remove(item);
        }

        private void CourseCertificateImageBtn_Click(object sender, RoutedEventArgs e)
        {
            var picked = ImagePicker.Pick();
            if (picked == null) return;

            _certificateImage = picked;
            ImagePicker.Preview(CourseCertificateImage, _certificateImage);
        }

        private void PersonalCourseList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void PersonalCourseList_SizeChanged(object sender, SizeChangedEventArgs e) { }

        // ==================== IEmployeeSection ====================

        public void LoadFrom(EmployeeProfile profile)
        {
            _items.Clear();
            foreach (var record in profile.Courses)
                _items.Add(record);

            RowNumbering.Renumber(_items);
            _certificateImage = string.Empty;
            ImagePicker.Preview(CourseCertificateImage, null);
        }

        public void SaveTo(EmployeeProfile profile)
        {
            profile.Courses.Clear();
            foreach (var record in _items)
                profile.Courses.Add(record);
        }

        public bool Validate(out string message)
        {
            message = string.Empty;
            return true;
        }

        private void ClearForm()
        {
            EducationalInstitutionTitleTB.Clear();
            EducationalInstitutionNameTB.Clear();
            EducationalInstitutionTimeTB.Clear();
            EducationalInstitutionNoteTB.Clear();
            _certificateImage = string.Empty;
            ImagePicker.Preview(CourseCertificateImage, null);
            EducationalInstitutionTitleTB.Focus();
        }

        private static string? NullIfEmpty(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static void Warn(string text)
            => MessageBox.Show(text, "دوره‌های آموزشی", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}