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
                    .FirstOrDefaultAsync(p => p.Id == id);
            }

            public async Task<IEnumerable<Product>> GetAllAsync()
            {
                return await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.UnitOfMeasure)
                    .ToListAsync();
            }

            public async Task AddAsync(Product product)
            {
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
            }

            public async Task UpdateAsync(Product product)
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }

            public async Task DeleteAsync(int id)
            {
                var product = await _context.Products.FindAsync(id);
                if (product != null)
                {
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
