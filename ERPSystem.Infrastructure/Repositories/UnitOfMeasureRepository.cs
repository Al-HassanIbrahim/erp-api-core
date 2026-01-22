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

        public async Task<UnitOfMeasure?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.UnitsOfMeasure
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
        }

        public async Task<List<UnitOfMeasure>> GetAllByCompanyAsync(int companyId, CancellationToken cancellationToken = default)
        {
            return await _context.UnitsOfMeasure
                .AsNoTracking()
                .Where(u => u.CompanyId == companyId && !u.IsDeleted)
                .OrderBy(u => u.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(UnitOfMeasure unitOfMeasure, CancellationToken cancellationToken = default)
        {
            await _context.UnitsOfMeasure.AddAsync(unitOfMeasure, cancellationToken);
        }

        public void Update(UnitOfMeasure unitOfMeasure)
        {
            _context.UnitsOfMeasure.Update(unitOfMeasure);
        }

        public void Delete(UnitOfMeasure unitOfMeasure)
        {
            unitOfMeasure.IsDeleted = true;
            _context.UnitsOfMeasure.Update(unitOfMeasure);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
