using ERPSystem.Domain.Entities.HR;
using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Abstractions
{
    internal interface ILeaveBalanceRepository
    {
        Task<LeaveBalance?> GetByEmployeeYearAndTypeAsync(Guid employeeId, int year, LeaveType type);
        Task<IEnumerable<LeaveBalance>> GetByEmployeeAndYearAsync(Guid employeeId, int year);
        Task AddAsync(LeaveBalance balance);
        Task UpdateAsync(LeaveBalance balance);
    }
}
