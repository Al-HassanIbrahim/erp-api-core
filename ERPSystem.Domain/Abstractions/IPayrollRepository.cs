using ERPSystem.Domain.Entities.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Abstractions
{
    public interface IPayrollRepository
    {
        Task<Payroll?> GetByIdAsync(Guid id);
        Task<Payroll?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<Payroll>> GetByEmployeeIdAsync(Guid employeeId);
        Task<Payroll?> GetByEmployeeAndPeriodAsync(Guid employeeId, int month, int year);
        Task<IEnumerable<Payroll>> GetByMonthAndYearAsync(int month, int year);
        Task<bool> ExistsForEmployeeAndPeriodAsync(Guid employeeId, int month, int year);
        Task AddAsync(Payroll payroll);
        Task UpdateAsync(Payroll payroll);
        Task DeleteAsync(Guid id);
    }
}
