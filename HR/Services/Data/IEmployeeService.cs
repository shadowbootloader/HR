using HR.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HR.Services.Data
{
    public interface IEmployeeService
    {
        // خواندن
        Task<List<Employee>> GetAllAsync(bool includeDeleted = false);
        Task<Employee?> GetByIdAsync(Guid employeeId, bool includeDetails = true);
        Task<Employee?> GetByNationalCodeAsync(string nationalCode);
        Task<Employee?> GetByPersonnelCodeAsync(string personnelCode);
        Task<List<Employee>> SearchAsync(string term);

        // نوشتن
        Task<Employee> CreateAsync(Employee employee);
        Task<Employee> UpdateAsync(Employee employee);
        Task<bool> SoftDeleteAsync(Guid employeeId);
        Task<bool> HardDeleteAsync(Guid employeeId);
        Task<bool> RestoreAsync(Guid employeeId);

        // عملیات کمکی
        Task<bool> ExistsNationalCodeAsync(string nationalCode, Guid? excludeId = null);
        Task<bool> ExistsPersonnelCodeAsync(string personnelCode, Guid? excludeId = null);
    }
}
