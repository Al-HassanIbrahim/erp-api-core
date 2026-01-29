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
        Task<LeaveRequest?> GetByIdAsync(Guid id, int companyId, CancellationToken ct = default);
        Task<LeaveRequest?> GetByIdWithDetailsAsync(Guid id, int companyId, CancellationToken ct = default);

        Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(Guid employeeId, int companyId, CancellationToken ct = default);
        Task<IEnumerable<LeaveRequest>> GetPendingAsync(int companyId, CancellationToken ct = default);

        Task<bool> HasOverlappingLeaveAsync(Guid employeeId, DateOnly start, DateOnly end, int companyId, Guid? excludeId = null, CancellationToken ct = default);

        Task<IEnumerable<LeaveRequest>> GetApprovedByEmployeeAndPeriodAsync(
            Guid employeeId, DateOnly startDate, DateOnly endDate, int companyId, CancellationToken ct = default);

        Task AddAsync(LeaveRequest leaveRequest);
        Task UpdateAsync(LeaveRequest leaveRequest);

        Task DeleteAsync(Guid id, int companyId, CancellationToken ct = default);
    }
}
