using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HR.Models;

namespace HR.Controls.AddEmployees
{
    public partial class Languages : UserControl, IEmployeeSection
    {
        private readonly ObservableCollection<LanguageRecord> _items = new();

        public string SectionTitle => "زبان خارجی";

        private static readonly string[] LanguageNames =
        {
            "انگلیسی", "عربی", "فرانسوی", "آلمانی", "اسپانیایی",
            "ترکی", "روسی", "چینی", "ژاپنی", "سایر"
        };

        public Languages()
        {
            InitializeComponent();

            PersonalLangList.ItemsSource = _items;
            _items.CollectionChanged += (_, __) => RowNumbering.Renumber(_items);

            LangNameTB.ItemsSource = LanguageNames;
            LangNameTB.SelectedIndex = 0;

            foreach (var combo in new[] { ReadingCombo, WritingCombo, DarkMatlabCombo, SpeakingCombo })
            {
                combo.ItemsSource = new[]
                {
                    new ComboItem(LanguageLevel.None,         "—"),
                    new ComboItem(LanguageLevel.Basic,        "مبتدی"),
                    new ComboItem(LanguageLevel.Intermediate, "متوسط"),
                    new ComboItem(LanguageLevel.Good,         "خوب"),
                    new ComboItem(LanguageLevel.Fluent,       "روان"),
                    new ComboItem(LanguageLevel.Native,       "زبان مادری"),
                };
                combo.DisplayMemberPath = nameof(ComboItem.Text);
                combo.SelectedValuePath = nameof(ComboItem.Value);
                combo.SelectedIndex = 1;
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

            LangNameTB.SelectedIndex = 0;
            ReadingCombo.SelectedIndex = 1;
            WritingCombo.SelectedIndex = 1;
            DarkMatlabCombo.SelectedIndex = 1;
            SpeakingCombo.SelectedIndex = 1;
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