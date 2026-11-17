using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services;
using HR.Services.Data;

namespace HR.Controls.AddEmployees
{
    public partial class JobHistory : UserControl, IEmployeeSection
    {
        private const string NoneItem = "—";

        private readonly ObservableCollection<JobHistoryRecord> _items = new();

        public string SectionTitle => "سوابق شغلی";

        public JobHistory()
        {
            InitializeComponent();

            PersonaljobsList.ItemsSource = _items;
            _items.CollectionChanged += (_, __) => RowNumbering.Renumber(_items);

            Loaded += JobHistory_Loaded;
        }

        private async void JobHistory_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= JobHistory_Loaded;

            try
            {
                var reasons = (await LookupCache.GetAsync())[nameof(HR.Data.Entities.EmploymentEndReason)];
                if (reasons.Count == 0) return;

                JobEndReasonCombo.ItemsSource = new[] { NoneItem }
                    .Concat(reasons.Select(r => r.Title))
                    .ToList();

                JobEndReasonCombo.SelectedIndex = 0;
            }
            catch
            {
                // دیتابیس در دسترس نیست → لیست خالی می‌ماند
            }
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
            if (start == null)
            {
                Warn("تاریخ شروع را انتخاب کنید.");
                return;
            }
            if (start.HasValue && end.HasValue && end < start)
            {
                Warn("تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد.");
                return;
            }

            var reason = JobEndReasonCombo.SelectedItem as string;

            SectionList<JobHistoryRecord>.Commit(_items, PersonaljobsList, new JobHistoryRecord
            {
                Company = company,
                JobTitle = post,
                CompanyPhone = NullIfEmpty(phone),
                StartDate = start,
                EndDate = end,
                LeaveReason = string.IsNullOrWhiteSpace(reason) || reason == NoneItem ? null : reason,
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

        private void PersonaljobsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PersonaljobsList.SelectedItem is not JobHistoryRecord item) return;
            CompanyNameTB.Text = item.Company;
            PostNameTB.Text = item.JobTitle;
            JobFoneNumberTB.Text = item.CompanyPhone ?? string.Empty;
            JobStartDateDP.SelectedDate = item.StartDate;
            JobEndDateDP.SelectedDate = item.EndDate;
            LookupBinder.Select(JobEndReasonCombo, item.LeaveReason);
            JobNoteTB.Text = item.Note ?? string.Empty;
        }
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
            JobNoteTB.Clear();
            JobStartDateDP.SelectedDate = null;
            JobEndDateDP.SelectedDate = null;
            if (JobEndReasonCombo.Items.Count > 0) JobEndReasonCombo.SelectedIndex = 0;
            CompanyNameTB.Focus();
        }

        private static string? NullIfEmpty(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static void Warn(string text)
            => MessageBox.Show(text, "سوابق شغلی", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}