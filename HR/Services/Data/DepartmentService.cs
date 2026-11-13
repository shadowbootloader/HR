using HR.Data;
using HR.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HR.Services.Data
{
    public class DepartmentService : IDepartmentService
    {
        private readonly PersonnelDbContext _db;

        public DepartmentService(PersonnelDbContext db)
        {
            _db = db;
        }

        public DepartmentService() : this(new PersonnelDbContext())
        {
        }

        public async Task<List<Department>> GetAllAsync()
        {
            return await _db.Departments
                .OrderBy(d => d.Title)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Department?> GetByIdAsync(short id)
        {
            return await _db.Departments
                .Include(d => d.Parent)
                .Include(d => d.Children)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DepartmentId == id);
        }

        public async Task<List<Department>> GetTreeAsync()
        {
            var all = await _db.Departments
                .AsNoTracking()
                .ToListAsync();

            // ساخت درخت در حافظه
            var lookup = all.ToLookup(d => d.ParentDepartment);
            foreach (var dept in all)
            {
                dept.Children = lookup[dept.DepartmentId].ToList();
            }

            return all.Where(d => d.ParentDepartment == null).ToList();
        }

        public async Task<Department> CreateAsync(Department department)
        {
            _db.Departments.Add(department);
            await _db.SaveChangesAsync();
            return department;
        }

        public async Task<Department> UpdateAsync(Department department)
        {
            var existing = await _db.Departments.FindAsync(department.DepartmentId);
            if (existing == null)
                throw new InvalidOperationException("دپارتمان یافت نشد.");

            existing.Title = department.Title;
            existing.ParentDepartment = department.ParentDepartment;

            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(short id)
        {
            var dept = await _db.Departments
                .Include(d => d.Children)
                .Include(d => d.Employees)
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
    }
}
