using System;
using System.Globalization;
using System.Linq;

namespace HR.Services.Data.Mapping
{
    internal static class MapperHelpers
    {
        public static string Required(string? value) => Required(value, "این مقدار");

        public static string Required(string? value, string field)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim() == "—")
                throw new InvalidOperationException($"{field} الزامی است.");
            return value.Trim();
        }

        public static string EmptyIfMissing(string? value)
            => string.IsNullOrWhiteSpace(value) || value.Trim() == "—" ? string.Empty : value.Trim();

        public static DateTime RequireDate(DateTime? value, string field)
        {
            if (value == null)
                throw new InvalidOperationException($"{field} الزامی است.");
            return value.Value;
        }

        public static string? DigitsOnly(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : new string(value.Where(char.IsDigit).ToArray());

        public static string Sheba(string? value)
        {
            var text = value?.Trim().ToUpperInvariant() ?? string.Empty;
            if (text.StartsWith("IR", StringComparison.Ordinal)) text = text[2..];
            var digits = new string(text.Where(char.IsDigit).ToArray());
            if (digits.Length != 24)
                throw new InvalidOperationException("شماره شبا باید ۲۴ رقم باشد.");
            return "IR" + digits;
        }

        public static string Card(string? value)
        {
            var digits = DigitsOnly(value);
            if (digits == null || digits.Length != 16)
                throw new InvalidOperationException("شماره کارت باید ۱۶ رقم باشد.");
            return digits;
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
            if (string.IsNullOrEmpty(text))
                throw new InvalidOperationException("نام و نام خانوادگی الزامی است.");

            var index = text.LastIndexOf(' ');
            if (index <= 0) return (text, text);

            return (text[..index].Trim(), text[(index + 1)..].Trim());
        }
    }
}
