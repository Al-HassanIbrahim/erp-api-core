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

        private void EnsureCompany(int companyId)
        {
            if (companyId != CompanyId)
                throw new UnauthorizedAccessException("Cross-company access is not allowed.");
        }

        public async Task<Attendance?> GetByIdAsync(Guid id, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id, ct);
        }

        public async Task<Attendance?> GetByEmployeeAndDateAsync(Guid employeeId, DateOnly date, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == date, ct);
        }

        public async Task<IReadOnlyList<Attendance>> GetByEmployeeAndPeriodAsync(
            Guid employeeId, DateOnly start, DateOnly end, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Where(a => a.EmployeeId == employeeId &&
                            a.Date >= start &&
                            a.Date <= end)
                .OrderBy(a => a.Date)
                .ToListAsync(ct);
        }

        public async Task<bool> HasCheckedInTodayAsync(Guid employeeId, DateOnly date, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .AnyAsync(a => a.EmployeeId == employeeId &&
                               a.Date == date &&
                               a.CheckInTime != null, ct);
        }

        public async Task<bool> IsPayrollProcessedForPeriodAsync(Guid employeeId, DateOnly date, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await _context.Payrolls
                .AnyAsync(p => p.CompanyId == CompanyId &&
                               p.EmployeeId == employeeId &&
                               p.PayPeriodStart <= date &&
                               p.PayPeriodEnd >= date &&
                               p.Status != PayrollStatus.Draft, ct);
        }

        // ✅ CRUD: delegate to base (CompanyId enforced + cross-company update blocked)
        public Task AddAsync(Attendance attendance) => base.AddAsync(attendance);
        public Task UpdateAsync(Attendance attendance) => base.UpdateAsync(attendance);

        public async Task DeleteAsync(Guid id, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);
            await base.DeleteAsync(id); // base uses GetByIdAsync scoped by company anyway
        }
    }
}
