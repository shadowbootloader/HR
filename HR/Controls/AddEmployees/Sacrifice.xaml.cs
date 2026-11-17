using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;
using HR.Services.Data;

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

            Loaded += Sacrifice_Loaded;
        }

        private async void Sacrifice_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Sacrifice_Loaded;

            try
            {
                var cache = await LookupCache.GetAsync();

                Fill(Sacrifice_RelationshipCombo, cache[LookupKeys.SacrificeRelationship].Select(x => x.Title).ToList());
                Fill(Sacrifice_TypeCombo, cache[LookupKeys.SacrificeType].Select(x => x.Title).ToList());
                Fill(Sacrifice_UnitCombo, cache[LookupKeys.AmountUnit].Select(x => x.Title).ToList());
            }
            catch
            {
                // دیتابیس در دسترس نیست → لیست خالی می‌ماند
            }
        }

        private static void Fill(ComboBox combo, System.Collections.Generic.List<string> items)
        {
            if (items.Count == 0) return;

            combo.ItemsSource = items;
            combo.SelectedIndex = 0;
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

            var amount = Sacrifice_MuchTB.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(amount) &&
                !decimal.TryParse(amount, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
            {
                Warn("میزان باید عددی باشد.");
                Sacrifice_MuchTB.Focus();
                return;
            }

            _list.Commit(new HR.Models.SacrificeRecord
            {
                FullName = name,
                SacrificeRelationship = Sacrifice_RelationshipCombo.SelectedItem as string ?? string.Empty,
                SacrificeType = Sacrifice_TypeCombo.SelectedItem as string ?? string.Empty,
                Amount = NullIfEmpty(amount),
                AmountUnit = Sacrifice_UnitCombo.SelectedItem as string,
                Note = NullIfEmpty(Sacrifice_NoteTB.Text)
            });

            ClearForm();
        }

        private void DeleteSacrificeDataBtn_Click(object sender, RoutedEventArgs e) => _list.DeleteSelected();
        private void PersonalSacrificeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PersonalSacrificeList.SelectedItem is not SacrificeRecord item) return;
            Sacrifice_FullNameTB.Text = item.FullName;
            LookupBinder.Select(Sacrifice_RelationshipCombo, item.SacrificeRelationship);
            LookupBinder.Select(Sacrifice_TypeCombo, item.SacrificeType);
            Sacrifice_MuchTB.Text = item.Amount ?? string.Empty;
            LookupBinder.Select(Sacrifice_UnitCombo, item.AmountUnit);
            Sacrifice_NoteTB.Text = item.Note ?? string.Empty;
        }

        public void LoadFrom(EmployeeProfile profile) => _list.Replace(profile.Sacrifices);
        public void SaveTo(EmployeeProfile profile) => _list.CopyTo(profile.Sacrifices);

        public bool Validate(out string message) { message = string.Empty; return true; }

        private void ClearForm()
        {
            Sacrifice_FullNameTB.Clear();
            Sacrifice_MuchTB.Clear();
            Sacrifice_NoteTB.Clear();
            if (Sacrifice_RelationshipCombo.Items.Count > 0) Sacrifice_RelationshipCombo.SelectedIndex = 0;
            if (Sacrifice_TypeCombo.Items.Count > 0) Sacrifice_TypeCombo.SelectedIndex = 0;
            if (Sacrifice_UnitCombo.Items.Count > 0) Sacrifice_UnitCombo.SelectedIndex = 0;
            Sacrifice_FullNameTB.Focus();
        }

        private static string? NullIfEmpty(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        private static void Warn(string t)
            => MessageBox.Show(t, "ایثارگری", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}