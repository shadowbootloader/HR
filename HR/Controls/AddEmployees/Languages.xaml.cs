using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;
using HR.Services.Data;

namespace HR.Controls.AddEmployees
{
    public partial class Languages : UserControl, IEmployeeSection
    {
        private readonly ObservableCollection<LanguageRecord> _items = new();

        public string SectionTitle => "زبان خارجی";

        public Languages()
        {
            InitializeComponent();

            PersonalLangList.ItemsSource = _items;
            _items.CollectionChanged += (_, __) => RowNumbering.Renumber(_items);

            foreach (var combo in new[] { ReadingCombo, WritingCombo, DarkMatlabCombo, SpeakingCombo })
            {
                combo.DisplayMemberPath = nameof(ComboItem.Text);
                combo.SelectedValuePath = nameof(ComboItem.Value);
            }

            Loaded += Languages_Loaded;
        }

        private async void Languages_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Languages_Loaded;

            try
            {
                var languages = (await LookupCache.GetAsync())[LookupKeys.Language];
                if (languages.Count > 0)
                {
                    var titles = languages.Select(l => l.Title).ToList();
                    var current = LangNameTB.SelectedItem as string;

                    LangNameTB.ItemsSource = titles;
                    LangNameTB.SelectedItem = current != null && titles.Contains(current)
                        ? current
                        : titles[0];
                }
            }
            catch
            {
                // دیتابیس در دسترس نیست → لیست پیش‌فرض خالی می‌ماند
            }

            // جدول جداگانه‌ای برای سطح زبان نیست؛ روی SkillLevel نگاشت می‌شود
            foreach (var combo in new[] { ReadingCombo, WritingCombo, DarkMatlabCombo, SpeakingCombo })
            {
                await LookupBinder.FillAsync<LanguageLevel>(combo, LookupKeys.SkillLevel);
                combo.SelectedIndex = 0;
            }
        }

        // ==================== رویدادها ====================

        private void AddLangDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var language = LangNameTB.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(language))
            {
                Warn("زبان را انتخاب کنید.");
                return;
            }

            if (_items.Any(x => x.Language == language))
            {
                Warn($"زبان «{language}» قبلاً ثبت شده است.");
                return;
            }

            _items.Add(new LanguageRecord
            {
                Language = language,
                Reading = ReadLevel(ReadingCombo),
                Writing = ReadLevel(WritingCombo),
                Comprehension = ReadLevel(DarkMatlabCombo),
                Speaking = ReadLevel(SpeakingCombo)
            });

            if (LangNameTB.Items.Count > 0) LangNameTB.SelectedIndex = 0;
            ReadingCombo.SelectedIndex = 0;
            WritingCombo.SelectedIndex = 0;
            DarkMatlabCombo.SelectedIndex = 0;
            SpeakingCombo.SelectedIndex = 0;
        }

        private void DeleteLangDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = PersonalLangList.SelectedItems.Cast<LanguageRecord>().ToList();
            foreach (var item in selected)
                _items.Remove(item);
        }

        // ==================== IEmployeeSection ====================

        public void LoadFrom(EmployeeProfile profile)
        {
            _items.Clear();
            foreach (var language in profile.Languages)
                _items.Add(language);

            RowNumbering.Renumber(_items);
        }

        public void SaveTo(EmployeeProfile profile)
        {
            profile.Languages.Clear();
            foreach (var language in _items)
                profile.Languages.Add(language);
        }

        public bool Validate(out string message)
        {
            var duplicate = _items
                .GroupBy(x => x.Language)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate != null)
            {
                message = $"زبان تکراری: {duplicate.Key}";
                return false;
            }

            message = string.Empty;
            return true;
        }

        // ==================== کمکی‌ها ====================

        private static LanguageLevel ReadLevel(ComboBox combo)
            => combo.SelectedValue is LanguageLevel level ? level : LanguageLevel.None;

        private static void Warn(string text)
            => MessageBox.Show(text, "زبان خارجی", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}