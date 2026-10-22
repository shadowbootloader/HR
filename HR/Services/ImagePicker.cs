using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace HR.Services
{
    /// <summary>
    /// انتخاب تصویر از دیسک، کپی آن در پوشه رسانه برنامه و نمایش پیش‌نمایش.
    /// فقط نام فایل ذخیره می‌شود (نه مسیر کامل) تا در SQL Server قابل نگهداری باشد.
    /// </summary>
    public static class ImagePicker
    {
        private const string AppFolderName = "HRSuite";
        private const string MediaFolderName = "Media";

        public static string MediaRoot =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                AppFolderName,
                MediaFolderName);

        /// <summary>انتخاب تصویر؛ در صورت انصراف null برمی‌گرداند.</summary>
        public static string? Pick()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "انتخاب تصویر",
                Filter = "تصاویر (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                CheckFileExists = true
            };

            return dialog.ShowDialog() == true ? Store(dialog.FileName) : null;
        }

        /// <summary>کپی فایل در پوشه رسانه و برگرداندن نام فایل ذخیره‌شده.</summary>
        public static string Store(string sourcePath)
        {
            Directory.CreateDirectory(MediaRoot);

            var extension = Path.GetExtension(sourcePath);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var target = Path.Combine(MediaRoot, fileName);

            File.Copy(sourcePath, target, overwrite: true);
            return fileName;
        }

        /// <summary>نمایش تصویر ذخیره‌شده روی کنترل مقصد.</summary>
        public static void Preview(Image target, string? storedName)
        {
            if (target == null) return;

            var fullPath = FullPath(storedName);

            if (fullPath.Length == 0 || !File.Exists(fullPath))
            {
                target.Source = null;
                return;
            }

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();

            target.Source = bitmap;
        }

        public static string FullPath(string? storedName)
            => string.IsNullOrWhiteSpace(storedName)
                ? string.Empty
                : Path.Combine(MediaRoot, storedName);
    }
}