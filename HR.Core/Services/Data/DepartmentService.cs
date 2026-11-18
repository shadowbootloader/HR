using HR.Data;
using HR.Data.Entities;
using HR.Services.Auth;
using HR.Services.Files;
using HR.Services.Org;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HR.Services.Data
{
    public sealed class DepartmentMemberRow
    {
        public string FullName { get; init; } = string.Empty;
        public string PersonnelCode { get; init; } = string.Empty;
        public string Position { get; init; } = string.Empty;
        public DateTime? StartDate { get; init; }
    }

    public class DepartmentService : IDepartmentService, IDisposable
    {
        private readonly PersonnelDbContext _db;

        public DepartmentService(PersonnelDbContext db) => _db = db;

        public DepartmentService() : this(new PersonnelDbContext())
        {
        }

        public async Task<Department> EnsureHeadquartersAsync()
        {
            var existing = await _db.Departments
                .FirstOrDefaultAsync(item => item.Title == OrgChartLayout.HeadquartersTitle && item.ParentDepartmentId == null);
            if (existing != null) return existing;

            existing = await _db.Departments.FirstOrDefaultAsync(item => item.Title == OrgChartLayout.HeadquartersTitle);
            if (existing != null) return existing;

            var created = new Department { Title = OrgChartLayout.HeadquartersTitle, IsActive = true };
            _db.Departments.Add(created);
            await _db.SaveChangesAsync();
            return created;
        }

        public async Task<List<Department>> GetAllAsync(bool onlyActive = false)
        {
            var query = _db.Departments
                .Include(d => d.Parent)
                .Include(d => d.Manager)
                .AsQueryable();

            if (onlyActive)
                query = query.Where(d => d.IsActive);

            return await query
                .OrderBy(d => d.Title)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Department?> GetByIdAsync(short id)
        {
            return await _db.Departments
                .Include(d => d.Parent)
                .Include(d => d.Children)
                .Include(d => d.Offices)
                .Include(d => d.Manager)
                .Include(d => d.Deputy)
                .Include(d => d.CashCustodian)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DepartmentId == id);
        }

        public async Task<List<Department>> GetTreeAsync()
        {
            var all = await _db.Departments.AsNoTracking().ToListAsync();
            var lookup = all.ToLookup(d => d.ParentDepartmentId);

            foreach (var dept in all)
                dept.Children = lookup[dept.DepartmentId].ToList();

            return all.Where(d => d.ParentDepartmentId == null).OrderBy(d => d.Title).ToList();
        }

        public async Task<bool> ExistsTitleAsync(string title, short? parentId = null, short? excludeId = null)
        {
            title = title.Trim();
            var query = _db.Departments.Where(d => d.Title == title && d.ParentDepartmentId == parentId);
            if (excludeId.HasValue)
                query = query.Where(d => d.DepartmentId != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<Department> CreateAsync(Department department)
        {
            AccessGuard.RequirePeopleManagement();
            if (department == null) throw new ArgumentNullException(nameof(department));
            if (string.IsNullOrWhiteSpace(department.Title))
                throw new InvalidOperationException("نام دپارتمان الزامی است.");

            department.Title = department.Title.Trim();
            ValidateFields(department);

            if (await ExistsTitleAsync(department.Title, department.ParentDepartmentId))
                throw new InvalidOperationException("دپارتمان هم‌نام در همین سطح والد وجود دارد.");

            if (department.ParentDepartmentId.HasValue)
            {
                var parentExists = await _db.Departments.AnyAsync(d => d.DepartmentId == department.ParentDepartmentId.Value);
                if (!parentExists)
                    throw new InvalidOperationException("دپارتمان والد یافت نشد.");
            }

            foreach (var office in department.Offices)
            {
                if (office.OfficeId == Guid.Empty)
                    office.OfficeId = Guid.NewGuid();
            }

            try
            {
                _db.Departments.Add(department);
                await _db.SaveChangesAsync();
                await new DocumentLinkService().SyncDepartmentLogoAsync(_db, department);
                await _db.SaveChangesAsync();
                return department;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException(
                    "خطا در ذخیره دپارتمان: " + (ex.GetBaseException()?.Message ?? ex.Message), ex);
            }
        }

        public async Task<Department> UpdateAsync(Department department)
        {
            AccessGuard.RequirePeopleManagement();
            if (department == null) throw new ArgumentNullException(nameof(department));
            if (department.DepartmentId <= 0)
                throw new InvalidOperationException("شناسه دپارتمان نامعتبر است.");
            if (string.IsNullOrWhiteSpace(department.Title))
                throw new InvalidOperationException("نام دپارتمان الزامی است.");

            department.Title = department.Title.Trim();
            ValidateFields(department);

            if (department.ParentDepartmentId == department.DepartmentId)
                throw new InvalidOperationException("دپارتمان نمی‌تواند والد خودش باشد.");

            if (await ExistsTitleAsync(department.Title, department.ParentDepartmentId, department.DepartmentId))
                throw new InvalidOperationException("دپارتمان هم‌نام در همین سطح والد وجود دارد.");

            var existing = await _db.Departments
                .Include(d => d.Offices)
                .FirstOrDefaultAsync(d => d.DepartmentId == department.DepartmentId);

            if (existing == null)
                throw new InvalidOperationException("دپارتمان یافت نشد.");

            // جلوگیری از حلقه در درخت
            if (department.ParentDepartmentId.HasValue &&
                await IsDescendantAsync(department.DepartmentId, department.ParentDepartmentId.Value))
            {
                throw new InvalidOperationException("نمی‌توان زیرمجموعه را به‌عنوان والد انتخاب کرد.");
            }

            existing.Title = department.Title;
            existing.ParentDepartmentId = department.ParentDepartmentId;
            existing.IsActive = department.IsActive;
            existing.SecurityLevel = department.SecurityLevel;
            existing.EstablishedDate = department.EstablishedDate;
            existing.ApprovedCapacity = department.ApprovedCapacity;
            existing.LogoPath = department.LogoPath;
            existing.ManagerEmployeeId = department.ManagerEmployeeId;
            existing.DeputyEmployeeId = department.DeputyEmployeeId;
            existing.CashCustodianEmployeeId = department.CashCustodianEmployeeId;
            existing.CostCenterCode = department.CostCenterCode;
            existing.AnnualBudget = department.AnnualBudget;
            existing.PettyCashLimit = department.PettyCashLimit;
            existing.Description = department.Description;

            // ادغام دفاتر: ردیف‌های موجود به‌روز می‌شوند، تازه‌ها اضافه و حذف‌شده‌ها پاک می‌شوند.
            // (پاک‌کردن همه و افزودن دوباره با همان OfficeId، دو نمونه با یک کلید در ردیابی EF می‌سازد.)
            var incomingOffices = department.Offices.ToList();
            var incomingOfficeIds = incomingOffices
                .Where(o => o.OfficeId != Guid.Empty)
                .Select(o => o.OfficeId)
                .ToHashSet();

            foreach (var removed in existing.Offices.Where(o => !incomingOfficeIds.Contains(o.OfficeId)).ToList())
            {
                existing.Offices.Remove(removed);
                _db.DepartmentOffices.Remove(removed);
            }

            foreach (var office in incomingOffices)
            {
                var officeName = string.IsNullOrWhiteSpace(office.Name) ? "—" : office.Name.Trim();
                var current = office.OfficeId == Guid.Empty
                    ? null
                    : existing.Offices.FirstOrDefault(o => o.OfficeId == office.OfficeId);

                if (current != null)
                {
                    current.Name = officeName;
                    current.City = office.City;
                    current.Address = office.Address;
                }
                else
                {
                    existing.Offices.Add(new DepartmentOffice
                    {
                        OfficeId = office.OfficeId == Guid.Empty ? Guid.NewGuid() : office.OfficeId,
                        DepartmentId = existing.DepartmentId,
                        Name = officeName,
                        City = office.City,
                        Address = office.Address
                    });
                }
            }

            try
            {
                await new DocumentLinkService().SyncDepartmentLogoAsync(_db, existing);
                await _db.SaveChangesAsync();
                return existing;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException(
                    "خطا در به‌روزرسانی دپارتمان: " + (ex.GetBaseException()?.Message ?? ex.Message), ex);
            }
        }

        public async Task<List<DepartmentMemberRow>> ListMembersAsync(short departmentId)
        {
            return await _db.EmployeeSupplements.AsNoTracking()
                .Where(item => item.DepartmentId == departmentId && !item.Employee.IsDeleted)
                .OrderBy(item => item.Employee.LastName)
                .ThenBy(item => item.Employee.FirstName)
                .Select(item => new DepartmentMemberRow
                {
                    FullName = (item.Employee.FirstName + " " + item.Employee.LastName).Trim(),
                    PersonnelCode = item.PersonnelCode,
                    Position = item.PositionTitle,
                    StartDate = item.StartDate
                })
                .ToListAsync();
        }

        public async Task<Dictionary<short, int>> CountMembersAsync()
        {
            var rows = await _db.EmployeeSupplements
                .Where(s => s.DepartmentId != null && !s.Employee.IsDeleted)
                .GroupBy(s => s.DepartmentId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();

            return rows.Where(x => x.Key != null).ToDictionary(x => x.Key!.Value, x => x.Count);
        }

        public async Task<bool> DeleteAsync(short id)
        {
            AccessGuard.RequirePeopleManagement();
            var dept = await _db.Departments
                .Include(d => d.Children)
                .Include(d => d.Employees).ThenInclude(employee => employee.Employee)
                .Include(d => d.Offices)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (dept == null) return false;

            if (dept.Title == OrgChartLayout.HeadquartersTitle)
                throw new InvalidOperationException("ستاد مرکزی ریشه چارت است و قابل حذف نیست.");

            if (dept.Children.Any())
                throw new InvalidOperationException("این دپارتمان زیرمجموعه دارد و قابل حذف نیست. ابتدا دپارتمان‌های فرزند را جابه‌جا یا حذف کنید.");

            if (dept.Employees.Any(row => row.Employee != null && !row.Employee.IsDeleted))
                throw new InvalidOperationException("این دپارتمان کارمند فعال دارد و قابل حذف نیست. ابتدا کارکنان را به دپارتمان دیگری منتقل کنید.");

            if (dept.Employees.Any())
                throw new InvalidOperationException("پرونده کارمند حذف‌شده هنوز به این دپارتمان وصل است و قابل حذف نیست.");

            await new DocumentLinkService().RemoveDepartmentAsync(_db, id);
            _db.Departments.Remove(dept);
            await _db.SaveChangesAsync();
            return true;
        }

        /// <summary>حدود طول و مقادیر منفی را قبل از رسیدن به SQL بررسی می‌کند (پیام فارسی به‌جای خطای مبهم).</summary>
        private static void ValidateFields(Department department)
        {
            if (department.Title.Length > 100)
                throw new InvalidOperationException("نام دپارتمان نباید بیش از ۱۰۰ نویسه باشد.");
            if (department.SecurityLevel?.Length > 50)
                throw new InvalidOperationException("سطح حفاظتی نباید بیش از ۵۰ نویسه باشد.");
            if (department.CostCenterCode?.Length > 50)
                throw new InvalidOperationException("کد مرکز هزینه نباید بیش از ۵۰ نویسه باشد.");
            if (department.Description?.Length > 500)
                throw new InvalidOperationException("توضیحات نباید بیش از ۵۰۰ نویسه باشد.");
            if (department.ApprovedCapacity < 0)
                throw new InvalidOperationException("ظرفیت مصوب نمی‌تواند منفی باشد.");
            if (department.AnnualBudget < 0)
                throw new InvalidOperationException("بودجه سالانه نمی‌تواند منفی باشد.");
            if (department.PettyCashLimit < 0)
                throw new InvalidOperationException("سقف تنخواه نمی‌تواند منفی باشد.");

            foreach (var office in department.Offices)
            {
                if (office.Name?.Trim().Length > 100)
                    throw new InvalidOperationException("نام دفتر نباید بیش از ۱۰۰ نویسه باشد.");
                if (office.City?.Length > 100)
                    throw new InvalidOperationException("نام شهر دفتر نباید بیش از ۱۰۰ نویسه باشد.");
                if (office.Address?.Length > 500)
                    throw new InvalidOperationException("نشانی دفتر نباید بیش از ۵۰۰ نویسه باشد.");
            }
        }

        /// <summary>آیا candidateId زیرمجموعه (مستقیم/غیرمستقیم) rootId است؟</summary>
        private async Task<bool> IsDescendantAsync(short rootId, short candidateId)
        {
            var all = await _db.Departments
                .Select(d => new { d.DepartmentId, d.ParentDepartmentId })
                .AsNoTracking()
                .ToListAsync();

            var children = all.ToLookup(x => x.ParentDepartmentId);
            var stack = new Stack<short>();
            stack.Push(rootId);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                foreach (var child in children[current])
                {
                    if (child.DepartmentId == candidateId) return true;
                    stack.Push(child.DepartmentId);
                }
            }

            return false;
        }

        public void Dispose() => _db.Dispose();
    }
}
