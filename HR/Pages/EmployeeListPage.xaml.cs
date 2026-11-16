using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HR.Controls;
using HR.Services.Data;
using MaterialDesignThemes.Wpf;

namespace HR.Pages
{
    public partial class EmployeeListPage : Page, INotifyPropertyChanged
    {
        public event Action<Page>? NavigationRequested;

        private const int PageSize = 10;

        private readonly List<EmployeeModel> _all = new();
        private readonly ObservableCollection<EmployeeModel> _visible = new();
        private readonly IEmployeeService _employeeService = new EmployeeService();

        private int _currentPage = 1;
        private int _totalPages = 1;
        private string _searchTerm = string.Empty;
        private string _statusFilter = "همه";
        private bool _isLoading;

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

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            Loaded += async (_, _) => await LoadEmployeesAsync();
        }

        // ==================== چرخه عمر ====================

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            EmployeeRowControl.NavigationRequested -= OnRowNavigationRequested;
            EmployeeRowControl.NavigationRequested += OnRowNavigationRequested;
            EmployeeRowControl.ListRefreshRequested -= OnListRefreshRequested;
            EmployeeRowControl.ListRefreshRequested += OnListRefreshRequested;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            EmployeeRowControl.NavigationRequested -= OnRowNavigationRequested;
            EmployeeRowControl.ListRefreshRequested -= OnListRefreshRequested;
        }

        private async void OnListRefreshRequested()
            => await LoadEmployeesAsync();

        private void OnRowNavigationRequested(Page page) => NavigationRequested?.Invoke(page);

        // ==================== خواندن از پایگاه داده ====================

        private async Task LoadEmployeesAsync()
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                // اگر عبارت جستجو دارید از SearchAsync استفاده می‌کنیم؛ وگرنه همه
                var employees = string.IsNullOrWhiteSpace(_searchTerm)
                    ? await _employeeService.GetAllAsync()
                    : await _employeeService.SearchAsync(_searchTerm);

                _all.Clear();

                foreach (var e in employees)
                {
                    _all.Add(MapToModel(e));
                }

                ApplyFilter(resetPage: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در بارگذاری لیست کارکنان:\n" + ex.Message, "خطا",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private static EmployeeModel MapToModel(HR.Data.Entities.Employee e)
        {
            return new EmployeeModel
            {
                Id = e.EmployeeId,
                FullName = $"{e.FirstName} {e.LastName}".Trim(),
                Email = e.Email,
                NationalCode = e.NationalCode,
                PersonnelCode = e.Supplement?.PersonnelCode ?? "—",
                Position = e.Supplement?.PositionTitle ?? "—",
                Department = e.Supplement?.Department?.Title ?? "—",
                DepartmentIcon = "👤",
                Status = e.IsDeleted ? "حذف‌شده" : "فعال",
                Gender = e.Gender?.Title ?? "—",
                PerformanceValue = 0,
                PerformanceLabel = "—",
                AvatarUrl = PackIconKind.Account
            };
        }

        // ==================== فیلتر و صفحه‌بندی ====================

        private void ApplyFilter(bool resetPage)
        {
            var query = _all.AsEnumerable();

            // جستجوی سمت‌کلاینت روی نتایج بارگذاری‌شده
            // (اگر SearchAsync از سرور آمده، این فیلتر تکمیلی است)
            if (!string.IsNullOrWhiteSpace(_searchTerm))
            {
                query = query.Where(x =>
                    x.FullName.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    x.Position.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    x.Department.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    x.NationalCode.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    x.PersonnelCode.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    x.Email.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase));
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

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchTerm = SearchBox.Text?.Trim() ?? string.Empty;

            // برای عبارت‌های کوتاه فقط فیلتر حافظه؛ برای طولانی‌تر بارگذاری از سرور
            if (_searchTerm.Length >= 2)
                await LoadEmployeesAsync();
            else
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

        /// <summary>بارگذاری مجدد لیست از پایگاه داده (مثلاً بعد از ثبت کارمند جدید).</summary>
        public Task RefreshAsync() => LoadEmployeesAsync();
    }

    public class EmployeeModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NationalCode { get; set; } = string.Empty;
        public string PersonnelCode { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string DepartmentIcon { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public double PerformanceValue { get; set; }
        public string PerformanceLabel { get; set; } = string.Empty;
        public PackIconKind AvatarUrl { get; set; }
    }
}
