using HR.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HR.Services.Data
{
    /// <summary>
    /// تبدیل مقادیر فرم به کلید خارجی واقعی.
    /// عمومی است: برای هر جدول Lookup بدون افزودن متد جدید کار می‌کند.
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

        // ==================== enum → Id بر اساس اندیس ردیف DB ====================

        /// <summary>
        /// مقدار enum را به Id ردیف هم‌اندیس تبدیل می‌کند.
        /// <paramref name="offset"/> برای enumهایی است که اولین عضوشان «نامشخص» است.
        /// </summary>
        public Task<byte> FromEnumAsync<TEntity, TEnum>(TEnum value, int offset = 0)
            where TEntity : class
            where TEnum : struct, Enum
        {
            var index = Convert.ToInt32(value) - offset;
            return FromIndexAsync<TEntity>(index);
        }

        public Task<byte> FromIndexAsync<TEntity>(int index) where TEntity : class
        {
            var rows = _cache.Get<TEntity>();

            if (rows.Count == 0)
                throw new InvalidOperationException($"جدول «{typeof(TEntity).Name}» خالی است؛ ابتدا آن را پر کنید.");

            if (index < 0) index = 0;
            if (index >= rows.Count) index = rows.Count - 1;

            return Task.FromResult(checked((byte)rows[index].Id));
        }

        // ==================== عنوان متنی → Id ====================

        public async Task<byte> ByteFromTitleAsync<TEntity>(string? title) where TEntity : class
            => checked((byte)await _cache.ResolveOrCreateAsync<TEntity>(title));

        public Task<short> IdFromTitleAsync<TEntity>(string? title) where TEntity : class
            => _cache.ResolveOrCreateAsync<TEntity>(title);

        // ==================== موقعیت جغرافیایی ====================

        public Task<HierarchyId?> EducationLocationAsync(string? province, string? city)
            => _geo.ResolveLocationNodeAsync(province, city);

        public Task<HierarchyId?> CountryNodeAsync(string? country)
            => _geo.FindCountryNodeAsync(country);

        public async Task<int> CountryAsync(string? title)
        {
            var country = await _geo.FindCountryAsync(title);
            if (country != null) return country.GeoId;

            var first = (await _geo.GetCountriesAsync()).FirstOrDefault();
            if (first != null) return first.GeoId;

            throw new InvalidOperationException("جدول Geo هیچ کشوری ندارد؛ ابتدا کشورها را وارد کنید.");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _geo.Dispose();
            _db.Dispose();
        }
    }
}