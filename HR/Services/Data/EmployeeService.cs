using HR.Data;
using HR.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HR.Services.Data
{
    public class EmployeeService : IEmployeeService
    {
        private readonly PersonnelDbContext _db;

        public EmployeeService(PersonnelDbContext db)
        {
            _db = db;
        }

        public EmployeeService() : this(new PersonnelDbContext())
        {
        }

        // ==================== خواندن ====================

        public async Task<List<Employee>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _db.Employees
                .Include(e => e.Gender)
                .Include(e => e.MaritalStatus)
                .Include(e => e.Supplement)!
                    .ThenInclude(s => s.Department)
                .AsQueryable();

            if (!includeDeleted)
                query = query.Where(e => !e.IsDeleted);

            return await query
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(Guid employeeId, bool includeDetails = true)
        {
            var query = _db.Employees.AsQueryable();

            if (includeDetails)
            {
                query = query
                    .Include(e => e.Gender)
                    .Include(e => e.MaritalStatus)
                    .Include(e => e.MilitaryStatus)
                    .Include(e => e.Religion)
                    .Include(e => e.Supplement)!
                        .ThenInclude(s => s.Department)
                    .Include(e => e.Supplement)!
                        .ThenInclude(s => s.InsuranceType)
                    .Include(e => e.Supplement)!
                        .ThenInclude(s => s.CooperationType)
                    .Include(e => e.Educations)
                        .ThenInclude(ed => ed.Degree)
                    .Include(e => e.Educations)
                        .ThenInclude(ed => ed.Status)
                    .Include(e => e.Courses)
                    .Include(e => e.BankAccounts)
                        .ThenInclude(b => b.Bank)
                    .Include(e => e.BankAccounts)
                        .ThenInclude(b => b.AccountType)
                    .Include(e => e.PhoneNumbers)
                        .ThenInclude(p => p.Type)
                    .Include(e => e.PhoneNumbers)
                        .ThenInclude(p => p.Status)
                    .Include(e => e.RelatedPersons)
                    .Include(e => e.Languages)
                        .ThenInclude(l => l.Language)
                    .Include(e => e.Languages)
                        .ThenInclude(l => l.ReadingLevel)
                    .Include(e => e.Softwares)
                        .ThenInclude(s => s.Software)
                    .Include(e => e.Softwares)
                        .ThenInclude(s => s.SkillLevel)
                    .Include(e => e.EmploymentHistories)
                    .Include(e => e.Citizenships)
                    .Include(e => e.ForeignTravels)
                    .Include(e => e.ForeignRelatives)
                    .Include(e => e.ForeignCooperations)
                    .Include(e => e.OrganizationCooperations)
                    .Include(e => e.OtherActivities)
                    .Include(e => e.SacrificeRecords)
                    .Include(e => e.HiringRecords)
                    .Include(e => e.PersonFiles)
                    .Include(e => e.Addresses)
                    .Include(e => e.BackgroundRecords);
            }

            return await query
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<Employee?> GetByNationalCodeAsync(string nationalCode)
        {
            return await _db.Employees
                .Include(e => e.Supplement)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.NationalCode == nationalCode && !e.IsDeleted);
        }

        public async Task<Employee?> GetByPersonnelCodeAsync(string personnelCode)
        {
            return await _db.Employees
                .Include(e => e.Supplement)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Supplement != null &&
                                          e.Supplement.PersonnelCode == personnelCode &&
                                          !e.IsDeleted);
        }

        public async Task<List<Employee>> SearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return await GetAllAsync();

            term = term.Trim();

            return await _db.Employees
                .Include(e => e.Gender)
                .Include(e => e.Supplement)!
                    .ThenInclude(s => s.Department)
                .Where(e => !e.IsDeleted &&
                       (e.FirstName.Contains(term) ||
                        e.LastName.Contains(term) ||
                        e.NationalCode.Contains(term) ||
                        (e.Supplement != null && e.Supplement.PersonnelCode.Contains(term)) ||
                        (e.FirstName + " " + e.LastName).Contains(term)))
                .OrderBy(e => e.LastName)
                .AsNoTracking()
                .ToListAsync();
        }

        // ==================== نوشتن ====================

        public async Task<Employee> CreateAsync(Employee employee)
        {
            if (employee.EmployeeId == Guid.Empty)
                employee.EmployeeId = Guid.NewGuid();

            employee.IsDeleted = false;

            // اگر Supplement وجود دارد و Id ندارد
            if (employee.Supplement != null && employee.Supplement.EmployeeSupplementId == Guid.Empty)
            {
                employee.Supplement.EmployeeSupplementId = Guid.NewGuid();
                employee.Supplement.EmployeeId = employee.EmployeeId;
            }

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();
            return employee;
        }

        public async Task<Employee> UpdateAsync(Employee employee)
        {
            var existing = await _db.Employees
                .Include(e => e.Supplement)
                .FirstOrDefaultAsync(e => e.EmployeeId == employee.EmployeeId);

            if (existing == null)
                throw new InvalidOperationException("کارمند یافت نشد.");

            // به‌روزرسانی فیلدهای اصلی
            _db.Entry(existing).CurrentValues.SetValues(employee);

            // مدیریت Supplement
            if (employee.Supplement != null)
            {
                if (existing.Supplement == null)
                {
                    employee.Supplement.EmployeeId = employee.EmployeeId;
                    if (employee.Supplement.EmployeeSupplementId == Guid.Empty)
                        employee.Supplement.EmployeeSupplementId = Guid.NewGuid();

                    _db.EmployeeSupplements.Add(employee.Supplement);
                }
                else
                {
                    _db.Entry(existing.Supplement).CurrentValues.SetValues(employee.Supplement);
                }
            }

            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> SoftDeleteAsync(Guid employeeId)
        {
            var employee = await _db.Employees.FindAsync(employeeId);
            if (employee == null) return false;

            employee.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HardDeleteAsync(Guid employeeId)
        {
            var employee = await _db.Employees
                .Include(e => e.Supplement)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null) return false;

            _db.Employees.Remove(employee);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(Guid employeeId)
        {
            var employee = await _db.Employees.FindAsync(employeeId);
            if (employee == null) return false;

            employee.IsDeleted = false;
            await _db.SaveChangesAsync();
            return true;
        }

        // ==================== بررسی تکراری بودن ====================

        public async Task<bool> ExistsNationalCodeAsync(string nationalCode, Guid? excludeId = null)
        {
            var query = _db.Employees.Where(e => e.NationalCode == nationalCode && !e.IsDeleted);
            if (excludeId.HasValue)
                query = query.Where(e => e.EmployeeId != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> ExistsPersonnelCodeAsync(string personnelCode, Guid? excludeId = null)
        {
            var query = _db.EmployeeSupplements
                .Where(s => s.PersonnelCode == personnelCode);

            if (excludeId.HasValue)
                query = query.Where(s => s.EmployeeId != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}
