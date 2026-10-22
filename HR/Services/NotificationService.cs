using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using HR.Controls;


namespace HR.Services
{
    public static class NotificationService
    {
        private static Panel? _host;

        private static readonly Queue<NotificationRequest>
            _queue = new();

        private static NeonNotification? _current;


        public static void Initialize(Panel host)
        {
            _host = host;
        }


        // ==========================================
        // SUCCESS
        // ==========================================

        public static void Success(
            string title,
            string message,
            double duration = 4)
        {
            Show(
                title,
                message,
                NeonNotification.NotificationType.Success,
                duration);
        }


        // ==========================================
        // ERROR
        // ==========================================

        public static void Error(
            string title,
            string message,
            double duration = 5)
        {
            Show(
                title,
                message,
                NeonNotification.NotificationType.Error,
                duration);
        }


        // ==========================================
        // WARNING
        // ==========================================

        public static void Warning(
            string title,
            string message,
            double duration = 5)
        {
            Show(
                title,
                message,
                NeonNotification.NotificationType.Warning,
                duration);
        }


        // ==========================================
        // INFO
        // ==========================================

        public static void Info(
            string title,
            string message,
            double duration = 4)
        {
            Show(
                title,
                message,
                NeonNotification.NotificationType.Info,
                duration);
        }


        // ==========================================
        // SHOW
        // ==========================================

        public static void Show(
            string title,
            string message,
            NeonNotification.NotificationType type,
            double duration)
        {
            if (_host == null)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[NotificationService] Initialize() صدا زده نشده؛ پیام نمایش داده نشد.");
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                var request = new NotificationRequest
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    Duration = duration
                };

                _queue.Enqueue(request);

                ShowNext();
            });
        }


        // ==========================================
        // QUEUE
        // ==========================================

        private static void ShowNext()
        {
            if (_current != null)
                return;

            if (_queue.Count == 0)
                return;

            var request = _queue.Dequeue();

            _current = new NeonNotification();

            _current.HorizontalAlignment =
                HorizontalAlignment.Right;

            _current.VerticalAlignment =
                VerticalAlignment.Bottom;

            _current.Margin = new Thickness(0, 0, 25, 25);

            _current.Closed += Current_Closed;

            _host.Children.Add(_current);

            _current.Show(
                request.Title,
                request.Message,
                request.Type,
                request.Duration);
        }


        private static void Current_Closed(
            object sender,
            EventArgs e)
        {
            if (_current != null)
            {
                _host.Children.Remove(_current);

                _current.Closed -= Current_Closed;

                _current = null;
            }

            ShowNext();
        }


        private class NotificationRequest
        {
            public string Title { get; set; }

            public string Message { get; set; }

            public NeonNotification.NotificationType Type { get; set; }

            public double Duration { get; set; }
        }
    }
}

////
//NotificationService.Success(
//    "ثبت موفق",
//    "اطلاعات کارمند با موفقیت ثبت شد.");
//
//NotificationService.Error(
//    "خطا",
//    "در ذخیره اطلاعات مشکلی به وجود آمد.");
//
//NotificationService.Warning(
//    "توجه",
//    "برخی از اطلاعات کارمند تکمیل نشده است.");
//
//NotificationService.Info(
//    "اطلاع‌رسانی",
//    "اطلاعات جدیدی برای شما وجود دارد.");