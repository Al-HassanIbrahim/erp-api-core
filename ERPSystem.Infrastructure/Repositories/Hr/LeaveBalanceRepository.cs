using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.HR;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Hr
{
    public class LeaveBalanceRepository : BaseRepository<LeaveBalance>, ILeaveBalanceRepository
    {
        public LeaveBalanceRepository(AppDbContext context, ICurrentUserService current)
            : base(context, current) { }

        private void EnsureCompany(int companyId)
        {
            if (companyId != CompanyId)
                throw new UnauthorizedAccessException("Cross-company access is not allowed.");
        }

        public async Task<LeaveBalance?> GetByEmployeeYearAndTypeAsync(
            Guid employeeId, int year, LeaveType type, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId &&
                                           lb.Year == year &&
                                           lb.LeaveType == type, ct);
        }

        public async Task<IEnumerable<LeaveBalance>> GetByEmployeeAndYearAsync(
            Guid employeeId, int year, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Where(lb => lb.EmployeeId == employeeId && lb.Year == year)
                .ToListAsync(ct);
        }

        public Task AddAsync(LeaveBalance balance) => base.AddAsync(balance);
        public Task UpdateAsync(LeaveBalance balance) => base.UpdateAsync(balance);
    }
}
