using HR.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HR.Services.Data
{
    public interface IDepartmentService
    {
        Task<List<Department>> GetAllAsync();
        Task<Department?> GetByIdAsync(short id);
        Task<List<Department>> GetTreeAsync();
        Task<Department> CreateAsync(Department department);
        Task<Department> UpdateAsync(Department department);
        Task<bool> DeleteAsync(short id);
    }
}
