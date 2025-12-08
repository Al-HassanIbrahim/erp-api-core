using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Products;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories
{
    public class UnitOfMeasureRepository : IUnitOfMeasureRepository
    {
        private readonly AppDbContext _context;

        public UnitOfMeasureRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UnitOfMeasure?> GetByIdAsync(int id)
        {
            return await _context.UnitsOfMeasure.FindAsync(id);
        }

        public async Task<IEnumerable<UnitOfMeasure>> GetAllAsync()
        {
            return await _context.UnitsOfMeasure.ToListAsync();
        }

        public async Task AddAsync(UnitOfMeasure unitOfMeasure)
        {
            await _context.UnitsOfMeasure.AddAsync(unitOfMeasure);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UnitOfMeasure unitOfMeasure)
        {
            _context.UnitsOfMeasure.Update(unitOfMeasure);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var unitOfMeasure = await _context.UnitsOfMeasure.FindAsync(id);
            if (unitOfMeasure != null)
            {
                _context.UnitsOfMeasure.Remove(unitOfMeasure);
                await _context.SaveChangesAsync();
            }
        }
    }
}
