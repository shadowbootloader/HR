using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HR.Controls;
using MaterialDesignThemes.Wpf;

namespace HR.Pages
{
    public partial class EmployeeListPage : Page , INotifyPropertyChanged
    {
        public event Action<Page> NavigationRequested;

        public ObservableCollection<EmployeeModel> Employees { get; set; }

        // ================== تنظیمات ==================
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _pageSize = 10;          // تعداد آیتم در هر صفحه

        public int CurrentPage => _currentPage;
        public int TotalPages => _totalPages;

        // ایونت برای اینکه لیستت رو آپدیت کنی
        public event Action<int> PageChanged;


        int _FromIndex;
        int _ToIndex;
        int _TotalCount;
        public int FromIndex 
        { 
            get
            {
                return _FromIndex;
            }
            set {
                _FromIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PaginationText));
            } 
        }
        public int ToIndex
        {
            get
            {
                return _ToIndex;
            }
            set
            {
                _ToIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PaginationText));
            }
        }
        public int TotalCount
        {
            get
            {
                return _TotalCount;
            }
            set
            {
                _TotalCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PaginationText));
            }
        }

        public int PagesCount;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public string PaginationText => $"نمایش {FromIndex} تا {ToIndex} از {TotalCount} کارکنان";


        

        public EmployeeListPage()
        {
            InitializeComponent();
            
            DataContext = this;
            InitializePagination(248, 10);
            LoadData(0);
            EmployeeItems.ItemsSource = Employees;
            
            
            PageChanged += (page) =>
            {
                // اینجا لیستت رو با صفحه جدید لود کن
                LoadData(page-1);
            };
        }

        // ================== فراخوانی اولیه ==================
        /// <summary>
        /// این متد رو بعد از لود شدن داده‌ها صدا بزن
        /// </summary>
        /// <param name="totalItems">تعداد کل آیتم‌های لیست</param>
        /// <param name="pageSize">تعداد آیتم در هر صفحه</param>
        public void InitializePagination(int totalItems, int pageSize = 10)
        {
            _pageSize = pageSize;
            _totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            _currentPage = 1;

            UpdatePaginationUI();
        }

        // ================== آپدیت ظاهر Pagination ==================
        private void UpdatePaginationUI()
        {
            // دکمه قبلی
            BtnPrevious.IsEnabled = _currentPage > 1;
            BtnPrevious.Opacity = BtnPrevious.IsEnabled ? 1 : 0.4;

            // دکمه بعدی
            BtnNext.IsEnabled = _currentPage < _totalPages;
            BtnNext.Opacity = BtnNext.IsEnabled ? 1 : 0.4;

            // ------------------ منطق نمایش صفحات ------------------
            if (_totalPages <= 1)
            {
                BtnCurrent.Content = "1";
                BtnPage2.Visibility = Visibility.Collapsed;
                BtnPage3.Visibility = Visibility.Collapsed;
                TxtDots.Visibility = Visibility.Collapsed;
                BtnLast.Visibility = Visibility.Collapsed;
            }
            else if (_totalPages == 2)
            {
                BtnCurrent.Content = "1";
                BtnPage2.Content = "2";
                BtnPage2.Visibility = Visibility.Visible;
                BtnPage3.Visibility = Visibility.Collapsed;
                TxtDots.Visibility = Visibility.Collapsed;
                BtnLast.Visibility = Visibility.Collapsed;
            }
            else if (_totalPages == 3)
            {
                BtnCurrent.Content = "1";
                BtnPage2.Content = "2";
                BtnPage3.Content = "3";
                BtnPage2.Visibility = Visibility.Visible;
                BtnPage3.Visibility = Visibility.Visible;
                TxtDots.Visibility = Visibility.Collapsed;
                BtnLast.Visibility = Visibility.Collapsed;
            }
            else
            {
                // بیشتر از ۳ صفحه
                if (_currentPage <= 2)
                {
                    // اول صفحات
                    BtnCurrent.Content = "1";
                    BtnPage2.Content = "2";
                    BtnPage3.Content = "3";

                    BtnPage2.Visibility = Visibility.Visible;
                    BtnPage3.Visibility = Visibility.Visible;
                    TxtDots.Visibility = Visibility.Visible;
                    BtnLast.Content = _totalPages.ToString();
                    BtnLast.Visibility = Visibility.Visible;
                }
                else if (_currentPage >= _totalPages - 2)
                {
                    // نزدیک به آخر (اینجا اصلاح شد)
                    BtnCurrent.Content = (_totalPages - 2).ToString();
                    BtnPage2.Content = (_totalPages - 1).ToString();
                    BtnPage3.Content = _totalPages.ToString();

                    BtnPage2.Visibility = Visibility.Visible;
                    BtnPage3.Visibility = Visibility.Visible;
                    TxtDots.Visibility = Visibility.Collapsed;   // نقطه‌چین لازم نیست
                    BtnLast.Visibility = Visibility.Collapsed;   // دیگه دکمه Last نشون نده
                }
                else
                {
                    // حالت وسط
                    BtnCurrent.Content = (_currentPage - 1).ToString();
                    BtnPage2.Content = _currentPage.ToString();
                    BtnPage3.Content = (_currentPage + 1).ToString();

                    BtnPage2.Visibility = Visibility.Visible;
                    BtnPage3.Visibility = Visibility.Visible;
                    TxtDots.Visibility = Visibility.Visible;
                    BtnLast.Content = _totalPages.ToString();
                    BtnLast.Visibility = Visibility.Visible;
                }
            }

            HighlightCurrentPage();
        }

        private void HighlightCurrentPage()
        {
            ResetButtonStyle(BtnCurrent);
            ResetButtonStyle(BtnPage2);
            ResetButtonStyle(BtnPage3);
            ResetButtonStyle(BtnLast);

            string current = _currentPage.ToString();

            if (BtnCurrent.Content.ToString() == current)
                SetActiveStyle(BtnCurrent);
            else if (BtnPage2.Content.ToString() == current)
                SetActiveStyle(BtnPage2);
            else if (BtnPage3.Content.ToString() == current)
                SetActiveStyle(BtnPage3);
            else if (BtnLast.Content.ToString() == current)
                SetActiveStyle(BtnLast);
        }

        private void SetActiveStyle(Button btn)
        {
            btn.Background = new SolidColorBrush(Color.FromRgb(0x3F, 0x8C, 0xFF));
            btn.FontWeight = FontWeights.SemiBold;
        }

        private void ResetButtonStyle(Button btn)
        {
            btn.Background = new SolidColorBrush(Color.FromArgb(0x22, 0xFF, 0xFF, 0xFF));
            btn.FontWeight = FontWeights.Normal;
        }

        // ================== رویدادها ==================
        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdatePaginationUI();
                PageChanged?.Invoke(_currentPage);
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                UpdatePaginationUI();
                PageChanged?.Invoke(_currentPage);
            }
        }

        private void PageButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Content.ToString(), out int page))
            {
                if (page != _currentPage)
                {
                    _currentPage = page;
                    UpdatePaginationUI();
                    PageChanged?.Invoke(_currentPage);
                }
            }
        }
        private void LoadData( int PageNumber)
        {
            TotalCount = 248;
            FromIndex = (PageNumber ) * _pageSize;
            if(FromIndex == 0)
            {
                FromIndex = 1;
            }
            if(PageNumber != (_totalPages - 1))
            {
                ToIndex = (PageNumber+1) * _pageSize;
            }
            else
            {
                ToIndex = TotalCount ;
            }
            Employees = new ObservableCollection<EmployeeModel>
{
    new EmployeeModel
    {
        FullName = "امیرحسین رضایی",
        Email = "amirhossein.rezaei@company.com",
        Position = "مدیر عملیات منابع انسانی",
        Department = "منابع انسانی",
        DepartmentIcon = "👥",
        Status = "فعال",
        PerformanceValue = 92,
        PerformanceLabel = "عالی",

        AvatarUrl = PackIconKind.Account
    },

    new EmployeeModel
    {
        FullName = "محمدرضا احمدی",
        Email = "mohammadreza.ahmadi@company.com",
        Position = "مهندس نرم‌افزار",
        Department = "فناوری اطلاعات",
        DepartmentIcon = "</>",
        Status = "فعال",
        PerformanceValue = 88,
        PerformanceLabel = "بسیار خوب",

        AvatarUrl = PackIconKind.Account
    },

    new EmployeeModel
    {
        FullName = "سعید کریمی",
        Email = "saeed.karimi@company.com",
        Position = "طراح تجربه و رابط کاربری",
        Department = "طراحی",
        DepartmentIcon = "🎨",
        Status = "در مرخصی",
        PerformanceValue = 76,
        PerformanceLabel = "خوب",

        AvatarUrl = PackIconKind.Account
    },

    new EmployeeModel
    {
        FullName = "رضا محمدی",
        Email = "reza.mohammadi@company.com",
        Position = "کارشناس جذب و استخدام",
        Department = "منابع انسانی",
        DepartmentIcon = "👥",
        Status = "فعال",
        PerformanceValue = 81,
        PerformanceLabel = "بسیار خوب",

        AvatarUrl = PackIconKind.Account
    },

    new EmployeeModel
    {
        FullName = "علی اکبری",
        Email = "ali.akbari@company.com",
        Position = "تحلیلگر مالی",
        Department = "امور مالی",
        DepartmentIcon = "📊",
        Status = "فعال",
        PerformanceValue = 90,
        PerformanceLabel = "عالی",

        AvatarUrl = PackIconKind.Account
    },

    new EmployeeModel
    {
        FullName = "مهدی حسینی",
        Email = "mehdi.hosseini@company.com",
        Position = "مدیر بازاریابی",
        Department = "بازاریابی",
        DepartmentIcon = "📢",
        Status = "در مرخصی",
        PerformanceValue = 65,
        PerformanceLabel = "نیازمند بهبود",

        AvatarUrl = PackIconKind.Account
    },

    new EmployeeModel
    {
        FullName = "پارسا نادری",
        Email = "parsa.naderi@company.com",
        Position = "مدیر موفقیت مشتریان",
        Department = "ارتباط با مشتریان",
        DepartmentIcon = "💙",
        Status = "فعال",
        PerformanceValue = 85,
        PerformanceLabel = "بسیار خوب",

        AvatarUrl = PackIconKind.Account
    }
};
        }

        void RetriveData(int PageNumber)
        {

        }

        private void SummaryCardControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // فیلتر لیست
        }

        private void StatusFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // فیلتر وضعیت
        }
        private void BtnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            NavigationRequested?.Invoke(new AddEmployeePage());
        }
    }

    // مدل ساده کارمند
    public class EmployeeModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public string DepartmentIcon { get; set; }
        public string Status { get; set; }
        public double PerformanceValue { get; set; }
        public string PerformanceLabel { get; set; }
        public PackIconKind AvatarUrl { get; set; }
    }
}