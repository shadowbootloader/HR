using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;
using HR.Services.Data;

namespace HR.Controls.AddEmployees
{
    public partial class Criminal : UserControl, IEmployeeSection
    {
        private readonly SectionList<CriminalRecord> _list = new();

        public string SectionTitle => "سابقه کیفری - سیاسی";

        public Criminal()
        {
            InitializeComponent();
            _list.Attach(PersonalCriminalList);

            Loaded += Criminal_Loaded;
        }

        private async void Criminal_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Criminal_Loaded;

            try
            {
                var types = (await LookupCache.GetAsync())[LookupKeys.RecordType];
                if (types.Count == 0) return;

                Criminal_TypeCombo.ItemsSource = types.Select(t => t.Title).ToList();
                Criminal_TypeCombo.SelectedIndex = 0;
            }
            catch
            {
                // دیتابیس در دسترس نیست → لیست خالی می‌ماند
            }
        }

        private void AddCriminalDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var title = Criminal_CriminalTitleTB.Text?.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                Warn("عنوان سابقه را وارد کنید.");
                Criminal_CriminalTitleTB.Focus();
                return;
            }

            _list.Commit(new CriminalRecord
            {
                PersonName = Criminal_FamilyNameCombo.Text?.Trim() ?? string.Empty,
                RecordType = Criminal_TypeCombo.SelectedItem as string
                             ?? Criminal_TypeCombo.Text?.Trim()
                             ?? string.Empty,
                Title = title,
                Note = NullIfEmpty(Criminal_NoteTB.Text)
            });

            ClearForm();
        }

        private void DeleteCriminalDataBtn_Click(object sender, RoutedEventArgs e) => _list.DeleteSelected();
        private void PersonalCriminalList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PersonalCriminalList.SelectedItem is not CriminalRecord item) return;
            Criminal_FamilyNameCombo.Text = item.PersonName;
            LookupBinder.Select(Criminal_TypeCombo, item.RecordType);
            Criminal_CriminalTitleTB.Text = item.Title;
            Criminal_NoteTB.Text = item.Note ?? string.Empty;
        }

        public void LoadFrom(EmployeeProfile profile) => _list.Replace(profile.CriminalRecords);
        public void SaveTo(EmployeeProfile profile) => _list.CopyTo(profile.CriminalRecords);

        public bool Validate(out string message) { message = string.Empty; return true; }

        private void ClearForm()
        {
            Criminal_CriminalTitleTB.Clear();
            Criminal_NoteTB.Clear();
            Criminal_FamilyNameCombo.Text = string.Empty;
            if (Criminal_TypeCombo.Items.Count > 0) Criminal_TypeCombo.SelectedIndex = 0;
        }

        private static string? NullIfEmpty(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        private static void Warn(string t)
            => MessageBox.Show(t, "سابقه کیفری - سیاسی", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}