using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using HR.Models;

namespace HR.Controls.AddEmployees
{
    public partial class Family : UserControl, IEmployeeSection
    {
        private readonly ObservableCollection<FamilyMemberRecord> _items = new();

        public string SectionTitle => "بستگان درجه یک";

        public Family()
        {
            InitializeComponent();

            PersonalFamilyList.ItemsSource = _items;
            _items.CollectionChanged += (_, __) => RowNumbering.Renumber(_items);

            FamilyRelationCombo.SelectedIndex = 0;
        }

        // ==================== رویدادها ====================

        private void AddFamilyDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var firstName = FamilyNameTB.Text?.Trim();
            var lastName = Family_FamilyNameTB.Text?.Trim();

            if (string.IsNullOrWhiteSpace(firstName))
            {
                Warn("نام را وارد کنید.");
                FamilyNameTB.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                Warn("نام خانوادگی را وارد کنید.");
                Family_FamilyNameTB.Focus();
                return;
            }

            var nationalCode = Family_NationalIDNumberTB.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(nationalCode) && !Regex.IsMatch(nationalCode, @"^\d{10}$"))
            {
                Warn("کد ملی باید ۱۰ رقم باشد.");
                Family_NationalIDNumberTB.Focus();
                return;
            }

            var phone = Family_PhoneNumberTB.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(phone) && !Regex.IsMatch(phone, @"^0\d{10}$"))
            {
                Warn("تلفن همراه باید ۱۱ رقم و با ۰ شروع شود.");
                Family_PhoneNumberTB.Focus();
                return;
            }

            _items.Add(new FamilyMemberRecord
            {
                FirstName = firstName,
                LastName = lastName,
                Relation = (FamilyRelationCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "سایر",
                FatherName = NullIfEmpty(Family_FatherNameTB.Text),
                NationalCode = NullIfEmpty(nationalCode),
                PhoneNumber = NullIfEmpty(phone),
                Note = NullIfEmpty(Family_NoteTB.Text)
            });

            ClearForm();
        }

        private void DeleteFamilyDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = PersonalFamilyList.SelectedItems.Cast<FamilyMemberRecord>().ToList();
            foreach (var item in selected)
                _items.Remove(item);
        }

        // ==================== IEmployeeSection ====================

        public void LoadFrom(EmployeeProfile profile)
        {
            _items.Clear();
            foreach (var member in profile.Family)
                _items.Add(member);

            RowNumbering.Renumber(_items);
        }

        public void SaveTo(EmployeeProfile profile)
        {
            profile.Family.Clear();
            foreach (var member in _items)
                profile.Family.Add(member);
        }

        public bool Validate(out string message)
        {
            var duplicate = _items
                .Where(m => !string.IsNullOrWhiteSpace(m.NationalCode))
                .GroupBy(m => m.NationalCode)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate != null)
            {
                message = $"کد ملی تکراری: {duplicate.Key}";
                return false;
            }

            message = string.Empty;
            return true;
        }

        // ==================== کمکی‌ها ====================

        private void ClearForm()
        {
            FamilyNameTB.Clear();
            Family_FamilyNameTB.Clear();
            Family_FatherNameTB.Clear();
            Family_NationalIDNumberTB.Clear();
            Family_PhoneNumberTB.Clear();
            Family_NoteTB.Clear();
            FamilyRelationCombo.SelectedIndex = 0;
            FamilyNameTB.Focus();
        }

        private static string? NullIfEmpty(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static void Warn(string text)
            => MessageBox.Show(text, "بستگان درجه یک", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}