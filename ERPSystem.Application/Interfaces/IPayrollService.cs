using ERPSystem.Application.DTOs.HR.Payroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.Interfaces
{
    public interface IPayrollService
    {
        Task<PayrollBatchDto> GeneratePayrollAsync(GeneratePayrollDto dto, string generatedBy, CancellationToken ct = default);
        Task<PayrollDetailDto> GenerateEmployeePayrollAsync(Guid employeeId, int month, int year, string generatedBy, CancellationToken ct = default);
        Task<PayrollDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<PayrollDto>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken ct = default);

        // Get payrolls by period
        Task<IEnumerable<PayrollDto>> GetByPeriodAsync(int month, int year, CancellationToken ct = default);

        Task<PayrollDetailDto> UpdateAsync(Guid id, UpdatePayrollDto dto, string modifiedBy, CancellationToken ct = default);

        // Process payroll (Draft -> Processed)
        Task ProcessPayrollAsync(Guid id, string processedBy, CancellationToken ct = default);

        // Mark as paid (Processed -> Paid)
        Task MarkAsPaidAsync(Guid id, MarkPaidDto dto, string paidBy, CancellationToken ct = default);

        // Revert to draft (Processed -> Draft)
        Task RevertToDraftAsync(Guid id, string modifiedBy, CancellationToken ct = default);

        
        Task DeleteAsync(Guid id, CancellationToken ct = default);

        
        Task<PayrollSummaryDto> GetPeriodSummaryAsync(int month, int year, CancellationToken ct = default);

        // Recalculate payroll
        Task<PayrollDetailDto> RecalculateAsync(Guid id, string modifiedBy, CancellationToken ct = default);
    }
}
