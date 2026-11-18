using HR.Data.Entities;

namespace HR.Services.Data
{
    /// <summary>
    /// قواعد فرم افزودن/ویرایش: کد پستی حتی بدون متن نشانی ذخیره می‌شود
    /// و سمت همان عنوان شغلی ستون PositionTitle است.
    /// </summary>
    public static class EmployeeFormFields
    {
        public const string PostalOnlyAddress = "—";

        public static Address? AddressFromForm(Guid employeeId, string? addressText, string? addressTitle, string? postalCode)
        {
            var text = addressText?.Trim();
            var code = postalCode?.Trim();
            var hasText = !string.IsNullOrWhiteSpace(text) && text != PostalOnlyAddress;
            var hasPostal = !string.IsNullOrWhiteSpace(code);
            if (!hasText && !hasPostal) return null;

            return new Address
            {
                AddressId = Guid.NewGuid(),
                EmployeeId = employeeId,
                Title = string.IsNullOrWhiteSpace(addressTitle) ? "نشانی" : addressTitle.Trim(),
                AddressText = hasText ? text! : PostalOnlyAddress,
                PostalCode = hasPostal ? code : null,
                RelatedPersonId = null
            };
        }

        public static (string AddressText, string? PostalCode) ReadAddress(Address? address)
        {
            if (address == null) return (string.Empty, null);
            var text = address.AddressText?.Trim();
            if (string.IsNullOrWhiteSpace(text) || text == PostalOnlyAddress) text = string.Empty;
            var postal = string.IsNullOrWhiteSpace(address.PostalCode) ? null : address.PostalCode.Trim();
            return (text, postal);
        }

        /// <summary>جعبه «سمت» در فرم همان PositionTitle است.</summary>
        public static string? PostFromTitle(string? positionTitle)
            => string.IsNullOrWhiteSpace(positionTitle) ? null : positionTitle.Trim();
    }
}
