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

        public static int Duration(string? text)
        {
            var digits = DigitsOnly(text);
            return int.TryParse(digits, out var hours) ? hours : 0;
        }

        public static decimal Decimal(string? text)
            => decimal.TryParse(text?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
                ? value
                : 0m;
    }
}