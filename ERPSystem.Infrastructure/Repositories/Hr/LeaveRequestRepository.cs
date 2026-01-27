using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.HR;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Hr
{
    public class LeaveRequestRepository : BaseRepository<LeaveRequest>, ILeaveRequestRepository
    {
        public LeaveRequestRepository(AppDbContext context, ICurrentUserService current)
            : base(context, current) { }

        // Avoid FindAsync; use base (company-scoped)
        public Task<LeaveRequest?> GetByIdAsync(Guid id)
            => base.GetByIdAsync(id);

        public async Task<LeaveRequest?> GetByIdWithDetailsAsync(Guid id)
        {
            return await Query()
                .Include(lr => lr.Employee)
                .Include(lr => lr.Attachments)
                .FirstOrDefaultAsync(lr => lr.Id == id);
        }

        public async Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(Guid employeeId)
        {
            return await Query()
                .Where(lr => lr.EmployeeId == employeeId)
                .OrderByDescending(lr => lr.RequestDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetPendingAsync()
        {
            return await Query()
                .Include(lr => lr.Employee)
                .Where(lr => lr.Status == LeaveRequestStatus.Pending)
                .OrderBy(lr => lr.RequestDate)
                .ToListAsync();
        }

        public async Task<bool> HasOverlappingLeaveAsync(
            Guid employeeId, DateOnly start, DateOnly end, Guid? excludeId = null)
        {
            var query = Query()
                .Where(lr => lr.EmployeeId == employeeId &&
                             lr.Status == LeaveRequestStatus.Approved &&
                             (lr.StartDate <= end && lr.EndDate >= start));

            if (excludeId.HasValue)
                query = query.Where(lr => lr.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetApprovedByEmployeeAndPeriodAsync(
            Guid employeeId, DateOnly startDate, DateOnly endDate)
        {
            return await Query()
                .Where(lr => lr.EmployeeId == employeeId &&
                             lr.Status == LeaveRequestStatus.Approved &&
                             lr.StartDate <= endDate &&
                             lr.EndDate >= startDate)
                .ToListAsync();
        }

        // CRUD: delegate to base (enforces CompanyId + blocks cross-company updates)
        public Task AddAsync(LeaveRequest leaveRequest) => base.AddAsync(leaveRequest);
        public Task UpdateAsync(LeaveRequest leaveRequest) => base.UpdateAsync(leaveRequest);
        public Task DeleteAsync(Guid id) => base.DeleteAsync(id);
    }
}
