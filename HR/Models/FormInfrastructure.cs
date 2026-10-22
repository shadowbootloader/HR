using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HR.Models
{
    /// <summary>
    /// پایه‌ی رکوردهای فرم: شماره ردیف نمایشی + اطلاع‌رسانی تغییر.
    /// </summary>
    public abstract class ObservableRow : INotifyPropertyChanged
    {
        private int _row;

        /// <summary>شماره ردیف نمایشی (۱-based). توسط کنترل فرم مدیریت می‌شود.</summary>
        public int Row
        {
            get => _row;
            set
            {
                if (_row == value) return;
                _row = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>شماره‌گذاری مجدد ردیف‌ها پس از افزودن/حذف.</summary>
    public static class RowNumbering
    {
        public static void Renumber<T>(IList<T> items) where T : ObservableRow
        {
            for (int i = 0; i < items.Count; i++)
                items[i].Row = i + 1;
        }
    }

    /// <summary>آیتم ComboBox که متن فارسی را به یک مقدار (معمولاً enum) نگاشت می‌کند.</summary>
    public sealed class ComboItem
    {
        public ComboItem(object? value, string text)
        {
            Value = value;
            Text = text;
        }

        public object? Value { get; }
        public string Text { get; }

        public override string ToString() => Text;
    }
}