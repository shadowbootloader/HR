using HR.Data;
using HR.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HR.Services.Data
{
    /// <summary>
    /// تبدیل مقادیر فرم به کلید خارجی واقعی.
    /// مقادیر enum با اندیس ردیف DB، فیلدهای متنی با عنوان و موقعیت‌ها با جدول Geo حل می‌شوند.
    /// </summary>
    public sealed class LookupResolver : IDisposable
    {
        private readonly PersonnelDbContext _db;
        private readonly LookupCache _cache;
        private readonly GeoService _geo;
        private bool _disposed;

        private LookupResolver(PersonnelDbContext db, LookupCache cache)
        {
            _db = db;
            _cache = cache;
            _geo = new GeoService(db);
        }

        public static async Task<LookupResolver> LoadAsync(PersonnelDbContext db)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            return new LookupResolver(db, await LookupCache.GetAsync());
        }

        // ==================== enum‌ها → اندیس ردیف DB ====================

        public async Task<byte> GenderAsync(Models.Gender v) => ToByte(await ByIndexAsync(LookupKeys.Gender, (int)v - 1));

        public async Task<byte> MaritalStatusAsync(Models.MaritalStatus v) => ToByte(await ByIndexAsync(LookupKeys.MaritalStatus, (int)v - 1));

        public async Task<byte?> MilitaryStatusAsync(Models.MilitaryStatus v)
        {
            if (v == Models.MilitaryStatus.Unspecified) return null;
            return ToByte(await ByIndexAsync(LookupKeys.MilitaryStatus, (int)v - 1));
        }

        public async Task<byte> DegreeAsync(DegreeLevel v) => ToByte(await ByIndexAsync(LookupKeys.EducationDegree, (int)v));

        public async Task<byte> EducationStatusAsync(EducationStatusKind v) => ToByte(await ByIndexAsync(LookupKeys.EducationStatus, (int)v));

        public async Task<byte> AccountTypeAsync(BankAccountType v) => ToByte(await ByIndexAsync(LookupKeys.AccountTypeBank, (int)v));

        public async Task<byte> PhoneKindAsync(PhoneKind v) => ToByte(await ByIndexAsync(LookupKeys.PhoneType, (int)v));

        public async Task<byte> SkillLevelAsync(Models.SkillLevel v) => ToByte(await ByIndexAsync(LookupKeys.SkillLevel, (int)v));

        /// <summary>جدول جداگانه‌ای برای سطح زبان نیست؛ روی SkillLevel نگاشت می‌شود.</summary>
        public async Task<byte> LanguageLevelAsync(LanguageLevel v) => ToByte(await ByIndexAsync(LookupKeys.SkillLevel, (int)v));

        // ==================== فیلدهای متنی آزاد ====================

        public async Task<byte> InsuranceTypeAsync(string? title) => ToByte(await _cache.ResolveOrCreateAsync(LookupKeys.InsuranceType, title));

        public async Task<byte> CooperationTypeAsync(string? title) => ToByte(await _cache.ResolveOrCreateAsync(LookupKeys.CooperationType, title));

        public async Task<byte> CitizenshipReasonAsync(string? title) => ToByte(await _cache.ResolveOrCreateAsync(LookupKeys.CitizenshipReason, title));

        public async Task<byte> RecordTypeAsync(string? title) => ToByte(await _cache.ResolveOrCreateAsync(LookupKeys.RecordType, title));

        public async Task<byte> PhoneStatusAsync(string? title) => ToByte(await _cache.ResolveOrCreateAsync(LookupKeys.PhoneStatus, title));

        public async Task<short> BankAsync(string? title) => await _cache.ResolveOrCreateAsync(LookupKeys.Bank, title);

        public async Task<short> LanguageAsync(string? title) => await _cache.ResolveOrCreateAsync(LookupKeys.Language, title);

        public async Task<short> SoftwareAsync(string? title) => await _cache.ResolveOrCreateAsync(LookupKeys.Software, title);

        public async Task<short> DepartmentAsync(string? title) => await _cache.ResolveOrCreateAsync(LookupKeys.Department, title);

        // ==================== موقعیت جغرافیایی ====================

        public Task<HierarchyId?> EducationLocationAsync(string? province, string? city)
            => _geo.ResolveLocationNodeAsync(province, city);

        public async Task<int> CountryAsync(string? title)
        {
            var country = await _geo.FindCountryAsync(title);
            if (country != null) return country.GeoId;

            var first = (await _geo.GetCountriesAsync()).FirstOrDefault();
            if (first != null) return first.GeoId;

            throw new InvalidOperationException("جدول Geo هیچ کشوری ندارد؛ ابتدا کشورها را وارد کنید.");
        }

        // ==================== هسته ====================

        private Task<short> ByIndexAsync(string tableKey, int index)
        {
            var rows = _cache[tableKey];

            if (rows.Count == 0)
                throw new InvalidOperationException($"جدول «{tableKey}» خالی است؛ ابتدا آن را پر کنید.");

            if (index < 0) return Task.FromResult(rows[0].Id);
            if (index >= rows.Count) return Task.FromResult(rows[^1].Id);

            return Task.FromResult(rows[index].Id);
        }

        private static byte ToByte(short value) => checked((byte)value);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _geo.Dispose();
            _db.Dispose();
        }
    }
}