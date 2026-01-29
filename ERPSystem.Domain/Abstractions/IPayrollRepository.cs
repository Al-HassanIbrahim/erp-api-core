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
        Task<Payroll?> GetByIdAsync(Guid id, int companyId, CancellationToken ct = default);
        Task<Payroll?> GetByIdWithDetailsAsync(Guid id, int companyId, CancellationToken ct = default);

        Task<IEnumerable<Payroll>> GetByEmployeeIdAsync(Guid employeeId, int companyId, CancellationToken ct = default);
        Task<Payroll?> GetByEmployeeAndPeriodAsync(Guid employeeId, int month, int year, int companyId, CancellationToken ct = default);

        Task<IEnumerable<Payroll>> GetByMonthAndYearAsync(int month, int year, int companyId, CancellationToken ct = default);
        Task<bool> ExistsForEmployeeAndPeriodAsync(Guid employeeId, int month, int year, int companyId, CancellationToken ct = default);

        Task AddAsync(Payroll payroll);
        Task UpdateAsync(Payroll payroll);

        Task DeleteAsync(Guid id, int companyId, CancellationToken ct = default);
    }
}
