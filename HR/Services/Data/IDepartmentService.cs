using HR.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HR.Services.Data
{
    public interface IDepartmentService
    {
        Task<List<Department>> GetAllAsync(bool onlyActive = false);
        Task<Department?> GetByIdAsync(short id);
        Task<List<Department>> GetTreeAsync();
        Task<bool> ExistsTitleAsync(string title, short? parentId = null, short? excludeId = null);

        Task<Department> CreateAsync(Department department);
        Task<Department> UpdateAsync(Department department);
        Task<bool> DeleteAsync(short id);
    }
}
