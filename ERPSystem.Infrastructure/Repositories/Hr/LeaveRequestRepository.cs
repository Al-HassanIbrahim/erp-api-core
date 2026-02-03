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

        private void EnsureCompany(int companyId)
        {
            if (companyId != CompanyId)
                throw new UnauthorizedAccessException("Cross-company access is not allowed.");
        }

        public async Task<LeaveRequest?> GetByIdAsync(Guid id, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);
            return await Query().FirstOrDefaultAsync(lr => lr.Id == id, ct);
        }

        public async Task<LeaveRequest?> GetByIdWithDetailsAsync(Guid id, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Include(lr => lr.Employee)
                .Include(lr => lr.Attachments)
                .FirstOrDefaultAsync(lr => lr.Id == id, ct);
        }

        public async Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(Guid employeeId, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Where(lr => lr.EmployeeId == employeeId)
                .OrderByDescending(lr => lr.RequestDate)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<LeaveRequest>> GetPendingAsync(int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Include(lr => lr.Employee)
                .Where(lr => lr.Status == LeaveRequestStatus.Pending)
                .OrderBy(lr => lr.RequestDate)
                .ToListAsync(ct);
        }

        public async Task<bool> HasOverlappingLeaveAsync(
            Guid employeeId, DateOnly start, DateOnly end, int companyId, Guid? excludeId = null, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            var query = Query()
                .Where(lr => lr.EmployeeId == employeeId &&
                             lr.Status == LeaveRequestStatus.Approved || lr.Status == LeaveRequestStatus.Pending &&
                             (lr.StartDate <= end && lr.EndDate >= start));

            if (excludeId.HasValue)
                query = query.Where(lr => lr.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }

        public async Task<IEnumerable<LeaveRequest>> GetApprovedByEmployeeAndPeriodAsync(
            Guid employeeId, DateOnly startDate, DateOnly endDate, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Where(lr => lr.EmployeeId == employeeId &&
                             lr.Status == LeaveRequestStatus.Approved &&
                             lr.StartDate <= endDate &&
                             lr.EndDate >= startDate)
                .ToListAsync(ct);
        }

        public Task<bool> AnyByEmployeeAsync(Guid employeeId, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);
            return Query().AnyAsync(lr => lr.EmployeeId == employeeId, ct);
        }

        // CRUD: delegate to base (enforces CompanyId + blocks cross-company updates)
        public Task AddAsync(LeaveRequest leaveRequest) => base.AddAsync(leaveRequest);
        public Task UpdateAsync(LeaveRequest leaveRequest) => base.UpdateAsync(leaveRequest);

        public async Task DeleteAsync(Guid id, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);
            await base.DeleteAsync(id);
        }
    }
}
