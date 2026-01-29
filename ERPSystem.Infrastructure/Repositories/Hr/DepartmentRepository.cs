using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.HR;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Hr
{
    public class DepartmentRepository : BaseRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(AppDbContext context, ICurrentUserService current)
            : base(context, current) { }

        private void EnsureCompany(int companyId)
        {
            if (companyId != CompanyId)
                throw new UnauthorizedAccessException("Cross-company access is not allowed.");
        }

        public async Task<Department?> GetByIdAsync(Guid id, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);
            return await Query().FirstOrDefaultAsync(d => d.Id == id, ct);
        }

        public async Task<Department?> GetByIdWithDetailsAsync(Guid id, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Include(d => d.Manager)
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id, ct);
        }

        public async Task<IEnumerable<Department>> GetAllAsync(int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Include(d => d.Manager)
                .ToListAsync(ct);
        }

        public async Task<bool> ExistsByCodeAsync(string code, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);
            return await Query().AnyAsync(d => d.Code == code, ct);
        }

        public async Task<bool> ExistsByNameAsync(string name, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            var normalized = name.Trim().ToLower();
            return await Query()
                .AnyAsync(d => d.Name != null && d.Name.ToLower() == normalized, ct);
        }

        public async Task<int> GetEmployeeCountAsync(Guid departmentId, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await _context.Employees
                .CountAsync(e => e.CompanyId == CompanyId && e.DepartmentId == departmentId, ct);
        }

        // Interface: AddAsync بدون companyId
        public Task AddAsync(Department department, CancellationToken ct = default)
        {
            // ct مش مستخدم داخل base.AddAsync (BaseRepo ما بيدعمش ct)
            return base.AddAsync(department);
        }

        // Interface: UpdateAsync بدون companyId
        public Task UpdateAsync(Department department, CancellationToken ct = default)
        {
            return base.UpdateAsync(department);
        }

        public async Task DeleteAsync(Guid id, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);
            await base.DeleteAsync(id); // ct مش مستخدم (BaseRepo ما بيدعمش ct)
        }
    }
}
