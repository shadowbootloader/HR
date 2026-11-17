using HR.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HR.Services.Data
{
    /// <summary>یک ردیف از جدول Lookup (Id + عنوان فارسی که از DB آمده).</summary>
    public sealed record LookupItem(short Id, string Title)
    {
        public override string ToString() => Title;
    }

    /// <summary>
    /// نام کلاس‌های Entity که به‌عنوان Lookup استفاده می‌شوند.
    /// کلیدها با نام تایپ یکی هستند تا از سمت UI قابل ارجاع باشند.
    /// </summary>
    public static class LookupKeys
    {
        public const string Gender = nameof(HR.Data.Entities.Gender);
        public const string MaritalStatus = nameof(HR.Data.Entities.MaritalStatus);
        public const string MilitaryStatus = nameof(HR.Data.Entities.MilitaryStatus);
        public const string EducationDegree = nameof(HR.Data.Entities.EducationDegree);
        public const string EducationStatus = nameof(HR.Data.Entities.EducationStatus);
        public const string AccountTypeBank = nameof(HR.Data.Entities.AccountTypeBank);
        public const string PhoneType = nameof(HR.Data.Entities.PhoneType);
        public const string PhoneStatus = nameof(HR.Data.Entities.PhoneStatus);
        public const string SkillLevel = nameof(HR.Data.Entities.SkillLevel);
        public const string InsuranceType = nameof(HR.Data.Entities.InsuranceType);
        public const string CooperationType = nameof(HR.Data.Entities.CooperationType);
        public const string Bank = nameof(HR.Data.Entities.Bank);
        public const string Language = nameof(HR.Data.Entities.Language);
        public const string Software = nameof(HR.Data.Entities.Software);
        public const string Department = nameof(HR.Data.Entities.Department);
        public const string RecordType = nameof(HR.Data.Entities.RecordType);
        public const string TransportType = nameof(HR.Data.Entities.TransportType);
        public const string TravelReason = nameof(HR.Data.Entities.TravelReason);
        public const string AbroadReason = nameof(HR.Data.Entities.AbroadReason);
        public const string SacrificeType = nameof(HR.Data.Entities.SacrificeType);
        public const string SacrificeRelationship = nameof(HR.Data.Entities.SacrificeRelationship);
        public const string AmountUnit = nameof(HR.Data.Entities.AmountUnit);
        public const string CenterType = nameof(HR.Data.Entities.CenterType);
        public const string SectorType = nameof(HR.Data.Entities.SectorType);
        public const string CitizenshipReason = nameof(HR.Data.Entities.CitizenshipReason);
        public const string HiringResult = nameof(HR.Data.Entities.HiringResult);
        public const string ExemptionType = nameof(HR.Data.Entities.ExemptionType);
        public const string ExemptionReason = nameof(HR.Data.Entities.ExemptionReason);
        public const string Religion = nameof(HR.Data.Entities.Religion);
        public const string Denomination = nameof(HR.Data.Entities.Denomination);
    }

    /// <summary>
    /// کش یک‌باره‌ی همه‌ی جداول Lookup.
    /// جداول از مدل EF کشف می‌شوند: هر Entity که ستون متنی «Title» و کلید عددی «{نامEntity}Id» دارد.
    /// با افزودن جدول Lookup جدید به دیتابیس، بدون تغییر کد در کش حاضر می‌شود.
    /// </summary>
    public sealed class LookupCache : IDisposable
    {
        private static LookupCache? _current;
        private static readonly SemaphoreSlim _gate = new(1, 1);

        private static readonly MethodInfo LoaderMethod =
            typeof(LookupCache).GetMethod(nameof(LoadTableAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

        private static readonly MethodInfo CreatorMethod =
            typeof(LookupCache).GetMethod(nameof(CreateRowAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

        private readonly PersonnelDbContext _db;
        private readonly Dictionary<Type, List<LookupItem>> _tables = new();
        private readonly Dictionary<string, Type> _typesByName = new(StringComparer.OrdinalIgnoreCase);
        private bool _disposed;

        private LookupCache(PersonnelDbContext db) => _db = db;

        public static async Task<LookupCache> GetAsync()
        {
            if (_current != null) return _current;

            await _gate.WaitAsync();
            try
            {
                if (_current == null)
                    _current = await LoadAsync();

                return _current;
            }
            finally
            {
                _gate.Release();
            }
        }

        public static async Task ReloadAsync()
        {
            await _gate.WaitAsync();
            try
            {
                _current?.Dispose();
                _current = null;
            }
            finally
            {
                _gate.Release();
            }
        }

        private static async Task<LookupCache> LoadAsync()
        {
            var db = new PersonnelDbContext();
            var cache = new LookupCache(db);

            foreach (var clr in db.Model.GetEntityTypes().Select(e => e.ClrType).Distinct())
            {
                if (!IsLookup(clr)) continue;

                var task = (Task<List<LookupItem>>)LoaderMethod
                    .MakeGenericMethod(clr)
                    .Invoke(null, new object[] { db })!;

                cache._tables[clr] = await task;
                cache._typesByName[clr.Name] = clr;
            }

            return cache;
        }

        // ==================== کشف ساختار جدول ====================

        private static bool IsLookup(Type clr)
        {
            var title = clr.GetProperty("Title");
            if (title == null || title.PropertyType != typeof(string)) return false;

            return KeyProperty(clr) != null;
        }

        /// <summary>
        /// کلید جدول Lookup. در این دیتابیس همه‌ی این جداول الگوی «{نامEntity}Id» دارند،
        /// پس ستون‌های عددی دیگر (مثل DurationHours در Course) کلید حساب نمی‌شوند.
        /// </summary>
        private static PropertyInfo? KeyProperty(Type clr)
        {
            var byName = clr.GetProperty(clr.Name + "Id");
            return byName != null && IsInteger(byName.PropertyType) ? byName : null;
        }

        private static bool IsInteger(Type type)
            => type == typeof(byte) || type == typeof(short) || type == typeof(int);

        private static async Task<List<LookupItem>> LoadTableAsync<T>(PersonnelDbContext db) where T : class
        {
            var key = KeyProperty(typeof(T))!;
            var title = typeof(T).GetProperty("Title")!;

            var rows = await db.Set<T>().AsNoTracking().ToListAsync();

            return rows
                .Select(row => new LookupItem(
                    Convert.ToInt16(key.GetValue(row)),
                    (string?)title.GetValue(row) ?? string.Empty))
                .Where(x => !string.IsNullOrWhiteSpace(x.Title))
                .OrderBy(x => x.Id)
                .ToList();
        }

        private static async Task<LookupItem?> CreateRowAsync<T>(PersonnelDbContext db, string title) where T : class, new()
        {
            var row = new T();
            typeof(T).GetProperty("Title")!.SetValue(row, title);

            db.Set<T>().Add(row);
            await db.SaveChangesAsync();

            var id = Convert.ToInt16(KeyProperty(typeof(T))!.GetValue(row));
            return new LookupItem(id, title);
        }

        // ==================== خواندن ====================

        public IReadOnlyList<LookupItem> Get<TEntity>() where TEntity : class
            => _tables.TryGetValue(typeof(TEntity), out var rows) ? rows : Array.Empty<LookupItem>();

        public IReadOnlyList<LookupItem> this[string tableKey]
            => _typesByName.TryGetValue(tableKey, out var clr) && _tables.TryGetValue(clr, out var rows)
                ? rows
                : Array.Empty<LookupItem>();

        // ==================== حل یا ساخت ====================

        public Task<short> ResolveOrCreateAsync<TEntity>(string? title) where TEntity : class
            => ResolveAsync(typeof(TEntity), title, createIfMissing: IsUserCatalog(typeof(TEntity)));

        /// <summary>
        /// عنوان را به شناسه تبدیل می‌کند.
        /// فقط Ability و Software (متن آزاد کاربر) در صورت نبودن ساخته می‌شوند.
        /// دپارتمان و بقیه فهرست‌ها هرگز خودکار درج نمی‌شوند و ردیف اول انتخاب نمی‌شود.
        /// </summary>
        public async Task<short> ResolveAsync(Type clrType, string? title, bool createIfMissing)
        {
            var rows = _tables.TryGetValue(clrType, out var existing) ? existing : new List<LookupItem>();
            var text = title?.Trim();

            if (string.IsNullOrEmpty(text) || text == "—")
                throw new InvalidOperationException($"مقداری برای جدول {clrType.Name} انتخاب نشده است.");

            var match = rows.FirstOrDefault(x => string.Equals(x.Title, text, StringComparison.OrdinalIgnoreCase));
            if (match != null) return match.Id;

            if (!createIfMissing || !IsUserCatalog(clrType) || KeyProperty(clrType) == null)
                throw new InvalidOperationException(
                    $"«{text}» در جدول {clrType.Name} وجود ندارد. آن را از فهرست انتخاب کنید یا در داده پایه ثبت کنید.");

            var task = (Task<LookupItem?>)CreatorMethod
                .MakeGenericMethod(clrType)
                .Invoke(null, new object[] { _db, text })!;

            var created = await task;
            if (created == null)
                throw new InvalidOperationException($"ثبت «{text}» در جدول {clrType.Name} ممکن نشد.");

            rows.Add(created);
            _tables[clrType] = rows;
            return created.Id;
        }

        private static bool IsUserCatalog(Type clrType)
            => clrType == typeof(HR.Data.Entities.Ability) || clrType == typeof(HR.Data.Entities.Software);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _db.Dispose();
        }
    }
}