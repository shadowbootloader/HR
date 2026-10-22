using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;

namespace HR.Controls.AddEmployees
{
    public partial class ForeignRelatives : UserControl, IEmployeeSection
    {
        private readonly SectionList<ForeignRelativeRecord> _list = new();

        public string SectionTitle => "بستگان خارجی";

        public ForeignRelatives()
        {
            InitializeComponent();
            _list.Attach(PersonalForeignFamilyList);

            ForeignFamily_CountryNameCombo.ItemsSource = Lookups.Countries;
            ForeignFamily_CountryNameCombo.SelectedIndex = 0;
        }

        private void AddForeignFamilyDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var name = ForeignFamily_FamilyNameCombo.Text?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                Warn("نام و نام خانوادگی را وارد کنید.");
                return;
            }

            _list.Items.Add(new ForeignRelativeRecord
            {
                FullName = name,
                Country = ForeignFamily_CountryNameCombo.Text?.Trim() ?? string.Empty,
                DepartureDate = ForeignFamily_StartDateDP.SelectedDate,
                Reason = NullIfEmpty(ForeignFamily_ReasonTB.Text),
                Note = NullIfEmpty(ForeignFamily_NoteTB.Text)
            });

            ClearForm();
        }

        private void DeleteForeignFamilyDataBtn_Click(object sender, RoutedEventArgs e) => _list.DeleteSelected();
        private void PersonalForeignFamilyList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        public void LoadFrom(EmployeeProfile profile) => _list.Replace(profile.ForeignRelatives);
        public void SaveTo(EmployeeProfile profile) => _list.CopyTo(profile.ForeignRelatives);

        public bool Validate(out string message) { message = string.Empty; return true; }

        private void ClearForm()
        {
            ForeignFamily_FamilyNameCombo.Text = string.Empty;
            ForeignFamily_ReasonTB.Clear();
            ForeignFamily_NoteTB.Clear();
            ForeignFamily_StartDateDP.SelectedDate = null;
            ForeignFamily_CountryNameCombo.SelectedIndex = 0;
        }

        private static string? NullIfEmpty(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        private static void Warn(string t)
            => MessageBox.Show(t, "بستگان خارجی", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}