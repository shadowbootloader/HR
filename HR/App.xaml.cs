using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using HR.Pages;

namespace HR
{
    public partial class App : Application
    {
        /// <summary>فونت پیش‌فرض همه‌ی متن‌ها: B Nazanin از منابع برنامه.</summary>
        private static void ApplyDefaultFont()
        {
            var font = new FontFamily("pack://application:,,,/Fonts/#B Nazanin");
            TextElement.FontFamilyProperty.OverrideMetadata(typeof(TextElement), new FrameworkPropertyMetadata(font));
            TextBlock.FontFamilyProperty.OverrideMetadata(typeof(TextBlock), new FrameworkPropertyMetadata(font));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ApplyDefaultFont();
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
            ShowLogin();
        }

        private void ShowLogin()
        {
            var login = new LoginWindow();
            var accepted = login.ShowDialog() == true;
            if (!accepted)
            {
                Shutdown();
                return;
            }

            var main = new MainWindow();
            MainWindow = main;
            main.Closed += (_, _) =>
            {
                if (main.LogoutRequested)
                    ShowLogin();
                else
                    Shutdown();
            };
            main.Show();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(e.Exception);

            MessageBox.Show(
                e.Exception.Message,
                "خطای پیش‌بینی‌نشده",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            e.Handled = true;
        }

        private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
                Debug.WriteLine(exception);
        }
    }
}