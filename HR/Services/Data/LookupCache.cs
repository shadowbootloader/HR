using HR.Data;
using HR.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.Services.Data
{
    /// <summary>یک ردیف از جدول Lookup (Id + عنوان فارسی که از DB آمده).</summary>
    public sealed record LookupItem(short Id, string Title)
    {
        public override string ToString() => Title;
    }

    public static class LookupKeys
    {
        public const string Gender = "Gender";
        public const string MaritalStatus = "MaritalStatus";
        public const string MilitaryStatus = "MilitaryStatus";
        public const string EducationDegree = "EducationDegree";
        public const string EducationStatus = "EducationStatus";
        public const string AccountTypeBank = "AccountTypeBank";
        public const string PhoneType = "PhoneType";
        public const string PhoneStatus = "PhoneStatus";
        public const string SkillLevel = "SkillLevel";
        public const string InsuranceType = "InsuranceType";
        public const string CooperationType = "CooperationType";
        public const string Bank = "Bank";
        public const string Language = "Language";
        public const string Software = "Software";
        public const string Department = "Department";
        public const string RecordType = "RecordType";
        public const string TransportType = "TransportType";
        public const string SacrificeType = "SacrificeType";
        public const string CenterType = "CenterType";
        public const string SectorType = "SectorType";
        public const string CitizenshipReason = "CitizenshipReason";
        public const string HiringResult = "HiringResult";
    }

    /// <summary>
    /// کش یک‌باره‌ی همه‌ی جداول Lookup از دیتابیس.
    /// منبع واحد عنوان‌ها و Idها — هیچ لیستی در کد نگه داشته نمی‌شود.
    /// </summary>
    public sealed class LookupCache : IDisposable
    {
        private static LookupCache? _current;
        private static readonly SemaphoreSlim _gate = new(1, 1);

        private readonly PersonnelDbContext _db;
        private readonly Dictionary<string, List<LookupItem>> _tables = new(StringComparer.OrdinalIgnoreCase);
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

            cache._tables[LookupKeys.Gender] = await db.Genders.AsNoTracking().OrderBy(x => x.GenderId).Select(x => new LookupItem(x.GenderId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.MaritalStatus] = await db.MaritalStatuses.AsNoTracking().OrderBy(x => x.MaritalStatusId).Select(x => new LookupItem(x.MaritalStatusId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.MilitaryStatus] = await db.MilitaryStatuses.AsNoTracking().OrderBy(x => x.MilitaryStatusId).Select(x => new LookupItem(x.MilitaryStatusId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.EducationDegree] = await db.EducationDegrees.AsNoTracking().OrderBy(x => x.DegreeId).Select(x => new LookupItem(x.DegreeId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.EducationStatus] = await db.EducationStatuses.AsNoTracking().OrderBy(x => x.StatusId).Select(x => new LookupItem(x.StatusId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.AccountTypeBank] = await db.AccountTypeBanks.AsNoTracking().OrderBy(x => x.AccountTypeId).Select(x => new LookupItem(x.AccountTypeId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.PhoneType] = await db.PhoneTypes.AsNoTracking().OrderBy(x => x.PhoneTypeId).Select(x => new LookupItem(x.PhoneTypeId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.PhoneStatus] = await db.PhoneStatuses.AsNoTracking().OrderBy(x => x.PhoneStatusId).Select(x => new LookupItem(x.PhoneStatusId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.SkillLevel] = await db.SkillLevels.AsNoTracking().OrderBy(x => x.SkillLevelId).Select(x => new LookupItem(x.SkillLevelId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.InsuranceType] = await db.InsuranceTypes.AsNoTracking().OrderBy(x => x.InsuranceTypeId).Select(x => new LookupItem(x.InsuranceTypeId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.CooperationType] = await db.CooperationTypes.AsNoTracking().OrderBy(x => x.CooperationTypeId).Select(x => new LookupItem(x.CooperationTypeId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.Bank] = await db.Banks.AsNoTracking().OrderBy(x => x.Title).Select(x => new LookupItem(x.BankId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.Language] = await db.Languages.AsNoTracking().OrderBy(x => x.Title).Select(x => new LookupItem(x.LanguageId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.Software] = await db.Softwares.AsNoTracking().OrderBy(x => x.Title).Select(x => new LookupItem(x.SoftwareId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.Department] = await db.Departments.AsNoTracking().OrderBy(x => x.Title).Select(x => new LookupItem(x.DepartmentId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.RecordType] = await db.RecordTypes.AsNoTracking().OrderBy(x => x.RecordTypeId).Select(x => new LookupItem(x.RecordTypeId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.TransportType] = await db.TransportTypes.AsNoTracking().OrderBy(x => x.TransportTypeId).Select(x => new LookupItem(x.TransportTypeId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.SacrificeType] = await db.SacrificeTypes.AsNoTracking().OrderBy(x => x.SacrificeTypeId).Select(x => new LookupItem(x.SacrificeTypeId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.CenterType] = await db.CenterTypes.AsNoTracking().OrderBy(x => x.CenterTypeId).Select(x => new LookupItem(x.CenterTypeId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.SectorType] = await db.SectorTypes.AsNoTracking().OrderBy(x => x.SectorTypeId).Select(x => new LookupItem(x.SectorTypeId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.CitizenshipReason] = await db.CitizenshipReasons.AsNoTracking().OrderBy(x => x.CitizenshipReasonId).Select(x => new LookupItem(x.CitizenshipReasonId, x.Title)).ToListAsync();
            cache._tables[LookupKeys.HiringResult] = await db.HiringResults.AsNoTracking().OrderBy(x => x.HiringResultId).Select(x => new LookupItem(x.HiringResultId, x.Title)).ToListAsync();

            return cache;
        }

        public IReadOnlyList<LookupItem> this[string tableKey]
            => _tables.TryGetValue(tableKey, out var rows) ? rows : Array.Empty<LookupItem>();

        /// <summary>ردیف را با عنوان پیدا می‌کند و اگر نبود در همان جدول می‌سازد.</summary>
        public async Task<short> ResolveOrCreateAsync(string tableKey, string? title)
        {
            var text = title?.Trim();
            if (string.IsNullOrEmpty(text)) return this[tableKey].FirstOrDefault()?.Id ?? 0;

            var existing = this[tableKey]
                .FirstOrDefault(x => string.Equals(x.Title, text, StringComparison.OrdinalIgnoreCase));

            if (existing != null) return existing.Id;

            var created = await CreateAsync(tableKey, text);
            if (created != null)
            {
                if (!_tables.TryGetValue(tableKey, out var rows))
                    _tables[tableKey] = rows = new List<LookupItem>();

                rows.Add(created);
                return created.Id;
            }

            return this[tableKey].FirstOrDefault()?.Id ?? 0;
        }

        private async Task<LookupItem?> CreateAsync(string tableKey, string title)
        {
            switch (tableKey)
            {
                case LookupKeys.Bank:
                    { var r = new Bank { Title = title }; _db.Banks.Add(r); await _db.SaveChangesAsync(); return new LookupItem(r.BankId, r.Title); }
                case LookupKeys.Language:
                    { var r = new Language { Title = title }; _db.Languages.Add(r); await _db.SaveChangesAsync(); return new LookupItem(r.LanguageId, r.Title); }
                case LookupKeys.Software:
                    { var r = new Software { Title = title }; _db.Softwares.Add(r); await _db.SaveChangesAsync(); return new LookupItem(r.SoftwareId, r.Title); }
                case LookupKeys.Department:
                    { var r = new Department { Title = title }; _db.Departments.Add(r); await _db.SaveChangesAsync(); return new LookupItem(r.DepartmentId, r.Title); }
                default:
                    return null;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _db.Dispose();
        }
    }
}