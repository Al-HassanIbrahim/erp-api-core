using ERPSystem.Domain.Entities.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Hr
{
    public class PayrollRepository
    {
        private readonly AppDbContext _context;

        public PayrollRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Payroll?> GetByIdAsync(Guid id)
        {
            return await _context.Payrolls.FindAsync(id);
        }

        public async Task<Payroll?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .Include(p => p.LineItems)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Payroll>> GetByEmployeeIdAsync(Guid employeeId)
        {
            return await _context.Payrolls
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .ToListAsync();
        }

        public async Task<Payroll?> GetByEmployeeAndPeriodAsync(
            Guid employeeId, int month, int year)
        {
            return await _context.Payrolls
                .Include(p => p.LineItems)
                .FirstOrDefaultAsync(p => p.EmployeeId == employeeId &&
                                         p.Month == month &&
                                         p.Year == year);
        }

        public async Task<IEnumerable<Payroll>> GetByMonthAndYearAsync(int month, int year)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .Where(p => p.Month == month && p.Year == year)
                .ToListAsync();
        }

        public async Task<bool> ExistsForEmployeeAndPeriodAsync(
            Guid employeeId, int month, int year)
        {
            return await _context.Payrolls
                .AnyAsync(p => p.EmployeeId == employeeId &&
                              p.Month == month &&
                              p.Year == year);
        }

        public async Task AddAsync(Payroll payroll)
        {
            await _context.Payrolls.AddAsync(payroll);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Payroll payroll)
        {
            _context.Payrolls.Update(payroll);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var payroll = await GetByIdAsync(id);
            if (payroll != null)
            {
                _context.Payrolls.Remove(payroll);
                await _context.SaveChangesAsync();
            }
        }
    }
}
