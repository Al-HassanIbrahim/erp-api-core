using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.HR;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Hr
{
    public class DepartmentRepository : BaseRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(AppDbContext context, ICurrentUserService current)
            : base(context, current) { }

        public Task<Department?> GetByIdAsync(Guid id)
            => base.GetByIdAsync(id);

        public async Task<Department?> GetByIdWithDetailsAsync(Guid id)
        {
            return await Query()
                .Include(d => d.Manager)
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await Query()
                .Include(d => d.Manager)
                .ToListAsync();
        }

        public async Task<bool> ExistsByCodeAsync(string code)
            => await Query().AnyAsync(d => d.Code == code);

        public async Task<bool> ExistsByNameAsync(string name)
        {
            var normalized = name.Trim().ToLower();
            return await Query().AnyAsync(d => d.Name.ToLower() == normalized);
        }

        public async Task<int> GetEmployeeCountAsync(Guid departmentId)
        {
            return await _context.Employees
                .CountAsync(e => e.CompanyId == CompanyId && e.DepartmentId == departmentId);
        }

        public Task AddAsync(Department department) => base.AddAsync(department);
        public Task UpdateAsync(Department department) => base.UpdateAsync(department);
        public Task DeleteAsync(Guid id) => base.DeleteAsync(id);
    }
}
