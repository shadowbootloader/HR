using HR.Models;
using HR.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HR.Services
{
    /// <summary>
    /// پر کردن ComboBox از جداول Lookup پایگاه داده.
    /// </summary>
    public static class LookupBinder
    {
        /// <summary>
        /// ComboBox را از جدول Lookup پر می‌کند.
        /// برچسب‌ها از DB و مقدار هر آیتم شماره‌ی ترتیبی enum است.
        /// </summary>
        public static void Select(ComboBox combo, string? value)
        {
            if (combo == null) return;
            var text = value?.Trim();
            if (string.IsNullOrEmpty(text))
            {
                if (combo.Items.Count > 0) combo.SelectedIndex = 0;
                return;
            }

            for (int i = 0; i < combo.Items.Count; i++)
            {
                var label = combo.Items[i] switch
                {
                    string s => s,
                    ComboBoxItem box => box.Content?.ToString(),
                    ComboItem item => item.Text,
                    _ => combo.Items[i]?.ToString()
                };
                if (string.Equals(label, text, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }

            combo.Text = text;
        }

        public static string SelectedTitle(ComboBox combo)
        {
            if (combo.SelectedItem is ComboItem item && !string.IsNullOrWhiteSpace(item.Text))
                return item.Text.Trim();
            if (combo.SelectedItem is ComboBoxItem boxItem)
                return boxItem.Content?.ToString()?.Trim() ?? string.Empty;
            if (combo.SelectedItem is string text)
                return text.Trim();
            return combo.Text?.Trim() ?? string.Empty;
        }

        public static async Task FillAsync<TEnum>(ComboBox combo, string tableKey, int valueOffset = 0)
            where TEnum : struct, Enum
        {
            if (combo == null) return;

            try
            {
                var rows = (await LookupCache.GetAsync())[tableKey];
                if (rows.Count == 0) return;

                var items = new List<ComboItem>(rows.Count);
                for (int i = 0; i < rows.Count; i++)
                    items.Add(new ComboItem(Enum.ToObject(typeof(TEnum), i + valueOffset), rows[i].Title));

                var previous = combo.SelectedValue;

                combo.ItemsSource = items;
                combo.DisplayMemberPath = nameof(ComboItem.Text);
                combo.SelectedValuePath = nameof(ComboItem.Value);

                if (previous != null && items.Any(x => Equals(x.Value, previous)))
                    combo.SelectedValue = previous;
                else if (combo.SelectedIndex < 0)
                    combo.SelectedIndex = 0;
            }
            catch
            {
                // دیتابیس در دسترس نیست → لیست پیش‌فرض همان کنترل می‌ماند
            }
        }

        /// <summary>
        /// ComboBox را فقط با عنوان‌های متنی پر می‌کند (برای بانک، زبان، نرم‌افزار، دپارتمان و ...).
        /// SelectedItem برابر عنوان انتخاب‌شده خواهد بود.
        /// </summary>
        public static async Task FillTitlesAsync(ComboBox combo, string tableKey, bool addEmptyItem = false)
        {
            if (combo == null) return;

            try
            {
                var rows = (await LookupCache.GetAsync())[tableKey];
                var titles = rows.Select(r => r.Title).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();

                if (addEmptyItem)
                    titles.Insert(0, "—");

                if (titles.Count == 0) return;

                var previous = combo.SelectedItem as string ?? combo.Text;

                combo.ItemsSource = titles;

                if (!string.IsNullOrWhiteSpace(previous) && titles.Contains(previous))
                    combo.SelectedItem = previous;
                else if (combo.SelectedIndex < 0)
                    combo.SelectedIndex = 0;
            }
            catch
            {
            }
        }

        /// <summary>
        /// ComboBox را با LookupItem (Id + Title) پر می‌کند.
        /// DisplayMemberPath = Title ، SelectedValuePath = Id
        /// </summary>
        public static async Task FillItemsAsync(ComboBox combo, string tableKey)
        {
            if (combo == null) return;

            try
            {
                var rows = (await LookupCache.GetAsync())[tableKey];
                if (rows.Count == 0) return;

                var previous = combo.SelectedValue;

                combo.ItemsSource = rows.ToList();
                combo.DisplayMemberPath = nameof(LookupItem.Title);
                combo.SelectedValuePath = nameof(LookupItem.Id);

                if (previous != null && rows.Any(x => Equals(x.Id, previous)))
                    combo.SelectedValue = previous;
                else if (combo.SelectedIndex < 0)
                    combo.SelectedIndex = 0;
            }
            catch
            {
            }
        }

        /// <summary>
        /// چند ComboBox را همزمان با کلیدهای مختلف پر می‌کند.
        /// </summary>
        public static async Task FillManyTitlesAsync(params (ComboBox Combo, string TableKey)[] targets)
        {
            if (targets == null || targets.Length == 0) return;

            try
            {
                var cache = await LookupCache.GetAsync();
                foreach (var (combo, key) in targets)
                {
                    if (combo == null) continue;
                    var rows = cache[key];
                    var titles = rows.Select(r => r.Title).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
                    if (titles.Count == 0) continue;

                    var previous = combo.SelectedItem as string ?? combo.Text;
                    combo.ItemsSource = titles;
                    if (!string.IsNullOrWhiteSpace(previous) && titles.Contains(previous))
                        combo.SelectedItem = previous;
                    else if (combo.SelectedIndex < 0)
                        combo.SelectedIndex = 0;
                }
            }
            catch
            {
            }
        }
    }
}
