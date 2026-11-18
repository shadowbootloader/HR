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
        private Guid _selfId;
        private Guid? _pendingManager;
        private EmployeeProfile? _pendingProfile;
        private bool _applying;

        private sealed class PersonPick
        {
            public Guid Id { get; init; }
            public string Label { get; init; } = string.Empty;
        }

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

            if (InsuranceTypeCombo.Items.Count > 0 && InsuranceTypeCombo.SelectedIndex < 0) InsuranceTypeCombo.SelectedIndex = 0;
            if (CooperationTypeCombo.Items.Count > 0 && CooperationTypeCombo.SelectedIndex < 0) CooperationTypeCombo.SelectedIndex = 0;
            if (DepartmentCombo.Items.Count > 0 && DepartmentCombo.SelectedIndex < 0) DepartmentCombo.SelectedIndex = 0;
            BindManagers(Array.Empty<PersonPick>(), null);
            ApplyPending();
        }

        private async void DepartmentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_applying) return;
            var title = BlankDash(DepartmentCombo.SelectedItem?.ToString() ?? DepartmentCombo.Text);
            if (title == null)
            {
                BindManagers(Array.Empty<PersonPick>(), null);
                return;
            }

            try
            {
                using var departments = new DepartmentService();
                var department = (await departments.GetAllAsync()).FirstOrDefault(item => item.Title == title);
                if (department == null)
                {
                    BindManagers(Array.Empty<PersonPick>(), null);
                    return;
                }

                using var people = new EmployeeService();
                var options = await people.ListDirectManagerOptionsAsync(department.DepartmentId, _selfId == Guid.Empty ? null : _selfId);
                var selected = _pendingManager;
                _pendingManager = null;
                BindManagers(options.Select(item => new PersonPick { Id = item.EmployeeId, Label = item.Label }), selected);
            }
            catch (Exception ex)
            {
                NotificationService.Warning("مدیر مستقیم", ex.Message);
            }
        }

        private void BindManagers(IEnumerable<PersonPick> people, Guid? selected)
        {
            var items = new List<PersonPick> { new() { Id = Guid.Empty, Label = "—" } };
            items.AddRange(people);
            DirectManagerCombo.ItemsSource = items;
            DirectManagerCombo.DisplayMemberPath = nameof(PersonPick.Label);
            DirectManagerCombo.SelectedValuePath = nameof(PersonPick.Id);
            DirectManagerCombo.SelectedValue = selected is Guid id && items.Any(item => item.Id == id) ? id : Guid.Empty;
        }

        private void ApplyPending()
        {
            if (_pendingProfile == null || DepartmentCombo.Items.Count == 0) return;
            _applying = true;
            SelectText(DepartmentCombo, _pendingProfile.DepartmentName);
            _selfId = _pendingProfile.EmployeeKey;
            _pendingManager = _pendingProfile.DirectManagerEmployeeId;
            _applying = false;
            DepartmentCombo_SelectionChanged(DepartmentCombo, null!);
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
            _pendingProfile = profile;
            _selfId = profile.EmployeeKey;

            PersonnelCodeTB.Text = profile.PersonnelCode;
            ApplyPending();
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
            profile.DirectManagerEmployeeId = DirectManagerCombo.SelectedValue is Guid manager && manager != Guid.Empty
                ? manager
                : null;
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