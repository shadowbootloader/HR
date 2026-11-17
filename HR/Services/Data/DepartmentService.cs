using HR.Data;
using HR.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HR.Services.Data
{
    public class DepartmentService : IDepartmentService, IDisposable
    {
        private readonly PersonnelDbContext _db;

        public DepartmentService(PersonnelDbContext db) => _db = db;

        public DepartmentService() : this(new PersonnelDbContext())
        {
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
            if (department == null) throw new ArgumentNullException(nameof(department));
            if (string.IsNullOrWhiteSpace(department.Title))
                throw new InvalidOperationException("نام دپارتمان الزامی است.");

            department.Title = department.Title.Trim();

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
            if (department == null) throw new ArgumentNullException(nameof(department));
            if (department.DepartmentId <= 0)
                throw new InvalidOperationException("شناسه دپارتمان نامعتبر است.");
            if (string.IsNullOrWhiteSpace(department.Title))
                throw new InvalidOperationException("نام دپارتمان الزامی است.");

            department.Title = department.Title.Trim();

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

            // جایگزینی دفاتر
            existing.Offices.Clear();
            foreach (var office in department.Offices)
            {
                existing.Offices.Add(new DepartmentOffice
                {
                    OfficeId = office.OfficeId == Guid.Empty ? Guid.NewGuid() : office.OfficeId,
                    DepartmentId = existing.DepartmentId,
                    Name = office.Name?.Trim() ?? "—",
                    City = office.City,
                    Address = office.Address
                });
            }

            try
            {
                await _db.SaveChangesAsync();
                return existing;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException(
                    "خطا در به‌روزرسانی دپارتمان: " + (ex.GetBaseException()?.Message ?? ex.Message), ex);
            }
        }

        public async Task<Dictionary<short, int>> CountMembersAsync()
        {
            var rows = await _db.EmployeeSupplements
                .Where(s => !s.Employee.IsDeleted)
                .GroupBy(s => s.DepartmentId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();

            return rows.ToDictionary(x => x.Key, x => x.Count);
        }

        public async Task<bool> DeleteAsync(short id)
        {
            var dept = await _db.Departments
                .Include(d => d.Children)
                .Include(d => d.Employees)
                .Include(d => d.Offices)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (dept == null) return false;

            if (dept.Children.Any())
                throw new InvalidOperationException("این دپارتمان دارای زیرمجموعه است و قابل حذف نیست.");

            if (dept.Employees.Any())
                throw new InvalidOperationException("این دپارتمان دارای کارمند است و قابل حذف نیست.");

            _db.Departments.Remove(dept);
            await _db.SaveChangesAsync();
            return true;
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
