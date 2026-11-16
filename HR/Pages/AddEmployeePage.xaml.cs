using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using HR.Controls.AddEmployees;
using HR.Data;
using HR.Models;
using HR.Services;
using HR.Services.Data;

namespace HR.Pages
{
    public partial class AddEmployeePage : Page
    {
        private static readonly string[] LazySections =
        {
            "Bank", "Family", "Lang", "Skills", "Abilities",
            "WorkInterests", "SelfAssessment", "TripAbroad",
            "ForeignCooperation", "Citizenship", "Criminal",
            "ForeignRelatives", "Recruitment", "Organ",
            "Other", "Sacrifice", "Supplementary"
        };

        private readonly Dictionary<string, UserControl> _sectionCache = new();
        private bool _lazyLoaded;
        private EmployeeProfile _profile = new();
        private readonly IEmployeeService _employeeService = new EmployeeService();
        private bool _isSaving;
        private Guid? _editingId;

        /// <summary>حالت ایجاد کارمند جدید.</summary>
        public AddEmployeePage()
        {
            InitializeComponent();
            Loaded += AddEmployeePage_Loaded;
        }

        /// <summary>حالت ویرایش کارمند موجود.</summary>
        public AddEmployeePage(Guid employeeId) : this()
        {
            _editingId = employeeId;
        }

        private async void AddEmployeePage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= AddEmployeePage_Loaded;

            await LoadRemainingSectionsAsync();

            if (_editingId.HasValue)
            {
                await LoadEmployeeForEditAsync(_editingId.Value);
            }
            else
            {
                LoadProfileIntoForm();
            }
        }

        private async Task LoadEmployeeForEditAsync(Guid employeeId)
        {
            try
            {
                var entity = await _employeeService.GetByIdAsync(employeeId, includeDetails: true);
                if (entity == null)
                {
                    NotificationService.Warning("خطا", "کارمند یافت نشد.");
                    _editingId = null;
                    LoadProfileIntoForm();
                    return;
                }

                using var lookups = await LookupResolver.LoadAsync(new PersonnelDbContext());
                _profile = await EmployeeMapper.ToProfileAsync(entity, lookups);
                LoadProfileIntoForm();
            }
            catch (Exception ex)
            {
                NotificationService.Warning("خطا در بارگذاری", ex.Message);
            }
        }

        // ==================== ساخت بخش‌ها ====================

        private async Task LoadRemainingSectionsAsync()
        {
            if (_lazyLoaded) return;
            _lazyLoaded = true;

            foreach (var key in LazySections)
            {
                var section = GetOrCreate(key);

                if (section.Parent == null)
                    DataContainerST.Children.Add(section);

                await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);
            }
        }

        private UserControl GetOrCreate(string key)
        {
            if (_sectionCache.TryGetValue(key, out var existing))
                return existing;

            UserControl section = key switch
            {
                "Abilities" => new Abilities(),
                "Bank" => new BankAccounts(),
                "Citizenship" => new HR.Controls.AddEmployees.Citizenship(),
                "Criminal" => new Criminal(),
                "Family" => new Family(),
                "ForeignCooperation" => new HR.Controls.AddEmployees.ForeignCooperation(),
                "ForeignRelatives" => new ForeignRelatives(),
                "Lang" => new Languages(),
                "Skills" => new Skills(),
                "Organ" => new OrganCollaboration(),
                "WorkInterests" => new WorkInterests(),
                "SelfAssessment" => new SelfAssessment(),
                "Other" => new HR.Controls.AddEmployees.OtherActivity(),
                "Recruitment" => new Recruitment(),
                "Sacrifice" => new Sacrifice(),
                "Supplementary" => new Supplementary(),
                "TripAbroad" => new TripAbroad(),
                _ => throw new ArgumentOutOfRangeException(nameof(key), key, "بخش ناشناخته")
            };

            _sectionCache[key] = section;
            return section;
        }

        private IEnumerable<IEmployeeSection> EnumerateSections()
            => DataContainerST.Children.OfType<IEmployeeSection>();

        private void LoadProfileIntoForm()
        {
            foreach (var section in EnumerateSections())
                section.LoadFrom(_profile);
        }

        // ==================== ذخیره ====================

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_isSaving) return;

            var sections = EnumerateSections().ToList();

            if (sections.Count == 0)
            {
                NotificationService.Warning("توجه", "هیچ بخشی از فرم بارگذاری نشده است.");
                return;
            }

            foreach (var section in sections)
            {
                if (section.Validate(out var message))
                    continue;

                NotificationService.Warning(section.SectionTitle, message);
                return;
            }

            foreach (var section in sections)
                section.SaveTo(_profile);

            if (string.IsNullOrWhiteSpace(_profile.FirstName))
            {
                NotificationService.Warning("خطا", "نام الزامی است.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_profile.LastName))
            {
                NotificationService.Warning("خطا", "نام خانوادگی الزامی است.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_profile.NationalCode))
            {
                NotificationService.Warning("خطا", "کد ملی الزامی است.");
                return;
            }

            _isSaving = true;
            try
            {
                using var lookups = await LookupResolver.LoadAsync(new PersonnelDbContext());

                var entity = await EmployeeMapper.ToEntityAsync(_profile, lookups, _editingId);

                if (await _employeeService.ExistsNationalCodeAsync(entity.NationalCode, _editingId))
                {
                    NotificationService.Warning("خطا", "کد ملی تکراری است.");
                    return;
                }

                if (entity.Supplement != null &&
                    await _employeeService.ExistsPersonnelCodeAsync(entity.Supplement.PersonnelCode, _editingId))
                {
                    NotificationService.Warning("خطا", "کد پرسنلی تکراری است.");
                    return;
                }

                if (_editingId.HasValue)
                {
                    await _employeeService.UpdateAsync(entity);
                    NotificationService.Success(
                        "ویرایش موفق",
                        $"کارمند «{entity.FirstName} {entity.LastName}» با موفقیت به‌روز شد.");
                }
                else
                {
                    await _employeeService.CreateAsync(entity);
                    NotificationService.Success(
                        "ثبت موفق",
                        $"کارمند «{entity.FirstName} {entity.LastName}» با موفقیت ذخیره شد.");

                    _profile = new EmployeeProfile();
                    LoadProfileIntoForm();
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null &&
                    !string.Equals(ex.InnerException.Message, ex.Message, StringComparison.Ordinal))
                {
                    message += Environment.NewLine + ex.InnerException.Message;
                }

                NotificationService.Warning("خطا در ذخیره", message);
            }
            finally
            {
                _isSaving = false;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            var answer = MessageBox.Show(
                "تغییرات ذخیره‌نشده از بین برود؟",
                "انصراف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (answer != MessageBoxResult.Yes) return;

            if (_editingId.HasValue)
            {
                // بازگشت به لیست در صورت تمایل می‌تواند از بیرون هندل شود
                _ = LoadEmployeeForEditAsync(_editingId.Value);
            }
            else
            {
                _profile = new EmployeeProfile();
                LoadProfileIntoForm();
            }

            NotificationService.Info("انصراف", "فرم به حالت اولیه بازگشت.");
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
        }
    }
}
