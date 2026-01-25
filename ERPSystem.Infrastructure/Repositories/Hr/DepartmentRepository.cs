using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.HR;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Infrastructure.Repositories.Hr
{
    public class DepartmentRepository:IDepartmentRepository
    {
        private readonly AppDbContext _context;

        public DepartmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Department?> GetByIdAsync(Guid id)
        {
            return await _context.Departments.FindAsync(id);
        }

        public async Task<Department?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Departments
                .Include(d => d.Manager)
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await _context.Departments
                .Include(d => d.Manager)
                .ToListAsync();
        }

        public async Task<bool> ExistsByCodeAsync(string code)
        {
            return await _context.Departments
                .AnyAsync(d => d.Code == code);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Departments
                .AnyAsync(d => d.Name.ToLower() == name.ToLower());
        }

        public async Task<int> GetEmployeeCountAsync(Guid departmentId)
        {
            return await _context.Employees
                .CountAsync(e => e.DepartmentId == departmentId);
        }

        public async Task AddAsync(Department department)
        {
            await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Department department)
        {
            _context.Departments.Update(department);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var department = await GetByIdAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
            }
        }
    }
}
