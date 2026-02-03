using ERPSystem.Application.DTOs.HR.Attendance;
using ERPSystem.Application.DTOs.HR.Leave;
using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ERPSystem.Application.DTOs.HR.Attendance.Check;

namespace ERPSystem.Application.Interfaces
{
    public interface ILeaveRequestService
    {
        // Create leave request
        Task<LeaveRequestDto> CreateAsync(CreateLeaveRequestDto dto, CancellationToken ct =default);

        // Update leave request (only pending requests)
        Task<LeaveRequestDto> UpdateAsync(Guid id, UpdateLeaveRequestDto dto, CancellationToken ct = default);

        // Approve leave request
        Task ApproveAsync(Guid id, ApproveLeaveDto dto, string approvedBy, CancellationToken ct = default);

        // Reject leave request
        Task RejectAsync(Guid id, RejectLeaveDto dto, string rejectedBy, CancellationToken ct = default);

        // Cancel leave request
        Task CancelAsync(Guid id, string reason, string cancelledBy, CancellationToken ct = default);

        // Get leave request by ID
        Task<LeaveRequestDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

        // Get all leave requests for an employee
        Task<IEnumerable<LeaveRequestDto>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken ct = default);

        // Get pending leave requests (for approvers)
        Task<IEnumerable<LeaveRequestDto>> GetPendingAsync(CancellationToken ct = default);

        // Get leave balance for employee
        Task<LeaveBalanceDto> GetBalanceAsync(Guid employeeId, int year, CancellationToken ct = default);

        // Get leave history for employee (with filters)
        Task<IEnumerable<LeaveRequestDto>> GetHistoryAsync(
            Guid employeeId,
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            LeaveType? leaveType = null
            , CancellationToken ct = default);

        // Delete leave request (only pending)
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
