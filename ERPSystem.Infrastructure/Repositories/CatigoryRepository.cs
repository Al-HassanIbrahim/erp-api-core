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
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
        }

        public async Task<List<Category>> GetAllByCompanyAsync(int companyId, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .AsNoTracking()
                .Include(c => c.ParentCategory)
                .Where(c => c.CompanyId == companyId && !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
        {
            await _context.Categories.AddAsync(category, cancellationToken);
        }

        public void Update(Category category)
        {
            _context.Categories.Update(category);
        }

        public void Delete(Category category)
        {
            category.IsDeleted = true;
            _context.Categories.Update(category);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
