using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using HR.Controls.AddEmployees;
using HR.Models;
using HR.Services;

namespace HR.Pages
{
    public partial class AddEmployeePage : Page
    {
        /// <summary>بخش‌هایی که در XAML نیستند و تنبل (lazy) ساخته می‌شوند.</summary>
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

        public AddEmployeePage()
        {
            InitializeComponent();
            Loaded += AddEmployeePage_Loaded;
        }

        private async void AddEmployeePage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= AddEmployeePage_Loaded;

            await LoadRemainingSectionsAsync();
            LoadProfileIntoForm();
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

                // فرصت به UI تا اسکرول/کلیک قفل نشود
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
                "Citizenship" => new Citizenship(),
                "Criminal" => new Criminal(),
                "Family" => new Family(),
                "ForeignCooperation" => new ForeignCooperation(),
                "ForeignRelatives" => new ForeignRelatives(),
                "Lang" => new Languages(),
                "Skills" => new Skills(),
                "Organ" => new OrganCollaboration(),
                "WorkInterests" => new WorkInterests(),
                "SelfAssessment" => new SelfAssessment(),
                "Other" => new OtherActivity(),
                "Recruitment" => new Recruitment(),
                "Sacrifice" => new Sacrifice(),
                "Supplementary" => new Supplementary(),
                "TripAbroad" => new TripAbroad(),
                _ => throw new ArgumentOutOfRangeException(nameof(key), key, "بخش ناشناخته")
            };

            _sectionCache[key] = section;
            return section;
        }

        // ==================== اتصال به پرونده ====================

        private IEnumerable<IEmployeeSection> EnumerateSections()
            => DataContainerST.Children.OfType<IEmployeeSection>();

        private void LoadProfileIntoForm()
        {
            foreach (var section in EnumerateSections())
                section.LoadFrom(_profile);
        }

        // ==================== رویدادها ====================

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
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

            NotificationService.Success(
                "ثبت موفق",
                $"اطلاعات «{(_profile.FullName.Length > 0 ? _profile.FullName : "کارمند جدید")}» آماده ذخیره شد.");
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            var answer = MessageBox.Show(
                "تغییرات ذخیره‌نشده از بین برود؟",
                "انصراف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (answer != MessageBoxResult.Yes) return;

            _profile = new EmployeeProfile();
            LoadProfileIntoForm();

            NotificationService.Info("انصراف", "فرم به حالت اولیه بازگشت.");
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // بارگذاری تنبل فعلاً در Loaded انجام می‌شود.
            // اگر بعداً بارگذاری بر اساس اسکرول لازم شد، اینجا پیاده شود.
        }
    }
}