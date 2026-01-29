using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Hr
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(AppDbContext context, ICurrentUserService current)
            : base(context, current) { }

        private void EnsureCompany(int companyId)
        {
            if (companyId != CompanyId)
                throw new UnauthorizedAccessException("Cross-company access is not allowed.");
        }

        public async Task<Employee?> GetByIdAsync(Guid id, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .FirstOrDefaultAsync(e => e.Id == id, ct);
        }

        public async Task<Employee?> GetByIdWithDetailsAsync(Guid id, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Manager)
                .Include(e => e.DirectReports)
                .Include(e => e.Documents)
                .FirstOrDefaultAsync(e => e.Id == id, ct);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync(int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Include(e => e.Department)
                .Include(e => e.Position)
                .ToListAsync(ct);
        }

        public async Task<bool> ExistsByEmailAsync(string email, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            var normalized = email.Trim().ToLower();
            return await Query()
                .AnyAsync(e => e.Email != null && e.Email.ToLower() == normalized, ct);
        }

        public async Task<bool> ExistsByEmployeeCodeAsync(string code, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .AnyAsync(e => e.EmployeeCode == code, ct);
        }

        public async Task<bool> ExistsByNationalIdAsync(string nationalId, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .AnyAsync(e => e.NationalId == nationalId, ct);
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentIdAsync(Guid departmentId, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Include(e => e.Position)
                .Where(e => e.DepartmentId == departmentId)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Employee>> GetByStatusAsync(EmployeeStatus status, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Where(e => e.Status == status)
                .ToListAsync(ct);
        }

        public async Task<bool> HasCircularReportingAsync(Guid employeeId, Guid managerId, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            var visited = new HashSet<Guid>();
            var current = managerId;

            while (current != Guid.Empty)
            {
                if (current == employeeId) return true;
                if (!visited.Add(current)) return false;

                // Query() scoped للشركة الحالية، فمش هيمشي خارجها
                var next = await Query()
                    .Where(e => e.Id == current)
                    .Select(e => e.ReportsToId)
                    .FirstOrDefaultAsync(ct);

                current = next ?? Guid.Empty;
            }

            return false;
        }

        public Task AddAsync(Employee employee)
            => base.AddAsync(employee);

        public Task UpdateAsync(Employee employee)
            => base.UpdateAsync(employee);

        public async Task DeleteAsync(Guid id, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            // base.DeleteAsync already scoped via GetByIdAsync()
            await base.DeleteAsync(id);
        }
    }
}
