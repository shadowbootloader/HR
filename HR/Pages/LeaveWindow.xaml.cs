using System.Windows;
using HR.Services.Data;

namespace HR.Pages
{
    public partial class LeaveWindow : Window
    {
        private readonly Guid _employeeId;
        private readonly LeaveService _leaves = new();
        private Guid? _openId;

        public LeaveWindow(Guid employeeId, string fullName)
        {
            InitializeComponent();
            _employeeId = employeeId;
            NameText.Text = fullName;
            TypeBox.ItemsSource = LeaveService.Types;
            TypeBox.SelectedIndex = 0;
            StartPicker.SelectedDate = DateTime.Today;
            EndPicker.SelectedDate = DateTime.Today;
            Loaded += async (_, _) => await LoadCurrentAsync();
            Closed += (_, _) => _leaves.Dispose();
        }

        private async System.Threading.Tasks.Task LoadCurrentAsync()
        {
            var open = await _leaves.OpenAsync(_employeeId, DateTime.Today);
            _openId = open?.LeaveId;
            if (open == null)
            {
                CurrentText.Text = "مرخصی بازی ثبت نشده است.";
                EndButton.Visibility = Visibility.Collapsed;
                return;
            }

            CurrentText.Text = $"مرخصی {open.LeaveType} از {open.StartDate:yyyy/MM/dd} تا {open.EndDate:yyyy/MM/dd}";
            EndButton.Visibility = Visibility.Visible;
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = string.Empty;
            try
            {
                if (StartPicker.SelectedDate is not DateTime start || EndPicker.SelectedDate is not DateTime end)
                {
                    ErrorText.Text = "تاریخ شروع و پایان لازم است.";
                    return;
                }

                await _leaves.RegisterAsync(_employeeId, TypeBox.SelectedItem as string ?? "", start, end);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                ErrorText.Text = ex.Message;
            }
        }

        private async void End_Click(object sender, RoutedEventArgs e)
        {
            if (_openId == null) return;
            try
            {
                await _leaves.EndAsync(_openId.Value);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                ErrorText.Text = ex.Message;
            }
        }
    }
}
