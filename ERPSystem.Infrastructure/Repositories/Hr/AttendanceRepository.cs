using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.HR;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Hr
{
    public class AttendanceRepository : BaseRepository<Attendance>, IAttendanceRepository
    {
        public AttendanceRepository(AppDbContext context, ICurrentUserService current)
            : base(context, current) { }

        public async Task<Attendance?> GetByIdAsync(Guid id)
        {
            // Use Query() to enforce company scoping
            return await Query()
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Attendance?> GetByEmployeeAndDateAsync(Guid employeeId, DateOnly date)
        {
            // Company scoping + employeeId filter
            return await Query()
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == date);
        }

        public async Task<IEnumerable<Attendance>> GetByEmployeeAndPeriodAsync(
            Guid employeeId, DateOnly start, DateOnly end)
        {
            return await Query()
                .Where(a => a.EmployeeId == employeeId &&
                            a.Date >= start &&
                            a.Date <= end)
                .OrderBy(a => a.Date)
                .ToListAsync();
        }

        public async Task<bool> HasCheckedInTodayAsync(Guid employeeId, DateOnly date)
        {
            return await Query()
                .AnyAsync(a => a.EmployeeId == employeeId &&
                               a.Date == date &&
                               a.CheckInTime != null);
        }

        public async Task<bool> IsPayrollProcessedForPeriodAsync(Guid employeeId, DateOnly date)
        {
            // Payroll must also be company-scoped.
            // If Payroll implements ICompanyEntity: prefer Query() in PayrollRepository.
            // Here we enforce company filter explicitly.
            return await _context.Payrolls
                .AnyAsync(p => p.CompanyId == CompanyId &&
                               p.EmployeeId == employeeId &&
                               p.PayPeriodStart <= date &&
                               p.PayPeriodEnd >= date &&
                               p.Status != PayrollStatus.Draft);
        }

        // ✅ CRUD: delegate to base (so CompanyId is enforced and cross-company updates are blocked)
        public Task AddAsync(Attendance attendance) => base.AddAsync(attendance);
        public Task UpdateAsync(Attendance attendance) => base.UpdateAsync(attendance);
        public Task DeleteAsync(Guid id) => base.DeleteAsync(id);
    }
}
