using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using HR.Models;

namespace HR.Controls.AddEmployees
{
    public partial class JobHistory : UserControl, IEmployeeSection
    {
        private readonly ObservableCollection<JobHistoryRecord> _items = new();

        public string SectionTitle => "سوابق شغلی";

        public JobHistory()
        {
            InitializeComponent();

            PersonaljobsList.ItemsSource = _items;
            _items.CollectionChanged += (_, __) => RowNumbering.Renumber(_items);
        }

        private void AddJobDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var company = CompanyNameTB.Text?.Trim();
            var post = PostNameTB.Text?.Trim();

            if (string.IsNullOrWhiteSpace(company))
            {
                Warn("نام موسسه را وارد کنید.");
                CompanyNameTB.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(post))
            {
                Warn("سمت را وارد کنید.");
                PostNameTB.Focus();
                return;
            }

            var phone = JobFoneNumberTB.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(phone) && !Regex.IsMatch(phone, @"^\d{8,11}$"))
            {
                Warn("تلفن موسسه باید ۸ تا ۱۱ رقم باشد.");
                JobFoneNumberTB.Focus();
                return;
            }

            var start = JobStartDateDP.SelectedDate;
            var end = JobEndDateDP.SelectedDate;
            if (start.HasValue && end.HasValue && end < start)
            {
                Warn("تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد.");
                return;
            }

            _items.Add(new JobHistoryRecord
            {
                Company = company,
                JobTitle = post,
                CompanyPhone = NullIfEmpty(phone),
                StartDate = start,
                EndDate = end,
                LeaveReason = NullIfEmpty(JobEndReasonTB.Text),
                Note = NullIfEmpty(JobNoteTB.Text)
            });

            ClearForm();
        }

        private void DeletejobDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = PersonaljobsList.SelectedItems.Cast<JobHistoryRecord>().ToList();
            foreach (var item in selected)
                _items.Remove(item);
        }

        private void PersonaljobsList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void PersonaljobsList_SizeChanged(object sender, SizeChangedEventArgs e) { }

        // ==================== IEmployeeSection ====================

        public void LoadFrom(EmployeeProfile profile)
        {
            _items.Clear();
            foreach (var record in profile.JobHistories)
                _items.Add(record);

            RowNumbering.Renumber(_items);
        }

        public void SaveTo(EmployeeProfile profile)
        {
            profile.JobHistories.Clear();
            foreach (var record in _items)
                profile.JobHistories.Add(record);
        }

        public bool Validate(out string message)
        {
            foreach (var record in _items)
            {
                if (record.StartDate.HasValue && record.EndDate.HasValue && record.EndDate < record.StartDate)
                {
                    message = $"بازه تاریخ نامعتبر در ردیف {record.Row}";
                    return false;
                }
            }

            message = string.Empty;
            return true;
        }

        private void ClearForm()
        {
            CompanyNameTB.Clear();
            PostNameTB.Clear();
            JobFoneNumberTB.Clear();
            JobEndReasonTB.Clear();
            JobNoteTB.Clear();
            JobStartDateDP.SelectedDate = null;
            JobEndDateDP.SelectedDate = null;
            CompanyNameTB.Focus();
        }

        private static string? NullIfEmpty(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static void Warn(string text)
            => MessageBox.Show(text, "سوابق شغلی", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}