using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using HR.Data.Entities;
using HR.Services;
using HR.Services.Data;

namespace HR.Pages
{
    public partial class AddDepartmentPage : Page
    {
        public event Action<Page>? NavigationRequested;

        private readonly IDepartmentService _departmentService = new DepartmentService();
        private readonly IEmployeeService _employeeService = new EmployeeService();

        private readonly ObservableCollection<OfficeRow> _offices = new();
        private short? _editingId;
        private string? _logoPath;
        private bool _formatting;
        private bool _isSaving;
        private static readonly CultureInfo Fa = new("fa-IR");

        public AddDepartmentPage()
        {
            InitializeComponent();
            if (OfficesList != null)
                OfficesList.ItemsSource = _offices;

            Loaded += async (_, _) => await InitAsync();
        }

        public AddDepartmentPage(short departmentId) : this()
        {
            _editingId = departmentId;
        }

        private async Task InitAsync()
        {
            try
            {
                // والدین
                var departments = await _departmentService.GetAllAsync();
                var parentItems = departments
                    .Where(d => !_editingId.HasValue || d.DepartmentId != _editingId.Value)
                    .Select(d => new ParentItem(d.DepartmentId, d.Title))
                    .ToList();
                parentItems.Insert(0, new ParentItem(null, "— بدون والد —"));
                ParentDepartmentCB.ItemsSource = parentItems;
                ParentDepartmentCB.DisplayMemberPath = nameof(ParentItem.Title);
                ParentDepartmentCB.SelectedValuePath = nameof(ParentItem.Id);
                ParentDepartmentCB.SelectedIndex = 0;

                // کارکنان برای مدیر / جانشین / تنخواه‌دار
                var employees = await _employeeService.GetAllAsync();
                var people = employees
                    .Select(e => new EmployeePick(e.EmployeeId, $"{e.FirstName} {e.LastName}".Trim()))
                    .OrderBy(x => x.Name)
                    .ToList();
                people.Insert(0, new EmployeePick(null, "—"));

                BindEmployeeCombo(DepertmentManagerCB, people);
                BindEmployeeCombo(DepartmentDeputyCB, people);
                BindEmployeeCombo(DepartmentCashCustodian, people);

                DepartmentStatusCB.SelectedIndex = 0;
                DepartmentHFStatusCB.SelectedIndex = 0;

                if (_editingId.HasValue)
                    await LoadForEditAsync(_editingId.Value);
            }
            catch (Exception ex)
            {
                NotificationService.Warning("خطا", "بارگذاری فرم دپارتمان: " + ex.Message);
            }
        }

        private static void BindEmployeeCombo(ComboBox combo, System.Collections.Generic.List<EmployeePick> people)
        {
            combo.ItemsSource = people;
            combo.DisplayMemberPath = nameof(EmployeePick.Name);
            combo.SelectedValuePath = nameof(EmployeePick.Id);
            combo.SelectedIndex = 0;
        }

        private async Task LoadForEditAsync(short id)
        {
            var dept = await _departmentService.GetByIdAsync(id);
            if (dept == null)
            {
                NotificationService.Warning("خطا", "دپارتمان یافت نشد.");
                _editingId = null;
                return;
            }

            DepartmentnameTB.Text = dept.Title;
            DepartMentID.Text = dept.DepartmentId.ToString();
            DepartmentStatusCB.SelectedIndex = dept.IsActive ? 0 : 1;

            if (!string.IsNullOrWhiteSpace(dept.SecurityLevel))
            {
                for (int i = 0; i < DepartmentHFStatusCB.Items.Count; i++)
                {
                    if (DepartmentHFStatusCB.Items[i] is ComboBoxItem item &&
                        string.Equals(item.Content?.ToString(), dept.SecurityLevel, StringComparison.Ordinal))
                    {
                        DepartmentHFStatusCB.SelectedIndex = i;
                        break;
                    }
                }
            }

            ParentDepartmentCB.SelectedValue = dept.ParentDepartmentId;
            StartDateDP.SelectedDate = dept.EstablishedDate;
            MaxPersonalCountTB.Text = dept.ApprovedCapacity?.ToString() ?? "";
            AccountingCode.Text = dept.CostCenterCode ?? "";
            AnnualBudget.Text = dept.AnnualBudget?.ToString("N0", Fa) ?? "";
            PettyCashLimit.Text = dept.PettyCashLimit?.ToString("N0", Fa) ?? "";

            DepertmentManagerCB.SelectedValue = dept.ManagerEmployeeId;
            DepartmentDeputyCB.SelectedValue = dept.DeputyEmployeeId;
            DepartmentCashCustodian.SelectedValue = dept.CashCustodianEmployeeId;

            _logoPath = dept.LogoPath;
            ImagePicker.Preview(LogoImage, _logoPath);

            _offices.Clear();
            foreach (var o in dept.Offices)
            {
                _offices.Add(new OfficeRow
                {
                    OfficeId = o.OfficeId,
                    Name = o.Name,
                    City = o.City ?? "",
                    Address = o.Address ?? ""
                });
            }
        }

        // ==================== ذخیره ====================

