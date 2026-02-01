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

            var employee = await _context.Employees
                .Include(e => e.Attendances)
                .Include(e => e.LeaveRequests)
                    .ThenInclude(lr => lr.Attachments)
                .Include(e => e.Payrolls)
                    .ThenInclude(p => p.LineItems)
                .Include(e => e.Documents)
                .FirstOrDefaultAsync(e => e.Id == id && e.CompanyId == companyId, ct);

            if (employee == null)
                throw new KeyNotFoundException($"Employee with ID {id} not found");

            if (employee.Attendances?.Any() == true)
                _context.Attendances.RemoveRange(employee.Attendances);

            if (employee.LeaveRequests?.Any() == true)
                _context.LeaveRequests.RemoveRange(employee.LeaveRequests);

            if (employee.Payrolls?.Any() == true)
                _context.Payrolls.RemoveRange(employee.Payrolls);

            if (employee.Documents?.Any() == true)
                _context.EmployeeDocuments.RemoveRange(employee.Documents);

            _context.Employees.Remove(employee);

            await _context.SaveChangesAsync(ct);
        }
    }
}
