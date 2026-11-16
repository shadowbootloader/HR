using HR.Data;
using HR.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HR.Services.Data
{
    /// <summary>
    /// سرویس خواندن جداول Lookup از پایگاه داده.
    /// برای پر کردن فرم‌ها ترجیحاً از LookupCache / LookupBinder استفاده شود.
    /// این سرویس برای خواندن مستقیم و گزارش‌گیری مناسب است.
    /// </summary>
    public class LookupService
    {
        private readonly PersonnelDbContext _db;

        public LookupService(PersonnelDbContext db)
        {
            _db = db;
        }

        public LookupService() : this(new PersonnelDbContext())
        {
        }

        // ---------- اصلی ----------

        public async Task<List<Gender>> GetGendersAsync() =>
            await _db.Genders.AsNoTracking().OrderBy(x => x.GenderId).ToListAsync();

        public async Task<List<MaritalStatus>> GetMaritalStatusesAsync() =>
            await _db.MaritalStatuses.AsNoTracking().OrderBy(x => x.MaritalStatusId).ToListAsync();

        public async Task<List<MilitaryStatus>> GetMilitaryStatusesAsync() =>
            await _db.MilitaryStatuses.AsNoTracking().OrderBy(x => x.MilitaryStatusId).ToListAsync();

        public async Task<List<Religion>> GetReligionsAsync() =>
            await _db.Religions.AsNoTracking().OrderBy(x => x.ReligionId).ToListAsync();

        public async Task<List<Denomination>> GetDenominationsAsync() =>
            await _db.Denominations.AsNoTracking().OrderBy(x => x.DenominationId).ToListAsync();

        public async Task<List<EducationDegree>> GetEducationDegreesAsync() =>
            await _db.EducationDegrees.AsNoTracking().OrderBy(x => x.DegreeId).ToListAsync();

        public async Task<List<EducationStatus>> GetEducationStatusesAsync() =>
            await _db.EducationStatuses.AsNoTracking().OrderBy(x => x.StatusId).ToListAsync();

        public async Task<List<AccountTypeBank>> GetAccountTypesAsync() =>
            await _db.AccountTypeBanks.AsNoTracking().OrderBy(x => x.AccountTypeId).ToListAsync();

        public async Task<List<Bank>> GetBanksAsync() =>
            await _db.Banks.AsNoTracking().OrderBy(x => x.Title).ToListAsync();

        public async Task<List<InsuranceType>> GetInsuranceTypesAsync() =>
            await _db.InsuranceTypes.AsNoTracking().OrderBy(x => x.InsuranceTypeId).ToListAsync();

        public async Task<List<CooperationType>> GetCooperationTypesAsync() =>
            await _db.CooperationTypes.AsNoTracking().OrderBy(x => x.CooperationTypeId).ToListAsync();

        public async Task<List<SkillLevel>> GetSkillLevelsAsync() =>
            await _db.SkillLevels.AsNoTracking().OrderBy(x => x.SkillLevelId).ToListAsync();

        public async Task<List<Language>> GetLanguagesAsync() =>
            await _db.Languages.AsNoTracking().OrderBy(x => x.Title).ToListAsync();

        public async Task<List<Software>> GetSoftwaresAsync() =>
            await _db.Softwares.AsNoTracking().OrderBy(x => x.Title).ToListAsync();

        public async Task<List<PhoneType>> GetPhoneTypesAsync() =>
            await _db.PhoneTypes.AsNoTracking().OrderBy(x => x.PhoneTypeId).ToListAsync();

        public async Task<List<PhoneStatus>> GetPhoneStatusesAsync() =>
            await _db.PhoneStatuses.AsNoTracking().OrderBy(x => x.PhoneStatusId).ToListAsync();

        public async Task<List<FileType>> GetFileTypesAsync() =>
            await _db.FileTypes.AsNoTracking().OrderBy(x => x.FileTypeId).ToListAsync();

        public async Task<List<EmploymentEndReason>> GetEmploymentEndReasonsAsync() =>
            await _db.EmploymentEndReasons.AsNoTracking().OrderBy(x => x.EndReasonId).ToListAsync();

        public async Task<List<Department>> GetDepartmentsAsync() =>
            await _db.Departments.AsNoTracking().OrderBy(x => x.Title).ToListAsync();

        public async Task<List<ExemptionType>> GetExemptionTypesAsync() =>
            await _db.ExemptionTypes.AsNoTracking().OrderBy(x => x.ExemptionTypeId).ToListAsync();

        public async Task<List<ExemptionReason>> GetExemptionReasonsAsync() =>
            await _db.ExemptionReasons.AsNoTracking().OrderBy(x => x.ExemptionReasonId).ToListAsync();

        public async Task<List<CitizenshipReason>> GetCitizenshipReasonsAsync() =>
            await _db.CitizenshipReasons.AsNoTracking().OrderBy(x => x.CitizenshipReasonId).ToListAsync();

        public async Task<List<TransportType>> GetTransportTypesAsync() =>
            await _db.TransportTypes.AsNoTracking().OrderBy(x => x.TransportTypeId).ToListAsync();

        public async Task<List<TravelReason>> GetTravelReasonsAsync() =>
            await _db.TravelReasons.AsNoTracking().OrderBy(x => x.TravelReasonId).ToListAsync();

        public async Task<List<AbroadReason>> GetAbroadReasonsAsync() =>
            await _db.AbroadReasons.AsNoTracking().OrderBy(x => x.AbroadReasonId).ToListAsync();

        public async Task<List<CenterType>> GetCenterTypesAsync() =>
            await _db.CenterTypes.AsNoTracking().OrderBy(x => x.CenterTypeId).ToListAsync();

        public async Task<List<SectorType>> GetSectorTypesAsync() =>
            await _db.SectorTypes.AsNoTracking().OrderBy(x => x.SectorTypeId).ToListAsync();

        public async Task<List<HiringResult>> GetHiringResultsAsync() =>
            await _db.HiringResults.AsNoTracking().OrderBy(x => x.HiringResultId).ToListAsync();

        public async Task<List<RecordType>> GetRecordTypesAsync() =>
            await _db.RecordTypes.AsNoTracking().OrderBy(x => x.RecordTypeId).ToListAsync();

        public async Task<List<SacrificeType>> GetSacrificeTypesAsync() =>
            await _db.SacrificeTypes.AsNoTracking().OrderBy(x => x.SacrificeTypeId).ToListAsync();

        public async Task<List<SacrificeRelationship>> GetSacrificeRelationshipsAsync() =>
            await _db.SacrificeRelationships.AsNoTracking().OrderBy(x => x.SacrificeRelationshipId).ToListAsync();

        public async Task<List<AmountUnit>> GetAmountUnitsAsync() =>
            await _db.AmountUnits.AsNoTracking().OrderBy(x => x.AmountUnitId).ToListAsync();

        // ---------- Geo ----------

        public async Task<List<Geo>> GetGeosByLevelAsync(int level) =>
            await _db.Geos.Where(g => g.Level == level).AsNoTracking().OrderBy(g => g.Name).ToListAsync();

        public async Task<List<Geo>> SearchGeoAsync(string term, int? maxLevel = null)
        {
            var query = _db.Geos.AsQueryable();
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(g => g.Name.Contains(term) || (g.NativeName != null && g.NativeName.Contains(term)));
            if (maxLevel.HasValue)
                query = query.Where(g => g.Level <= maxLevel.Value);

            return await query.AsNoTracking().OrderBy(g => g.Name).Take(50).ToListAsync();
        }

        /// <summary>
        /// خواندن سریع عنوان‌ها از LookupCache (بدون query جداگانه).
        /// </summary>
        public static async Task<IReadOnlyList<LookupItem>> GetCachedAsync(string tableKey)
        {
            var cache = await LookupCache.GetAsync();
            return cache[tableKey];
        }
    }
}
