using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Abstractions
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetByIdAsync(Guid id);
        Task<Employee?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByEmployeeCodeAsync(string employeeCode);
        Task<bool> ExistsByNationalIdAsync(string nationalId);
        Task<IEnumerable<Employee>> GetByDepartmentIdAsync(Guid departmentId);
        Task<IEnumerable<Employee>> GetByStatusAsync(EmployeeStatus status);
        Task<bool> HasCircularReportingAsync(Guid employeeId, Guid managerId);
        Task AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task DeleteAsync(Guid id);
    }
}
