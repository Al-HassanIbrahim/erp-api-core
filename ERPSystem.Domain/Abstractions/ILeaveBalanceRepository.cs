using ERPSystem.Domain.Entities.HR;
using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Abstractions
{
    public interface ILeaveBalanceRepository
    {
        Task<LeaveBalance?> GetByEmployeeYearAndTypeAsync(Guid employeeId, int year, LeaveType type, int companyId, CancellationToken ct = default);
        Task<IEnumerable<LeaveBalance>> GetByEmployeeAndYearAsync(Guid employeeId, int year, int companyId, CancellationToken ct = default);
        Task AddAsync(LeaveBalance balance);
        Task UpdateAsync(LeaveBalance balance);
    }
}
