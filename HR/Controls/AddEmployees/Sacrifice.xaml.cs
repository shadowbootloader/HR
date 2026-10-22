using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;

namespace HR.Controls.AddEmployees
{
    public partial class Sacrifice : UserControl, IEmployeeSection
    {
        private readonly SectionList<SacrificeRecord> _list = new();

        public string SectionTitle => "ایثارگری";

        public Sacrifice()
        {
            InitializeComponent();
            _list.Attach(PersonalSacrificeList);

            Sacrifice_TypeCombo.ItemsSource = Lookups.SacrificeTypes;
            Sacrifice_TypeCombo.SelectedIndex = 0;
        }

        private void AddSacrificeDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var name = Sacrifice_FullNameTB.Text?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                Warn("نام و نام خانوادگی را وارد کنید.");
                Sacrifice_FullNameTB.Focus();
                return;
            }

            _list.Items.Add(new SacrificeRecord
            {
                FullName = name,
                SacrificeType = Sacrifice_TypeCombo.Text?.Trim() ?? string.Empty,
                Amount = NullIfEmpty(Sacrifice_MuchTB.Text),
                Note = NullIfEmpty(Sacrifice_NoteTB.Text)
            });

            ClearForm();
        }

        private void DeleteSacrificeDataBtn_Click(object sender, RoutedEventArgs e) => _list.DeleteSelected();
        private void PersonalSacrificeList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        public void LoadFrom(EmployeeProfile profile) => _list.Replace(profile.Sacrifices);
        public void SaveTo(EmployeeProfile profile) => _list.CopyTo(profile.Sacrifices);

        public bool Validate(out string message) { message = string.Empty; return true; }

        private void ClearForm()
        {
            Sacrifice_FullNameTB.Clear();
            Sacrifice_MuchTB.Clear();
            Sacrifice_NoteTB.Clear();
            Sacrifice_TypeCombo.SelectedIndex = 0;
            Sacrifice_FullNameTB.Focus();
        }

        private static string? NullIfEmpty(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        private static void Warn(string t)
            => MessageBox.Show(t, "ایثارگری", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}