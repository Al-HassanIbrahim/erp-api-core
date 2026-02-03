using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.HR;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Hr
{
    public class PayrollRepository : BaseRepository<Payroll>, IPayrollRepository
    {
        public PayrollRepository(AppDbContext context, ICurrentUserService current)
            : base(context, current) { }

        private void EnsureCompany(int companyId)
        {
            if (companyId != CompanyId)
                throw new UnauthorizedAccessException("Cross-company access is not allowed.");
        }

        public async Task<Payroll?> GetByIdAsync(Guid id, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);
            return await Query().FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task<Payroll?> GetByIdWithDetailsAsync(Guid id, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Include(p => p.Employee)
                .Include(p => p.LineItems)
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task<IEnumerable<Payroll>> GetByEmployeeIdAsync(Guid employeeId, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .ToListAsync(ct);
        }

        public async Task<Payroll?> GetByEmployeeAndPeriodAsync(Guid employeeId, int month, int year, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Include(p => p.LineItems)
                .FirstOrDefaultAsync(p => p.EmployeeId == employeeId &&
                                          p.Month == month &&
                                          p.Year == year, ct);
        }

        public async Task<IEnumerable<Payroll>> GetByMonthAndYearAsync(int month, int year, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Include(p => p.Employee)
                .Where(p => p.Month == month && p.Year == year)
                .ToListAsync(ct);
        }

        public async Task<bool> ExistsForEmployeeAndPeriodAsync(Guid employeeId, int month, int year, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .AnyAsync(p => p.EmployeeId == employeeId &&
                               p.Month == month &&
                               p.Year == year, ct);
        }

        public Task AddAsync(Payroll payroll) => base.AddAsync(payroll);
        public Task UpdateAsync(Payroll payroll) => base.UpdateAsync(payroll);

        public async Task DeleteAsync(Guid id, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);
            await base.DeleteAsync(id);
        }
    }
}
