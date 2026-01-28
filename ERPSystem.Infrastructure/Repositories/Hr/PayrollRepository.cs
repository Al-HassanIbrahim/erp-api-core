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

        // Avoid FindAsync; use base (company-scoped)
        public Task<Payroll?> GetByIdAsync(Guid id)
            => base.GetByIdAsync(id);

        public async Task<Payroll?> GetByIdWithDetailsAsync(Guid id)
        {
            return await Query()
                .Include(p => p.Employee)
                .Include(p => p.LineItems)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Payroll>> GetByEmployeeIdAsync(Guid employeeId)
        {
            return await Query()
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .ToListAsync();
        }

        public async Task<Payroll?> GetByEmployeeAndPeriodAsync(Guid employeeId, int month, int year)
        {
            return await Query()
                .Include(p => p.LineItems)
                .FirstOrDefaultAsync(p => p.EmployeeId == employeeId &&
                                          p.Month == month &&
                                          p.Year == year);
        }

        public async Task<IEnumerable<Payroll>> GetByMonthAndYearAsync(int month, int year)
        {
            return await Query()
                .Include(p => p.Employee)
                .Where(p => p.Month == month && p.Year == year)
                .ToListAsync();
        }

        public async Task<bool> ExistsForEmployeeAndPeriodAsync(Guid employeeId, int month, int year)
        {
            return await Query()
                .AnyAsync(p => p.EmployeeId == employeeId &&
                               p.Month == month &&
                               p.Year == year);
        }

        // CRUD: delegate to base (enforces CompanyId + blocks cross-company updates)
        public Task AddAsync(Payroll payroll) => base.AddAsync(payroll);
        public Task UpdateAsync(Payroll payroll) => base.UpdateAsync(payroll);
        public Task DeleteAsync(Guid id) => base.DeleteAsync(id);
    }
}
