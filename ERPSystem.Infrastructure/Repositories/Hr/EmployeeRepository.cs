using ERPSystem.Domain.Abstractions;
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
    public class EmployeeRepository: IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Employee?> GetByIdAsync(Guid id)
        {
            return await _context.Employees.FindAsync(id);
        }

        public async Task<Employee?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Manager)
                .Include(e => e.DirectReports)
                .Include(e => e.Documents)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .ToListAsync();
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Employees
                .AnyAsync(e => e.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> ExistsByEmployeeCodeAsync(string employeeCode)
        {
            return await _context.Employees
                .AnyAsync(e => e.EmployeeCode == employeeCode);
        }

        public async Task<bool> ExistsByNationalIdAsync(string nationalId)
        {
            return await _context.Employees
                .AnyAsync(e => e.NationalId == nationalId);
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentIdAsync(Guid departmentId)
        {
            return await _context.Employees
                .Include(e => e.Position)
                .Where(e => e.DepartmentId == departmentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetByStatusAsync(EmployeeStatus status)
        {
            return await _context.Employees
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
                if (current == employeeId)
                    return true;

                if (visited.Contains(current))
                    return false;

                visited.Add(current);

                var manager = await _context.Employees
                    .Where(e => e.Id == current)
                    .Select(e => e.ReportsToId)
                    .FirstOrDefaultAsync();

                current = manager ?? Guid.Empty;
            }

            return false;
        }

        public async Task AddAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var employee = await GetByIdAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
        }
    }
}
