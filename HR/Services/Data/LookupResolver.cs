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

        // ==================== عنوان متنی → Id (بدون حدس بر اساس ترتیب ردیف) ====================

        public async Task<byte> ByteFromTitleAsync<TEntity>(string? title) where TEntity : class
            => checked((byte)await _cache.ResolveOrCreateAsync<TEntity>(title));

        public async Task<byte?> ByteFromTitleOrNullAsync<TEntity>(string? title) where TEntity : class
        {
            if (string.IsNullOrWhiteSpace(title) || title.Trim() == "—") return null;
            return await ByteFromTitleAsync<TEntity>(title);
        }

        public Task<short> IdFromTitleAsync<TEntity>(string? title) where TEntity : class
            => _cache.ResolveAsync(typeof(TEntity), title, createIfMissing: false);

        public Task<short> IdFromUserCatalogAsync<TEntity>(string? title) where TEntity : class
            => _cache.ResolveOrCreateAsync<TEntity>(title);

        /// <summary>اولین ردیفی که عنوانش یکی از عبارت‌ها را دارد. اگر نبود خطا می‌دهد.</summary>
        public byte MatchTitle<TEntity>(IEnumerable<string> fragments, string label) where TEntity : class
        {
            var rows = _cache.Get<TEntity>();
            if (rows.Count == 0)
                throw new InvalidOperationException($"جدول {typeof(TEntity).Name} خالی است. {label}");

            var accepted = fragments.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
            var match = rows.FirstOrDefault(row =>
                accepted.Any(fragment => row.Title.Contains(fragment, StringComparison.OrdinalIgnoreCase)));

            if (match == null)
                throw new InvalidOperationException(
                    $"{label}: عنوانی شامل ({string.Join("، ", accepted)}) در جدول {typeof(TEntity).Name} نیست.");

            return checked((byte)match.Id);
        }

        // ==================== موقعیت جغرافیایی ====================

        public Task<HierarchyId?> EducationLocationAsync(string? province, string? city)
            => _geo.ResolveLocationNodeAsync(province, city);

        public Task<HierarchyId?> CountryNodeAsync(string? country)
            => _geo.FindCountryNodeAsync(country);

        public async Task<int> CountryAsync(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new InvalidOperationException("کشور انتخاب نشده است.");

            var country = await _geo.FindCountryAsync(title);
            if (country != null) return country.GeoId;

            throw new InvalidOperationException($"کشور «{title.Trim()}» در جدول Geo نیست.");
        }

        public Task<HierarchyId?> LocationByNameAsync(string? name)
            => _geo.FindLocationNodeByNameAsync(name);

        public Task<string?> LocationNameByNodeAsync(HierarchyId? node)
            => _geo.FindLocationNameByNodeAsync(node);

        public Task<string?> CountryNameByIdAsync(int countryId)
            => _geo.FindCountryNameByIdAsync(countryId);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _geo.Dispose();
            _db.Dispose();
        }
    }
}