using System.Windows;
using HR.Services.Auth;

namespace HR.Pages
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _auth = new();
        private bool _bootstrap;

        public LoginWindow()
        {
            InitializeComponent();
            Loaded += async (_, _) => await PrepareAsync();
            Closed += (_, _) => _auth.Dispose();
        }

        private async System.Threading.Tasks.Task PrepareAsync()
        {
            try
            {
                _bootstrap = !await _auth.HasUsersAsync();
            }
            catch (Exception ex)
            {
                ErrorText.Text = "اتصال به پایگاه برقرار نشد. بلوک ارتقای LastPersonnel.sql را اجرا کنید."
                    + Environment.NewLine + ex.GetBaseException().Message;
                SubmitButton.IsEnabled = false;
                return;
            }

            if (!_bootstrap) return;

            SubtitleText.Text = "هیچ کاربری نیست. مدیر اولیه را بسازید.";
            SubmitButton.Content = "ساخت مدیر";
            DisplayNameLabel.Visibility = Visibility.Visible;
            DisplayNameBox.Visibility = Visibility.Visible;
            ConfirmLabel.Visibility = Visibility.Visible;
            ConfirmBox.Visibility = Visibility.Visible;
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = string.Empty;
            SubmitButton.IsEnabled = false;
            try
            {
                if (_bootstrap)
                {
                    if (!string.Equals(PasswordBox.Password, ConfirmBox.Password, StringComparison.Ordinal))
                    {
                        ErrorText.Text = "تکرار رمز عبور یکسان نیست.";
                        return;
                    }

                    var created = await _auth.CreateInitialAdminAsync(UsernameBox.Text, PasswordBox.Password, DisplayNameBox.Text);
                    AuthSession.SignIn(created);
                    DialogResult = true;
                    return;
                }

                var result = await _auth.LoginAsync(UsernameBox.Text, PasswordBox.Password);
                if (result.Outcome != LoginOutcome.Success || result.User == null)
                {
                    ErrorText.Text = result.Message;
                    return;
                }

                AuthSession.SignIn(result.User);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                ErrorText.Text = ex.Message;
            }
            finally
            {
                SubmitButton.IsEnabled = true;
            }
        }
    }
}
