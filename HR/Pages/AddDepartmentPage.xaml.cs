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
using HR.Services.Auth;
using HR.Services.Data;

namespace HR.Pages
{
    public partial class AddDepartmentPage : Page
    {
        public event Action<Page>? NavigationRequested;

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

            if (AuthSession.Current?.CanManagePeople != true)
                SaveDepartmentBtn.IsEnabled = false;
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
                using var departmentService = new DepartmentService();
                var departments = await departmentService.GetAllAsync();
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
                using var employeeService = new EmployeeService();
                var employees = await employeeService.GetAllAsync();
                var people = employees
                    .Select(e => new EmployeePick(
                        e.EmployeeId,
                        string.IsNullOrWhiteSpace(e.Supplement?.PersonnelCode)
                            ? $"{e.FirstName} {e.LastName}".Trim()
                            : $"{e.FirstName} {e.LastName}".Trim() + " — " + e.Supplement.PersonnelCode))
                    .OrderBy(x => x.Name)
                    .ToList();
                people.Insert(0, new EmployeePick(Guid.Empty, "—"));

                BindEmployeeCombo(DepertmentManagerCB, people);
                BindEmployeeCombo(DepartmentDeputyCB, people);
                BindEmployeeCombo(DepartmentCashCustodian, people);

                DepartmentHFStatusCB.ItemsSource = new[] { "—" }.Concat(SecurityLevels.All).ToList();
                DepartmentHFStatusCB.SelectedIndex = 0;
                DepartmentStatusCB.SelectedIndex = 0;

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
            using var departmentService = new DepartmentService();
            var dept = await departmentService.GetByIdAsync(id);
            if (dept == null)
            {
                NotificationService.Warning("خطا", "دپارتمان یافت نشد.");
                _editingId = null;
                return;
            }

            DepartmentnameTB.Text = dept.Title;
            DepartMentID.Text = dept.DepartmentId.ToString();
            DepartmentStatusCB.SelectedIndex = dept.IsActive ? 0 : 1;

            DepartmentHFStatusCB.SelectedItem = string.IsNullOrWhiteSpace(dept.SecurityLevel) ? "—" : dept.SecurityLevel;

            ParentDepartmentCB.SelectedValue = dept.ParentDepartmentId;
            StartDateDP.SelectedDate = dept.EstablishedDate;
            MaxPersonalCountTB.Text = dept.ApprovedCapacity?.ToString() ?? "";
            AccountingCode.Text = dept.CostCenterCode ?? "";
            AnnualBudget.Text = dept.AnnualBudget?.ToString("N0", Fa) ?? "";
            PettyCashLimit.Text = dept.PettyCashLimit?.ToString("N0", Fa) ?? "";

            DepertmentManagerCB.SelectedValue = dept.ManagerEmployeeId ?? Guid.Empty;
            DepartmentDeputyCB.SelectedValue = dept.DeputyEmployeeId ?? Guid.Empty;
            DepartmentCashCustodian.SelectedValue = dept.CashCustodianEmployeeId ?? Guid.Empty;

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

            var capacityText = ToAsciiDigits(MaxPersonalCountTB.Text).Trim();
            if (capacityText.Length > 0 && (!int.TryParse(capacityText, out var capacityValue) || capacityValue < 0))
            {
                NotificationService.Warning("خطا", "ظرفیت مصوب باید یک عدد صحیح غیرمنفی باشد.");
                return;
            }

            _isSaving = true;
            try
            {
                var entity = BuildEntity(title);

                // برای هر ذخیره یک DbContext تازه: شکست ذخیره، تلاش بعدی را خراب نمی‌کند.
                using var departmentService = new DepartmentService();

                if (_editingId.HasValue)
                {
                    entity.DepartmentId = _editingId.Value;
                    await departmentService.UpdateAsync(entity);
                    NotificationService.Success("ویرایش موفق", $"دپارتمان «{title}» به‌روز شد.");
                }
                else
                {
                    var created = await departmentService.CreateAsync(entity);
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

            // ارقام فارسی/عربی هم پذیرفته می‌شوند (قبلاً int.TryParse آن‌ها را بی‌صدا نادیده می‌گرفت).
            int? capacity = null;
            if (int.TryParse(ToAsciiDigits(MaxPersonalCountTB.Text).Trim(), out var cap))
                capacity = cap;

            var statusText = (DepartmentStatusCB.SelectedItem as ComboBoxItem)?.Content?.ToString();
            var security = DepartmentHFStatusCB.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(security) || security == "—") security = null;

            var entity = new Department
            {
                Title = title,
                ParentDepartmentId = parentId,
                IsActive = statusText != "غیرفعال",
                SecurityLevel = security,
                EstablishedDate = StartDateDP.SelectedDate,
                ApprovedCapacity = capacity,
                LogoPath = _logoPath,
                ManagerEmployeeId = SelectedEmployee(DepertmentManagerCB),
                DeputyEmployeeId = SelectedEmployee(DepartmentDeputyCB),
                CashCustodianEmployeeId = SelectedEmployee(DepartmentCashCustodian),
                CostCenterCode = string.IsNullOrWhiteSpace(AccountingCode.Text) ? null : AccountingCode.Text.Trim(),
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

        /// <summary>ارقام فارسی (۰-۹) و عربی-هندی (٠-٩) را به ارقام لاتین تبدیل می‌کند.</summary>
        private static string ToAsciiDigits(string? text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            var chars = text.Select(c =>
                c is >= '۰' and <= '۹' ? (char)('0' + (c - '۰')) :
                c is >= '٠' and <= '٩' ? (char)('0' + (c - '٠')) :
                c).ToArray();

            return new string(chars);
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

        private static Guid? SelectedEmployee(ComboBox combo)
            => combo.SelectedValue is Guid id && id != Guid.Empty ? id : null;

        private sealed record ParentItem(short? Id, string Title);
        private sealed record EmployeePick(Guid Id, string Name);

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
