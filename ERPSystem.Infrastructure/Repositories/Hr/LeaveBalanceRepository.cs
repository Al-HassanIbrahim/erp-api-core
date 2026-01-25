using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.HR;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Hr
{
    public class LeaveBalanceRepository : ILeaveBalanceRepository
    {
        private readonly AppDbContext _context;

        public LeaveBalanceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LeaveBalance?> GetByEmployeeYearAndTypeAsync(
            Guid employeeId, int year, LeaveType type)
        {
            return await _context.leaveBalances
                .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId &&
                                          lb.Year == year &&
                                          lb.LeaveType == type);
        }

        public async Task<IEnumerable<LeaveBalance>> GetByEmployeeAndYearAsync(
            Guid employeeId, int year)
        {
            return await _context.leaveBalances
                .Where(lb => lb.EmployeeId == employeeId && lb.Year == year)
                .ToListAsync();
        }

        public async Task AddAsync(LeaveBalance balance)
        {
            await _context.leaveBalances.AddAsync(balance);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LeaveBalance balance)
        {
            _context.leaveBalances.Update(balance);
            await _context.SaveChangesAsync();
        }
    }
}
