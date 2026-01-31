using ERPSystem.Application.DTOs.HR.Employee;
using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<EmployeeDetailDto> CreateAsync(CreateEmployeeDto dto, string createdBy, CancellationToken ct = default);

        Task<EmployeeDetailDto> UpdateAsync(Guid id, UpdateEmployeeDto dto, string modifiedBy, CancellationToken ct = default);

        Task UpdateStatusAsync(Guid id, UpdateEmployeeDto dto, string modifiedBy, CancellationToken ct = default);

        Task<EmployeeDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<IEnumerable<EmployeeListDto>> GetAllAsync(CancellationToken ct = default);

        Task<IEnumerable<EmployeeListDto>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default);

        Task<IEnumerable<EmployeeListDto>> GetByStatusAsync(EmployeeStatus status, CancellationToken ct = default);

        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
