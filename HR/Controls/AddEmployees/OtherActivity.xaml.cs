using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;

namespace HR.Controls.AddEmployees
{
    public partial class OtherActivity : UserControl, IEmployeeSection
    {
        private readonly SectionList<OtherActivityRecord> _list = new();

        public string SectionTitle => "سایر فعالیت‌ها";

        public OtherActivity()
        {
            InitializeComponent();
            _list.Attach(PersonalOtherActivityList);
        }

        private void AddOtherActivityDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var title = OtherActivity_NameTB.Text?.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                Warn("عنوان را وارد کنید.");
                OtherActivity_NameTB.Focus();
                return;
            }

            _list.Commit(new OtherActivityRecord
            {
                Title = title,
                Place = NullIfEmpty(OtherActivity_OptationPlaceTB.Text),
                Note = NullIfEmpty(OtherActivity_NoteTB.Text)
            });

            ClearForm();
        }

        private void DeleteOtherActivityDataBtn_Click(object sender, RoutedEventArgs e) => _list.DeleteSelected();
        private void PersonalOtherActivityList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PersonalOtherActivityList.SelectedItem is not OtherActivityRecord item) return;
            OtherActivity_NameTB.Text = item.Title;
            OtherActivity_OptationPlaceTB.Text = item.Place ?? string.Empty;
            OtherActivity_NoteTB.Text = item.Note ?? string.Empty;
        }

        public void LoadFrom(EmployeeProfile profile) => _list.Replace(profile.OtherActivities);
        public void SaveTo(EmployeeProfile profile) => _list.CopyTo(profile.OtherActivities);

        public bool Validate(out string message) { message = string.Empty; return true; }

        private void ClearForm()
        {
            OtherActivity_NameTB.Clear();
            OtherActivity_OptationPlaceTB.Clear();
            OtherActivity_NoteTB.Clear();
            OtherActivity_NameTB.Focus();
        }

        private static string? NullIfEmpty(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        private static void Warn(string t)
            => MessageBox.Show(t, "سایر فعالیت‌ها", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}