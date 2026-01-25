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
        Task<PayrollBatchDto> GeneratePayrollAsync(GeneratePayrollDto dto, string generatedBy);
        Task<PayrollDetailDto> GenerateEmployeePayrollAsync(Guid employeeId, int month, int year, string generatedBy);
        Task<PayrollDetailDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<PayrollDto>> GetByEmployeeIdAsync(Guid employeeId);

        // Get payrolls by period
        Task<IEnumerable<PayrollDto>> GetByPeriodAsync(int month, int year);

        Task<PayrollDetailDto> UpdateAsync(Guid id, UpdatePayrollDto dto, string modifiedBy);

        // Process payroll (Draft -> Processed)
        Task ProcessPayrollAsync(Guid id, string processedBy);

        // Mark as paid (Processed -> Paid)
        Task MarkAsPaidAsync(Guid id, MarkPaidDto dto, string paidBy);

        // Revert to draft (Processed -> Draft)
        Task RevertToDraftAsync(Guid id, string modifiedBy);

        
        Task DeleteAsync(Guid id);

        
        Task<PayrollSummaryDto> GetPeriodSummaryAsync(int month, int year);

        // Recalculate payroll
        Task<PayrollDetailDto> RecalculateAsync(Guid id, string modifiedBy);
    }
}
