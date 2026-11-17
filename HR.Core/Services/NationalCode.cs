namespace HR.Services
{
    /// <summary>رقم کنترلی کد ملی ۱۰ رقمی ایران.</summary>
    public static class NationalCode
    {
        public static bool IsValid(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != 10)
                return false;

            foreach (var ch in code)
            {
                if (ch < '0' || ch > '9') return false;
            }

            var allSame = true;
            for (var i = 1; i < code.Length; i++)
            {
                if (code[i] != code[0])
                {
                    allSame = false;
                    break;
                }
            }
            if (allSame) return false;

            var sum = 0;
            for (var i = 0; i < 9; i++)
                sum += (code[i] - '0') * (10 - i);

            var remainder = sum % 11;
            var check = remainder < 2 ? remainder : 11 - remainder;
            return code[9] - '0' == check;
        }
    }
}
