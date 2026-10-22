using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HR.Controls;
using HR.Models;
using MaterialDesignThemes.Wpf;

namespace HR.Pages
{
    public partial class EmployeeListPage : Page, INotifyPropertyChanged
    {
        public event Action<Page>? NavigationRequested;

        private const int PageSize = 10;

        private readonly List<EmployeeModel> _all = new();
        private readonly ObservableCollection<EmployeeModel> _visible = new();

        private int _currentPage = 1;
        private int _totalPages = 1;
        private string _searchTerm = string.Empty;
        private string _statusFilter = "همه";

        public int FromIndex { get; private set; }
        public int ToIndex { get; private set; }
        public int TotalCount { get; private set; }

        public string PaginationText => TotalCount == 0
            ? "موردی یافت نشد"
            : $"نمایش {FromIndex} تا {ToIndex} از {TotalCount} کارکنان";

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public EmployeeListPage()
        {
            InitializeComponent();

            DataContext = this;

            EmployeeItems.ItemsSource = _visible;
            LoadSampleData();
            ApplyFilter(resetPage: true);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        // ==================== چرخه عمر ====================

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            EmployeeRowControl.NavigationRequested -= OnRowNavigationRequested;
            EmployeeRowControl.NavigationRequested += OnRowNavigationRequested;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
            => EmployeeRowControl.NavigationRequested -= OnRowNavigationRequested;

        private void OnRowNavigationRequested(Page page) => NavigationRequested?.Invoke(page);

        // ==================== داده ====================

        private void LoadSampleData()
        {
            _all.Clear();

            string[] firstNames = { "امیرحسین", "محمدرضا", "سعید", "رضا", "علی", "مهدی", "پارسا", "نیما", "حسین", "زهرا", "مریم", "سارا", "الهام", "مینا", "فاطمه", "نازنین" };
            string[] lastNames = { "رضایی", "احمدی", "کریمی", "محمدی", "اکبری", "حسینی", "نادری", "نوری", "موسوی", "صالحی", "شریفی", "جعفری", "رستمی", "کاظمی" };
            string[] positions = { "مهندس نرم‌افزار", "کارشناس جذب", "تحلیلگر مالی", "طراح رابط کاربری", "مدیر بازاریابی", "سرپرست تیم", "کارشناس فروش", "حسابدار", "برنامه‌نویس بک‌اند", "لید تست" };
            string[] departments = { "منابع انسانی", "فناوری اطلاعات", "امور مالی", "بازاریابی", "پشتیبانی", "آموزش و توسعه" };
            string[] statuses = { "فعال", "مرخصی", "غیرفعال" };

            var random = new Random(1404);

            for (int i = 1; i <= 148; i++)
            {
                var first = firstNames[random.Next(firstNames.Length)];
                var last = lastNames[random.Next(lastNames.Length)];

                _all.Add(new EmployeeModel
                {
                    FullName = $"{first} {last}",
                    Email = $"user{i}@company.com",
                    Position = positions[random.Next(positions.Length)],
                    Department = departments[random.Next(departments.Length)],
                    DepartmentIcon = "👤",
                    Status = statuses[random.Next(statuses.Length)],
                    PerformanceValue = random.Next(45, 100),
                    PerformanceLabel = "—",
                    AvatarUrl = PackIconKind.Account
                });
            }
        }

        // ==================== فیلتر و صفحه‌بندی ====================

        private void ApplyFilter(bool resetPage)
        {
            var query = _all.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(_searchTerm))
            {
                query = query.Where(x =>
                    x.FullName.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    x.Position.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    x.Department.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (_statusFilter != "همه")
                query = query.Where(x => x.Status == _statusFilter);

            var filtered = query.ToList();

            if (resetPage) _currentPage = 1;

            _totalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)PageSize));
            if (_currentPage > _totalPages) _currentPage = _totalPages;

            _visible.Clear();
            foreach (var item in filtered.Skip((_currentPage - 1) * PageSize).Take(PageSize))
                _visible.Add(item);

            TotalCount = filtered.Count;
            FromIndex = TotalCount == 0 ? 0 : (_currentPage - 1) * PageSize + 1;
            ToIndex = Math.Min(_currentPage * PageSize, TotalCount);

            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(FromIndex));
            OnPropertyChanged(nameof(ToIndex));
            OnPropertyChanged(nameof(PaginationText));

            UpdatePaginationUI();
        }

        private void UpdatePaginationUI()
        {
            BtnPrevious.IsEnabled = _currentPage > 1;
            BtnPrevious.Opacity = BtnPrevious.IsEnabled ? 1 : 0.4;

            BtnNext.IsEnabled = _currentPage < _totalPages;
            BtnNext.Opacity = BtnNext.IsEnabled ? 1 : 0.4;

            BtnCurrent.Content = _currentPage.ToString();
            BtnPage2.Visibility = _totalPages >= 2 ? Visibility.Visible : Visibility.Collapsed;
            BtnPage3.Visibility = _totalPages >= 3 ? Visibility.Visible : Visibility.Collapsed;
            BtnPage2.Content = Math.Min(_totalPages, _currentPage + 1).ToString();
            BtnPage3.Content = Math.Min(_totalPages, _currentPage + 2).ToString();

            BtnLast.Visibility = _totalPages > 3 ? Visibility.Visible : Visibility.Collapsed;
            BtnLast.Content = _totalPages.ToString();
            TxtDots.Visibility = BtnLast.Visibility;
        }

        private void GoToPage(int page)
        {
            if (page < 1 || page > _totalPages || page == _currentPage) return;
            _currentPage = page;
            ApplyFilter(resetPage: false);
        }

        // ==================== رویدادها ====================

        private void BtnPrevious_Click(object sender, RoutedEventArgs e) => GoToPage(_currentPage - 1);

        private void BtnNext_Click(object sender, RoutedEventArgs e) => GoToPage(_currentPage + 1);

        private void PageButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Content?.ToString(), out var page))
                GoToPage(page);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchTerm = SearchBox.Text?.Trim() ?? string.Empty;
            ApplyFilter(resetPage: true);
        }

        private void StatusFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _statusFilter = (StatusFilterCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "همه";
            ApplyFilter(resetPage: true);
        }

        private void SummaryCardControl_Loaded(object sender, RoutedEventArgs e) { }

        private void BtnAddEmployee_Click(object sender, RoutedEventArgs e)
            => NavigationRequested?.Invoke(new AddEmployeePage());
    }

    public class EmployeeModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string DepartmentIcon { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double PerformanceValue { get; set; }
        public string PerformanceLabel { get; set; } = string.Empty;
        public PackIconKind AvatarUrl { get; set; }
    }
}