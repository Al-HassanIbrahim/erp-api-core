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
    public class AttendanceRepository
    {
        private readonly AppDbContext _context;

        public AttendanceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Attendance?> GetByIdAsync(Guid id)
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Attendance?> GetByEmployeeAndDateAsync(Guid employeeId, DateOnly date)
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == date);
        }

        public async Task<IEnumerable<Attendance>> GetByEmployeeAndPeriodAsync(
            Guid employeeId, DateOnly start, DateOnly end)
        {
            return await _context.Attendances
                .Where(a => a.EmployeeId == employeeId &&
                           a.Date >= start &&
                           a.Date <= end)
                .OrderBy(a => a.Date)
                .ToListAsync();
        }

        public async Task<bool> HasCheckedInTodayAsync(Guid employeeId, DateOnly date)
        {
            return await _context.Attendances
                .AnyAsync(a => a.EmployeeId == employeeId &&
                              a.Date == date &&
                              a.CheckInTime != null);
        }

        public async Task<bool> IsPayrollProcessedForPeriodAsync(Guid employeeId, DateOnly date)
        {
            return await _context.Payrolls
                .AnyAsync(p => p.EmployeeId == employeeId &&
                              p.PayPeriodStart <= date &&
                              p.PayPeriodEnd >= date &&
                              p.Status != PayrollStatus.Draft);
        }

        public async Task AddAsync(Attendance attendance)
        {
            await _context.Attendances.AddAsync(attendance);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Attendance attendance)
        {
            _context.Attendances.Update(attendance);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var attendance = await GetByIdAsync(id);
            if (attendance != null)
            {
                _context.Attendances.Remove(attendance);
                await _context.SaveChangesAsync();
            }
        }
    }
}
