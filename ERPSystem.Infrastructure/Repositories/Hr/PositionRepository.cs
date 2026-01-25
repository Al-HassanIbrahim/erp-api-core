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
    public class PositionRepository:IPositionRepository
    {
        private readonly AppDbContext _context;

        public PositionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<JobPosition?> GetByIdAsync(Guid id)
        {
            return await _context.JobPositions
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<JobPosition>> GetAllAsync()
        {
            return await _context.JobPositions
                .Include(p => p.Department)
                .ToListAsync();
        }

        public async Task<bool> ExistsByCodeAsync(string code)
        {
            return await _context.JobPositions
                .AnyAsync(p => p.Code == code);
        }

        public async Task AddAsync(JobPosition position)
        {
            await _context.JobPositions.AddAsync(position);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(JobPosition position)
        {
            _context.JobPositions.Update(position);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var position = await GetByIdAsync(id);
            if (position != null)
            {
                _context.JobPositions.Remove(position);
                await _context.SaveChangesAsync();
            }
        }
    }
}