        private async void SaveDepartmentBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_isSaving) return;

            var title = DepartmentnameTB.Text?.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                NotificationService.Warning("خطا", "نام دپارتمان الزامی است.");
                return;
            }

            _isSaving = true;
            try
            {
                var entity = BuildEntity(title);

                if (_editingId.HasValue)
                {
                    entity.DepartmentId = _editingId.Value;
                    await _departmentService.UpdateAsync(entity);
                    NotificationService.Success("ویرایش موفق", $"دپارتمان «{title}» به‌روز شد.");
                }
                else
                {
                    var created = await _departmentService.CreateAsync(entity);
                    DepartMentID.Text = created.DepartmentId.ToString();
                    _editingId = created.DepartmentId;
                    NotificationService.Success("ثبت موفق", $"دپارتمان «{title}» ذخیره شد.");
                }
            }
            catch (Exception ex)
            {
                NotificationService.Warning("خطا در ذخیره", ex.Message);
            }
            finally
            {
                _isSaving = false;
            }
        }

        private Department BuildEntity(string title)
        {
            short? parentId = ParentDepartmentCB.SelectedValue as short?;

            int? capacity = null;
            if (int.TryParse(MaxPersonalCountTB.Text?.Trim(), out var cap))
                capacity = cap;

            var statusText = (DepartmentStatusCB.SelectedItem as ComboBoxItem)?.Content?.ToString();
            var security = (DepartmentHFStatusCB.SelectedItem as ComboBoxItem)?.Content?.ToString();

            var entity = new Department
            {
                Title = title,
                ParentDepartmentId = parentId,
                IsActive = statusText != "غیرفعال",
                SecurityLevel = security,
                EstablishedDate = StartDateDP.SelectedDate,
                ApprovedCapacity = capacity,
                LogoPath = _logoPath,
                ManagerEmployeeId = DepertmentManagerCB.SelectedValue as Guid?,
                DeputyEmployeeId = DepartmentDeputyCB.SelectedValue as Guid?,
                CashCustodianEmployeeId = DepartmentCashCustodian.SelectedValue as Guid?,
                CostCenterCode = AccountingCode.Text?.Trim(),
                AnnualBudget = ParseMoney(AnnualBudget.Text),
                PettyCashLimit = ParseMoney(PettyCashLimit.Text)
            };

            foreach (var row in _offices)
            {
                entity.Offices.Add(new DepartmentOffice
                {
                    OfficeId = row.OfficeId == Guid.Empty ? Guid.NewGuid() : row.OfficeId,
                    Name = row.Name,
                    City = row.City,
                    Address = row.Address
                });
            }

            return entity;
        }

        // ==================== دفاتر ====================

        private void AddOfficeBtn_Click(object sender, RoutedEventArgs e)
        {
            var name = OfficeNameTB.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                NotificationService.Warning("توجه", "نام دفتر را وارد کنید.");
                return;
            }

            _offices.Add(new OfficeRow
            {
                OfficeId = Guid.NewGuid(),
                Name = name,
                City = OfficeCityTB.Text?.Trim() ?? "",
                Address = OfficeAddressTB.Text?.Trim() ?? ""
            });

            OfficeNameTB.Text = "";
            OfficeCityTB.Text = "";
            OfficeAddressTB.Text = "";
        }

        private void RemoveOfficeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is OfficeRow row)
                _offices.Remove(row);
        }

        // ==================== لوگو ====================

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var path = ImagePicker.Pick();
            if (path == null) return;
            _logoPath = path;
            ImagePicker.Preview(LogoImage, _logoPath);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationRequested?.Invoke(new DepartmentsPage());
        }

        // ==================== پول ====================

        private void MoneyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_formatting) return;
            if (sender is not TextBox tb) return;

            _formatting = true;

            string digits = new string(tb.Text
                .Select(c => c is >= '۰' and <= '۹' ? (char)('0' + (c - '۰')) : c)
                .Where(char.IsDigit)
                .ToArray());

            if (string.IsNullOrEmpty(digits))
            {
                tb.Text = "";
                _formatting = false;
                return;
            }

            if (long.TryParse(digits, out long value))
            {
                string formatted = value.ToString("N0", Fa);
                int caret = tb.CaretIndex;
                int oldLen = tb.Text.Length;
                tb.Text = formatted;
                tb.CaretIndex = Math.Max(0, caret + (tb.Text.Length - oldLen));
            }

            _formatting = false;
        }

        private static decimal? ParseMoney(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            string digits = new string(text
                .Select(c => c is >= '۰' and <= '۹' ? (char)('0' + (c - '۰')) : c)
                .Where(char.IsDigit)
                .ToArray());
            return decimal.TryParse(digits, out var v) ? v : null;
        }

        // ==================== مدل‌های کمکی UI ====================

        private sealed record ParentItem(short? Id, string Title);
        private sealed record EmployeePick(Guid? Id, string Name);

        private sealed class OfficeRow
        {
            public Guid OfficeId { get; set; }
            public string Name { get; set; } = "";
            public string City { get; set; } = "";
            public string Address { get; set; } = "";
            public string Display => string.Join(" | ", new[] { Name, City, Address }.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}
