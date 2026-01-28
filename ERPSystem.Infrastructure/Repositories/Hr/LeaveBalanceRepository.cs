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

        public async Task<LeaveBalance?> GetByEmployeeYearAndTypeAsync(
            Guid employeeId, int year, LeaveType type)
        {
            return await Query()
                .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId &&
                                           lb.Year == year &&
                                           lb.LeaveType == type);
        }

        public async Task<IEnumerable<LeaveBalance>> GetByEmployeeAndYearAsync(
            Guid employeeId, int year)
        {
            return await Query()
                .Where(lb => lb.EmployeeId == employeeId && lb.Year == year)
                .ToListAsync();
        }

        // CRUD: delegate to base (enforces CompanyId + blocks cross-company updates)
        public Task AddAsync(LeaveBalance balance) => base.AddAsync(balance);
        public Task UpdateAsync(LeaveBalance balance) => base.UpdateAsync(balance);
    }
}
