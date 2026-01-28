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
        Task<EmployeeDetailDto> CreateAsync(CreateEmployeeDto dto, string createdBy);

        Task<EmployeeDetailDto> UpdateAsync(Guid id, UpdateEmployeeDto dto, string modifiedBy);

        Task UpdateStatusAsync(Guid id, UpdateEmployeeDto dto, string modifiedBy);

        Task<EmployeeDetailDto?> GetByIdAsync(Guid id);

        Task<IEnumerable<EmployeeListDto>> GetAllAsync();

        Task<IEnumerable<EmployeeListDto>> GetByDepartmentAsync(Guid departmentId);

        Task<IEnumerable<EmployeeListDto>> GetByStatusAsync(EmployeeStatus status);

        Task DeleteAsync(Guid id);
    }
}
