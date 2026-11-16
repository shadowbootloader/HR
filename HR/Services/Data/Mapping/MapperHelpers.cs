using System;
using System.Globalization;
using System.Linq;

namespace HR.Services.Data.Mapping
{
    internal static class MapperHelpers
    {
        public static string Required(string? value) => string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

        public static string? DigitsOnly(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : new string(value.Where(char.IsDigit).ToArray());

        public static string Sheba(string? value)
        {
            var digits = DigitsOnly(value);
            if (string.IsNullOrEmpty(digits)) return string.Empty;
            if (digits.Length > 24) digits = digits[..24];
            return "IR" + digits.PadLeft(24, '0');
        }

        public static string Card(string? value)
        {
            var digits = DigitsOnly(value);
            if (string.IsNullOrEmpty(digits)) return string.Empty;
            if (digits.Length > 16) digits = digits[..16];
            return digits.PadLeft(16, '0');
        }

        public static string? JoinNote(string? first, string? second)
        {
            var parts = new[] { first?.Trim(), second?.Trim() }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            return parts.Length == 0 ? null : string.Join(" — ", parts);
        }

        public static (string? First, string? Second) SplitJoined(string? joined)
        {
            if (string.IsNullOrWhiteSpace(joined)) return (null, null);
            var parts = joined.Split(" — ", 2, StringSplitOptions.None);
            return parts.Length == 2 ? (parts[0], parts[1]) : (joined, null);
        }

        public static int Duration(string? text)
        {
            var digits = DigitsOnly(text);
            return int.TryParse(digits, out var hours) ? hours : 0;
        }

        public static decimal Decimal(string? text)
            => decimal.TryParse(text?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
                ? value
                : 0m;

        /// <summary>نام کامل را به نام و نام خانوادگی می‌شکند (آخرین فاصله).</summary>
        public static (string First, string Last) SplitName(string? fullName)
        {
            var text = fullName?.Trim();
            if (string.IsNullOrEmpty(text)) return ("—", "—");

            var index = text.LastIndexOf(' ');
            if (index <= 0) return (text, "—");

            return (text[..index].Trim(), text[(index + 1)..].Trim());
        }

        /// <summary>کد ملی ساختگی ۱۰ رقمی برای بستگانی که کد ملی ندارند.</summary>
        public static string SyntheticNationalCode()
            => new string(Guid.NewGuid().ToByteArray()
                .Take(5)
                .Select(b => (char)('0' + (b % 10)))
                .ToArray());
    }
}
