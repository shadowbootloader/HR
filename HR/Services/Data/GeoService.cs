using HR.Data;
using HR.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HR.Services.Data
{
    /// <summary>
    /// دسترسی خواندنی به جدول Geo (کشور / استان / شهر).
    /// ساخت ردیف جدید انجام نمی‌شود؛ فقط از داده‌ی موجود استفاده می‌کنیم.
    /// </summary>
    public sealed class GeoService : IDisposable
    {
        public const int CountryLevel = 1;
        public const int ProvinceLevel = 2;
        public const int CityLevel = 3;

        private const string DefaultCountry = "ایران";

        private readonly PersonnelDbContext _db;

        public GeoService(PersonnelDbContext db) => _db = db;

        public GeoService() : this(new PersonnelDbContext())
        {
        }

        public async Task<List<Geo>> GetCountriesAsync() =>
            await _db.Geos.AsNoTracking()
                .Where(g => g.Level == CountryLevel)
                .OrderBy(g => g.Name)
                .ToListAsync();

        public async Task<List<Geo>> GetProvincesAsync()
        {
            var provinces = await _db.Geos.AsNoTracking()
                .Where(g => g.Level == ProvinceLevel)
                .OrderBy(g => g.Name)
                .ToListAsync();

            if (provinces.Count > 0) return provinces;

            // اگر Level در این دیتابیس شماره‌گذاری دیگری داشت، فرزندان کشور پیش‌فرض را می‌دهیم
            var country = await _db.Geos.AsNoTracking()
                .FirstOrDefaultAsync(g => g.Name == DefaultCountry && g.Level == CountryLevel);

            if (country == null) return provinces;

            var children = await GetChildrenAsync(country);
            return children.OrderBy(c => c.Name).ToList();
        }

        public async Task<List<Geo>> GetCitiesAsync(int provinceGeoId)
        {
            var province = await _db.Geos.AsNoTracking()
                .FirstOrDefaultAsync(g => g.GeoId == provinceGeoId);

            if (province == null) return new List<Geo>();

            var cities = await GetChildrenAsync(province);
            return cities.OrderBy(c => c.Name).ToList();
        }

        private async Task<List<Geo>> GetChildrenAsync(Geo parent)
        {
            try
            {
                return await _db.Geos.AsNoTracking()
                    .Where(g => g.Node.GetAncestor(1) == parent.Node)
                    .ToListAsync();
            }
            catch
            {
                // اگر GetAncestor ترجمه نشد، در حافظه بر اساس مسیر فیلتر می‌کنیم
                var prefix = parent.Node.ToString();
                var depth = prefix.Count(c => c == '/');

                var rows = await _db.Geos.AsNoTracking()
                    .Where(g => g.Level == parent.Level + 1)
                    .ToListAsync();

                return rows.Where(g =>
                {
                    var path = g.Node.ToString();
                    return path.StartsWith(prefix, StringComparison.Ordinal) &&
                           path.Count(c => c == '/') == depth + 1;
                }).ToList();
            }
        }

        public async Task<Geo?> FindCountryAsync(string? name)
        {
            var text = name?.Trim();
            if (string.IsNullOrEmpty(text)) return null;

            return await _db.Geos.AsNoTracking()
                .FirstOrDefaultAsync(g => g.Level == CountryLevel && g.Name == text);
        }

        /// <summary>کشور را به گره‌ی Geo تبدیل می‌کند.</summary>
        public async Task<HierarchyId?> FindCountryNodeAsync(string? name)
        {
            var country = await FindCountryAsync(name);
            return country?.Node;
        }

        /// <summary>استان/شهر را به گره‌ی Geo تبدیل می‌کند؛ شهر اولویت دارد و در نبودش استان برمی‌گردد.</summary>
        public async Task<HierarchyId?> ResolveLocationNodeAsync(string? provinceName, string? cityName)
        {
            var provinceText = provinceName?.Trim();
            if (string.IsNullOrEmpty(provinceText)) return null;

            var province = (await GetProvincesAsync())
                .FirstOrDefault(p => string.Equals(p.Name, provinceText, StringComparison.OrdinalIgnoreCase));

            if (province == null) return null;

            var cityText = cityName?.Trim();
            if (!string.IsNullOrEmpty(cityText))
            {
                var city = (await GetCitiesAsync(province.GeoId))
                    .FirstOrDefault(c => string.Equals(c.Name, cityText, StringComparison.OrdinalIgnoreCase));

                if (city != null) return city.Node;
            }

            return province.Node;
        }

        public async Task<HierarchyId?> FindLocationNodeByNameAsync(string? name)
        {
            var text = name?.Trim();
            if (string.IsNullOrEmpty(text)) return null;

            var row = await _db.Geos.AsNoTracking()
                .Where(g => (g.Level == ProvinceLevel || g.Level == CityLevel) && g.Name == text)
                .OrderByDescending(g => g.Level)
                .FirstOrDefaultAsync();
            return row?.Node;
        }

        public async Task<string?> FindLocationNameByNodeAsync(HierarchyId? node)
        {
            if (node == null) return null;
            var row = await _db.Geos.AsNoTracking().FirstOrDefaultAsync(g => g.Node == node);
            return row?.Name;
        }

        public async Task<string?> FindCountryNameByIdAsync(int countryId)
        {
            var row = await _db.Geos.AsNoTracking()
                .FirstOrDefaultAsync(g => g.GeoId == countryId);
            return row?.Name;
        }

        public void Dispose() => _db.Dispose();
    }
}