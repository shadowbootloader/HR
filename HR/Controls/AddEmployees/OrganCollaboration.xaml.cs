using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;
using HR.Services.Data;

namespace HR.Controls.AddEmployees
{
    public partial class OrganCollaboration : UserControl, IEmployeeSection
    {
        private readonly SectionList<OrganCollaborationRecord> _list = new();

        public string SectionTitle => "همکاری با ارگان‌ها";

        public OrganCollaboration()
        {
            InitializeComponent();
            _list.Attach(PersonalCooperationOrganList);

            Loaded += OrganCollaboration_Loaded;
        }

        private async void OrganCollaboration_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OrganCollaboration_Loaded;

            try
            {
                var types = (await LookupCache.GetAsync())[LookupKeys.SectorType];
                if (types.Count == 0) return;

                CooperationOrgan_OrganTypeCombo.ItemsSource = types.Select(t => t.Title).ToList();
                CooperationOrgan_OrganTypeCombo.SelectedIndex = 0;
            }
            catch
            {
                // دیتابیس در دسترس نیست → لیست خالی می‌ماند
            }
        }

        private void AddCooperationOrganDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var name = CooperationOrgan_OrganNameTB.Text?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                Warn("نام مرکز را وارد کنید.");
                CooperationOrgan_OrganNameTB.Focus();
                return;
            }

            _list.Items.Add(new OrganCollaborationRecord
            {
                OrganName = name,
                OrganType = CooperationOrgan_OrganTypeCombo.SelectedItem as string ?? string.Empty,
                Note = NullIfEmpty(CooperationOrgan_NoteTB.Text)
            });

            ClearForm();
        }

        private void DeleteCooperationOrganDataBtn_Click(object sender, RoutedEventArgs e) => _list.DeleteSelected();
        private void PersonalCooperationOrganList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        public void LoadFrom(EmployeeProfile profile) => _list.Replace(profile.OrganCollaborations);
        public void SaveTo(EmployeeProfile profile) => _list.CopyTo(profile.OrganCollaborations);

        public bool Validate(out string message) { message = string.Empty; return true; }

        private void ClearForm()
        {
            CooperationOrgan_OrganNameTB.Clear();
            CooperationOrgan_NoteTB.Clear();
            if (CooperationOrgan_OrganTypeCombo.Items.Count > 0) CooperationOrgan_OrganTypeCombo.SelectedIndex = 0;
            CooperationOrgan_OrganNameTB.Focus();
        }

        private static string? NullIfEmpty(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        private static void Warn(string t)
            => MessageBox.Show(t, "همکاری با ارگان‌ها", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}