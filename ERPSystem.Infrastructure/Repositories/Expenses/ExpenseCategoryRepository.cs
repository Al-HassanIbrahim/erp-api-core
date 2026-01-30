using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Expenses;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Expenses
{
    public class ExpenseCategoryRepository : IExpenseCategoryRepository
    {
        private readonly AppDbContext _context;

        public ExpenseCategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<ExpenseCategory>> GetAllAsync(int companyId, CancellationToken ct)
        {
            return await _context.ExpenseCategories
                .AsNoTracking()
                .Where(c => c.CompanyId == companyId && !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync(ct);
        }

        public async Task<ExpenseCategory?> GetByIdAsync(int companyId, int id, CancellationToken ct)
        {
            return await _context.ExpenseCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId && !c.IsDeleted, ct);
        }

        public async Task<ExpenseCategory?> GetByIdForUpdateAsync(int companyId, int id, CancellationToken ct)
        {
            return await _context.ExpenseCategories
                .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId && !c.IsDeleted, ct);
        }

        public async Task<ExpenseCategory?> GetByNameAsync(int companyId, string name, CancellationToken ct)
        {
            return await _context.ExpenseCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CompanyId == companyId && !c.IsDeleted &&
                    c.Name.ToLower() == name.ToLower(), ct);
        }

        public async Task<bool> HasExpensesAsync(int companyId, int categoryId, CancellationToken ct)
        {
            return await _context.Expenses
                .AsNoTracking()
                .AnyAsync(e => e.CompanyId == companyId && e.ExpenseCategoryId == categoryId && !e.IsDeleted, ct);
        }

        public async Task<(int ExpenseCount, decimal TotalAmount)> GetCategoryStatsAsync(
            int companyId, int categoryId, CancellationToken ct)
        {
            var result = await _context.Expenses
                .AsNoTracking()
                .Where(e => e.CompanyId == companyId && e.ExpenseCategoryId == categoryId && !e.IsDeleted)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Count = g.Count(),
                    Total = g.Sum(e => e.Amount)
                })
                .FirstOrDefaultAsync(ct);

            return result == null ? (0, 0) : (result.Count, result.Total);
        }

        public async Task<ExpenseCategory> CreateAsync(ExpenseCategory entity, CancellationToken ct)
        {
            _context.ExpenseCategories.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task UpdateAsync(ExpenseCategory entity, CancellationToken ct)
        {
            _context.ExpenseCategories.Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(ExpenseCategory entity, CancellationToken ct)
        {
            entity.IsDeleted = true;
            await _context.SaveChangesAsync(ct);
        }
    }
}