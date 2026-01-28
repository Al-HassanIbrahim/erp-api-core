using ERPSystem.Domain.Entities.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Abstractions
{
    public interface IDepartmentRepository
    {
        Task<Department?> GetByIdAsync(Guid id);
        Task<Department?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<Department>> GetAllAsync();
        Task<bool> ExistsByCodeAsync(string code);
        Task<bool> ExistsByNameAsync(string name);
        Task<int> GetEmployeeCountAsync(Guid departmentId);
        Task AddAsync(Department department);
        Task UpdateAsync(Department department);
        Task DeleteAsync(Guid id);
    }
}
