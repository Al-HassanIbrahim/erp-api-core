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

        // Keep only employee-specific queries (with Includes / special filters)

        public async Task<Employee?> GetByIdWithDetailsAsync(Guid id)
        {
            return await Query()
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Manager)
                .Include(e => e.DirectReports)
                .Include(e => e.Documents)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await Query()
                .Include(e => e.Department)
                .Include(e => e.Position)
                .ToListAsync();
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            var normalized = email.Trim().ToLower();
            return await Query()
                .AnyAsync(e => e.Email.ToLower() == normalized);
        }

        public async Task<bool> ExistsByEmployeeCodeAsync(string employeeCode)
        {
            return await Query()
                .AnyAsync(e => e.EmployeeCode == employeeCode);
        }

        public async Task<bool> ExistsByNationalIdAsync(string nationalId)
        {
            return await Query()
                .AnyAsync(e => e.NationalId == nationalId);
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentIdAsync(Guid departmentId)
        {
            return await Query()
                .Include(e => e.Position)
                .Where(e => e.DepartmentId == departmentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetByStatusAsync(EmployeeStatus status)
        {
            return await Query()
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Where(e => e.Status == status)
                .ToListAsync();
        }

        public async Task<bool> HasCircularReportingAsync(Guid employeeId, Guid managerId)
        {
            var visited = new HashSet<Guid>();
            var current = managerId;

            while (current != Guid.Empty)
            {
                if (current == employeeId) return true;
                if (visited.Contains(current)) return false;
                visited.Add(current);

                var next = await Query()
                    .Where(e => e.Id == current)
                    .Select(e => e.ReportsToId)
                    .FirstOrDefaultAsync();

                current = next ?? Guid.Empty;
            }

            return false;
        }

        // ✅ DO NOT re-implement CRUD here:
        // - GetByIdAsync(Guid id) comes from BaseRepository (company-scoped)
        // - AddAsync(Employee) comes from BaseRepository (enforces CompanyId)
        // - UpdateAsync(Employee) comes from BaseRepository (blocks cross-company update)
        // - DeleteAsync(Guid id) comes from BaseRepository (company-scoped)
    }
}
