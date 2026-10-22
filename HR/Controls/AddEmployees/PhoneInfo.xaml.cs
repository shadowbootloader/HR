using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HR.Models;

namespace HR.Controls.AddEmployees
{
    public partial class PhoneInfo : UserControl, IEmployeeSection
    {
        private readonly ObservableCollection<PhoneRecord> _items = new();

        public string SectionTitle => "اطلاعات تماس";

        public PhoneInfo()
        {
            InitializeComponent();

            PersonalPhoneList.ItemsSource = _items;
            _items.CollectionChanged += (_, __) => RowNumbering.Renumber(_items);

            PhoneStatusCombo.ItemsSource = new[]
            {
                new ComboItem(PhoneKind.Mobile,    "همراه"),
                new ComboItem(PhoneKind.Home,      "منزل"),
                new ComboItem(PhoneKind.Work,      "کاری"),
                new ComboItem(PhoneKind.Emergency, "اضطراری"),
                new ComboItem(PhoneKind.Other,     "سایر"),
            };
            PhoneStatusCombo.DisplayMemberPath = nameof(ComboItem.Text);
            PhoneStatusCombo.SelectedValuePath = nameof(ComboItem.Value);
            PhoneStatusCombo.SelectedIndex = 0;
        }

        // ==================== رویدادها ====================

        private void AddPhoneBtn_Click(object sender, RoutedEventArgs e)
        {
            var number = FieldPhoneNumberTB.Text?.Trim();

            if (string.IsNullOrWhiteSpace(number))
            {
                Warn("شماره تماس را وارد کنید.");
                FieldPhoneNumberTB.Focus();
                return;
            }

            _items.Add(new PhoneRecord
            {
                Number = number,
                Owner = NullIfEmpty(OwnerPhoneTB.Text),
                Kind = PhoneStatusCombo.SelectedValue is PhoneKind kind ? kind : PhoneKind.Mobile,
                StartDate = PhoneStartDateDP.SelectedDate,
                EndDate = PhoneEndDateDP.SelectedDate,
                Note = NullIfEmpty(PhoneNumberNoteTB.Text),
                IsPrimary = _items.Count == 0
            });

            ClearForm();
        }

        private void DeletePhoneDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = PersonalPhoneList.SelectedItems.Cast<PhoneRecord>().ToList();
            if (selected.Count == 0) return;

            foreach (var item in selected)
                _items.Remove(item);

            EnsurePrimary();
        }

        // ==================== IEmployeeSection ====================

        public void LoadFrom(EmployeeProfile profile)
        {
            _items.Clear();
            foreach (var phone in profile.Phones)
                _items.Add(phone);

            RowNumbering.Renumber(_items);
            EnsurePrimary();
        }

        public void SaveTo(EmployeeProfile profile)
        {
            profile.Phones.Clear();
            foreach (var phone in _items)
                profile.Phones.Add(phone);
        }

        public bool Validate(out string message)
        {
            var duplicate = _items
                .GroupBy(p => p.Number)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate != null)
            {
                message = $"شماره تکراری: {duplicate.Key}";
                return false;
            }

            if (_items.Count(p => p.IsPrimary) > 1)
            {
                message = "بیش از یک شماره به‌عنوان «اصلی» علامت خورده است.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        // ==================== کمکی‌ها ====================

        private void EnsurePrimary()
        {
            if (_items.Count == 0 || _items.Any(p => p.IsPrimary)) return;
            _items[0].IsPrimary = true;
        }

        private void ClearForm()
        {
            FieldPhoneNumberTB.Clear();
            OwnerPhoneTB.Clear();
            PhoneNumberNoteTB.Clear();
            PhoneStartDateDP.SelectedDate = null;
            PhoneEndDateDP.SelectedDate = null;
            PhoneStatusCombo.SelectedIndex = 0;
            FieldPhoneNumberTB.Focus();
        }

        private static string? NullIfEmpty(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static void Warn(string text)
            => MessageBox.Show(text, "اطلاعات تماس", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}