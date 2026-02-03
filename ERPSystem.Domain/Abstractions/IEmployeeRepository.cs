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
        Task<Employee?> GetByIdAsync(Guid id, int companyId, CancellationToken ct = default);
        Task<Employee?> GetByIdWithDetailsAsync(Guid id, int companyId, CancellationToken ct = default);
        Task<IEnumerable<Employee>> GetAllAsync(int companyId, CancellationToken ct = default);

        Task<bool> ExistsByEmployeeCodeAsync(string code, int companyId, CancellationToken ct = default);
        Task<bool> ExistsByEmailAsync(string email, int companyId, CancellationToken ct = default);
        Task<bool> ExistsByNationalIdAsync(string nationalId, int companyId, CancellationToken ct = default);
        Task<IEnumerable<Employee>> GetByDepartmentIdAsync(Guid departmentId, int companyId, CancellationToken ct = default);
        Task<IEnumerable<Employee>> GetByStatusAsync(EmployeeStatus status, int companyId, CancellationToken ct = default);
        Task<bool> HasCircularReportingAsync(Guid employeeId, Guid managerId, int companyId, CancellationToken ct = default);
        Task AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task DeleteAsync(Guid id, int companyId, CancellationToken ct = default);

    }
}
