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
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.UnitOfMeasure)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Product?> GetByIdAsync(int id, int companyId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.UnitOfMeasure)
                .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId && !p.IsDeleted);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.UnitOfMeasure)
                .Where(p => !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAllByCompanyAsync(int companyId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.UnitOfMeasure)
                .Where(p => p.CompanyId == companyId && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int id, int companyId)
        {
            return await _context.Products
                .AnyAsync(p => p.Id == id && p.CompanyId == companyId && !p.IsDeleted);
        }

        public async Task<bool> CodeExistsAsync(string code, int companyId, int? excludeId = null)
        {
            var query = _context.Products
                .Where(p => p.Code == code && p.CompanyId == companyId && !p.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        public async Task UpdateAsync(Product product)
        {
            product.UpdatedAt = DateTime.UtcNow;
            _context.Products.Update(product);
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsDeleted = true;
                product.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
