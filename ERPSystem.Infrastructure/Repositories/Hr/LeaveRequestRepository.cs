using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.HR;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Infrastructure.Repositories.Hr
{
    public class LeaveRequestRepository:ILeaveRequestRepository
    {
        private readonly AppDbContext _context;

        public LeaveRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LeaveRequest?> GetByIdAsync(Guid id)
        {
            return await _context.LeaveRequests.FindAsync(id);
        }

        public async Task<LeaveRequest?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.Attachments)
                .FirstOrDefaultAsync(lr => lr.Id == id);
        }

        public async Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(Guid employeeId)
        {
            return await _context.LeaveRequests
                .Where(lr => lr.EmployeeId == employeeId)
                .OrderByDescending(lr => lr.RequestDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetPendingAsync()
        {
            return await _context.LeaveRequests
                .Include(lr => lr.Employee)
                .Where(lr => lr.Status == LeaveRequestStatus.Pending)
                .OrderBy(lr => lr.RequestDate)
                .ToListAsync();
        }

        public async Task<bool> HasOverlappingLeaveAsync(
            Guid employeeId, DateOnly start, DateOnly end, Guid? excludeId = null)
        {
            var query = _context.LeaveRequests
                .Where(lr => lr.EmployeeId == employeeId &&
                            lr.Status == LeaveRequestStatus.Approved &&
                            ((lr.StartDate <= end && lr.EndDate >= start)));

            if (excludeId.HasValue)
            {
                query = query.Where(lr => lr.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task AddAsync(LeaveRequest leaveRequest)
        {
            await _context.LeaveRequests.AddAsync(leaveRequest);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LeaveRequest leaveRequest)
        {
            _context.LeaveRequests.Update(leaveRequest);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var leaveRequest = await GetByIdAsync(id);
            if (leaveRequest != null)
            {
                _context.LeaveRequests.Remove(leaveRequest);
                await _context.SaveChangesAsync();
            }
        }
    }
}
