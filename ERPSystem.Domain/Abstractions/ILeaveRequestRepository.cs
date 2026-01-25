using ERPSystem.Domain.Entities.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Abstractions
{
    public interface ILeaveRequestRepository
    {
        Task<LeaveRequest?> GetByIdAsync(Guid id);
        Task<LeaveRequest?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(Guid employeeId);
        Task<IEnumerable<LeaveRequest>> GetPendingAsync();
        Task<bool> HasOverlappingLeaveAsync(Guid employeeId, DateOnly start, DateOnly end, Guid? excludeId = null);
        Task AddAsync(LeaveRequest leaveRequest);
        Task UpdateAsync(LeaveRequest leaveRequest);
        Task DeleteAsync(Guid id);
    }
}
