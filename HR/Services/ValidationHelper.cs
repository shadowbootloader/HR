using System.Windows.Controls;

namespace HR.Services
{
    /// <summary>
    /// نشان‌دادن/پاک‌کردن خطای اعتبارسنجی روی TextBox.
    /// نسخه قبلی پیام را در جایگاه «value» می‌گذاشت و ErrorContent نال می‌ماند،
    /// پس متن خطا هرگز نمایش داده نمی‌شد.
    /// </summary>
    public static class ValidationHelper
    {
        public static void SetError(TextBox textBox, string message)
        {
            var binding = textBox.GetBindingExpression(TextBox.TextProperty);
            if (binding == null) return;

            var error = new ValidationError(new MessageRule(message), binding)
            {
                ErrorContent = message
            };

            Validation.MarkInvalid(binding, error);
        }

        public static void ClearError(TextBox textBox)
        {
            var binding = textBox.GetBindingExpression(TextBox.TextProperty);
            if (binding == null) return;

            Validation.ClearInvalid(binding);
        }

        /// <summary>اگر خالی باشد خطا نشان می‌دهد و false برمی‌گرداند.</summary>
        public static bool CheckRequired(TextBox textBox, string message)
        {
            if (!string.IsNullOrWhiteSpace(textBox.Text))
            {
                ClearError(textBox);
                return true;
            }

            SetError(textBox, message);
            return false;
        }

        /// <summary>فقط بررسی خالی بودن، بدون دست‌زدن به UI.</summary>
        public static bool IsEmpty(TextBox textBox)
            => string.IsNullOrWhiteSpace(textBox.Text);

        private sealed class MessageRule : ValidationRule
        {
            private readonly string _message;

            public MessageRule(string message) => _message = message;

            public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
                => new(false, _message);
        }
    }
}