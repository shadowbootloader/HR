using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using HR.Services.Files;

namespace HR.Services
{
    /// <summary>
    /// انتخاب فایل از دیسک و مرحله‌سازی آن در ریشه ذخیره قابل تنظیم.
    /// مسیر نسبی برمی‌گردد تا بعد از ذخیره به پوشه کارمند یا دپارتمان منتقل شود.
    /// </summary>
    public static class ImagePicker
    {
        public static string? Pick() => Choose(FileKind.Image,
            "تصاویر (*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp)|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp");

        public static string? PickDocument() => Choose(FileKind.Document,
            "مدارک (*.jpg;*.jpeg;*.png;*.bmp;*.pdf)|*.jpg;*.jpeg;*.png;*.bmp;*.pdf");

        public static void Preview(Image target, string? storedName)
        {
            if (target == null) return;
            target.Source = Load(storedName);
        }

        /// <summary>BitmapImage از مسیر نسبی ذخیره‌شده. فایل نامعتبر تصویر خالی برمی‌گرداند تا فهرست نشکند.</summary>
        public static BitmapImage? Load(string? storedName)
        {
            var fullPath = FileStorageService.Load().FullPath(storedName);
            if (fullPath.Length == 0 || !File.Exists(fullPath) || !IsImage(fullPath))
                return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception ex) when (ex is IOException or NotSupportedException or System.Runtime.InteropServices.ExternalException)
            {
                return null;
            }
        }

        private static string? Choose(FileKind kind, string filter)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "انتخاب فایل",
                Filter = filter,
                CheckFileExists = true
            };

            if (dialog.ShowDialog() != true) return null;
            return FileStorageService.Load().Stage(dialog.FileName, kind).RelativePath;
        }

        private static bool IsImage(string path)
        {
            var extension = Path.GetExtension(path);
            return extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".gif", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".webp", StringComparison.OrdinalIgnoreCase);
        }
    }
}
