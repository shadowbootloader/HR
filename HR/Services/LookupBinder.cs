using HR.Models;
using HR.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HR.Services
{
    public static class LookupBinder
    {
        /// <summary>
        /// ComboBox را از جدول Lookup پر می‌کند.
        /// برچسب‌ها از DB و مقدار هر آیتم شماره‌ی ترتیبی enum است.
        /// </summary>
        public static async Task FillAsync<TEnum>(ComboBox combo, string tableKey, int valueOffset = 0)
            where TEnum : struct, Enum
        {
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
    }
}