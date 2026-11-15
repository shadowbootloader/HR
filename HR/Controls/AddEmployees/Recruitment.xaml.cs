using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;
using HR.Services.Data;

namespace HR.Controls.AddEmployees
{
    public partial class Recruitment : UserControl, IEmployeeSection
    {
        private const string NoneItem = "—";

        private readonly SectionList<RecruitmentRecord> _list = new();

        public string SectionTitle => "استخدام";

        public Recruitment()
        {
            InitializeComponent();
            _list.Attach(PersonalEmploymentList);

            Loaded += Recruitment_Loaded;
        }

        private async void Recruitment_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Recruitment_Loaded;

            try
            {
                var cache = await LookupCache.GetAsync();

                Fill(Employment_OrganTypeCombo, cache[LookupKeys.SectorType].Select(x => x.Title).ToList(), withNone: false);
                Fill(Employment_ResaltCombo, cache[LookupKeys.HiringResult].Select(x => x.Title).ToList(), withNone: true);
            }
            catch
            {
                // دیتابیس در دسترس نیست → لیست خالی می‌ماند
            }
        }

        private static void Fill(ComboBox combo, List<string> items, bool withNone)
        {
            if (items.Count == 0) return;

            var source = withNone ? new[] { NoneItem }.Concat(items).ToList() : items;

            combo.ItemsSource = source;
            combo.SelectedIndex = 0;
        }

        private void AddEmploymentDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var name = Employment_OrganNameTB.Text?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                Warn("نام مرکز را وارد کنید.");
                Employment_OrganNameTB.Focus();
                return;
            }

            var result = Employment_ResaltCombo.SelectedItem as string;

            _list.Items.Add(new RecruitmentRecord
            {
                InstitutionName = name,
                InstitutionType = Employment_OrganTypeCombo.SelectedItem as string ?? string.Empty,
                Result = string.IsNullOrWhiteSpace(result) || result == NoneItem ? null : result.Trim(),
                Note = NullIfEmpty(Employment_NoteTB.Text)
            });

            ClearForm();
        }

        private void DeleteEmploymentDataBtn_Click(object sender, RoutedEventArgs e) => _list.DeleteSelected();
        private void PersonalEmploymentList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        public void LoadFrom(EmployeeProfile profile) => _list.Replace(profile.Recruitments);
        public void SaveTo(EmployeeProfile profile) => _list.CopyTo(profile.Recruitments);

        public bool Validate(out string message) { message = string.Empty; return true; }

        private void ClearForm()
        {
            Employment_OrganNameTB.Clear();
            Employment_NoteTB.Clear();
            if (Employment_OrganTypeCombo.Items.Count > 0) Employment_OrganTypeCombo.SelectedIndex = 0;
            if (Employment_ResaltCombo.Items.Count > 0) Employment_ResaltCombo.SelectedIndex = 0;
            Employment_OrganNameTB.Focus();
        }

        private static string? NullIfEmpty(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        private static void Warn(string t)
            => MessageBox.Show(t, "استخدام", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}