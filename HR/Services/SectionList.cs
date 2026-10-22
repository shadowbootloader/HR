using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using HR.Models;

namespace HR.Services
{
    /// <summary>
    /// پیمانه مشترک لیست‌های فرم: اتصال به ListView، شماره‌گذاری خودکار ردیف‌ها
    /// و حذف گروهی آیتم‌های انتخاب‌شده. جلوی تکرار کد در ۱۳ بخش را می‌گیرد.
    /// </summary>
    public sealed class SectionList<T> where T : ObservableRow
    {
        public ObservableCollection<T> Items { get; } = new();

        private ListView? _listView;

        public void Attach(ListView listView)
        {
            _listView = listView;
            _listView.ItemsSource = Items;
            Items.CollectionChanged += (_, __) => RowNumbering.Renumber(Items);
        }

        public void DeleteSelected()
        {
            if (_listView == null) return;

            var selected = _listView.SelectedItems.Cast<T>().ToList();
            foreach (var item in selected)
                Items.Remove(item);
        }

        public void Replace(IEnumerable<T> source)
        {
            Items.Clear();
            foreach (var item in source)
                Items.Add(item);

            RowNumbering.Renumber(Items);
        }

        public void CopyTo(ObservableCollection<T> target)
        {
            target.Clear();
            foreach (var item in Items)
                target.Add(item);
        }

        public bool Any(System.Func<T, bool> predicate) => Items.Any(predicate);
    }
}