using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Expenses;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Expenses
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly AppDbContext _context;

        public ExpenseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(IReadOnlyList<Expense> Items, int TotalCount)> GetPagedAsync(
            int companyId,
            string? search,
            int? categoryId,
            ExpenseStatus? status,
            PaymentMethod? paymentMethod,
            DateTime? fromDate,
            DateTime? toDate,
            decimal? minAmount,
            decimal? maxAmount,
            string? sortBy,
            string? sortDir,
            int page,
            int pageSize,
            CancellationToken ct)
        {
            var query = _context.Expenses
                .AsNoTracking()
                .Include(e => e.Category)
                .Where(e => e.CompanyId == companyId && !e.IsDeleted);

            // Search in description and vendor
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(e =>
                    e.Description.ToLower().Contains(searchLower) ||
                    (e.Vendor != null && e.Vendor.ToLower().Contains(searchLower)));
            }

            // Filters
            if (categoryId.HasValue)
                query = query.Where(e => e.ExpenseCategoryId == categoryId.Value);

            if (status.HasValue)
                query = query.Where(e => e.Status == status.Value);

            if (paymentMethod.HasValue)
                query = query.Where(e => e.PaymentMethod == paymentMethod.Value);

            if (fromDate.HasValue)
                query = query.Where(e => e.ExpenseDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(e => e.ExpenseDate <= toDate.Value.Date);

            if (minAmount.HasValue)
                query = query.Where(e => e.Amount >= minAmount.Value);

            if (maxAmount.HasValue)
                query = query.Where(e => e.Amount <= maxAmount.Value);

            // Get total count before pagination
            var totalCount = await query.CountAsync(ct);

            // Sorting
            var sortByLower = sortBy?.ToLower();
            var isAscending = sortDir?.ToLower() == "asc";

            query = (sortByLower, isAscending) switch
            {
                ("amount", true) => query.OrderBy(e => e.Amount),
                ("amount", false) => query.OrderByDescending(e => e.Amount),
                ("description", true) => query.OrderBy(e => e.Description),
                ("description", false) => query.OrderByDescending(e => e.Description),
                ("expensedate", true) => query.OrderBy(e => e.ExpenseDate),
                ("expensedate", false) => query.OrderByDescending(e => e.ExpenseDate),
                _ => query.OrderByDescending(e => e.ExpenseDate) // Default: newest first
            };

            // Pagination
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<Expense?> GetByIdAsync(int companyId, int id, CancellationToken ct)
        {
            return await _context.Expenses
                .AsNoTracking()
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id && e.CompanyId == companyId && !e.IsDeleted, ct);
        }

        public async Task<Expense?> GetByIdForUpdateAsync(int companyId, int id, CancellationToken ct)
        {
            return await _context.Expenses
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id && e.CompanyId == companyId && !e.IsDeleted, ct);
        }

        public async Task<Expense> CreateAsync(Expense entity, CancellationToken ct)
        {
            _context.Expenses.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task UpdateAsync(Expense entity, CancellationToken ct)
        {
            _context.Expenses.Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Expense entity, CancellationToken ct)
        {
            entity.IsDeleted = true;
            await _context.SaveChangesAsync(ct);
        }

        public async Task<(decimal Total, decimal Max, decimal Min, int Count)> GetSummaryAsync(
            int companyId, DateTime? from, DateTime? to, CancellationToken ct)
        {
            var query = _context.Expenses
                .AsNoTracking()
                .Where(e => e.CompanyId == companyId && !e.IsDeleted);

            if (from.HasValue)
                query = query.Where(e => e.ExpenseDate >= from.Value.Date);

            if (to.HasValue)
                query = query.Where(e => e.ExpenseDate <= to.Value.Date);

            var result = await query
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Sum(e => e.Amount),
                    Max = g.Max(e => e.Amount),
                    Min = g.Min(e => e.Amount),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync(ct);

            if (result == null)
                return (0, 0, 0, 0);

            return (result.Total, result.Max, result.Min, result.Count);
        }

        public async Task<IReadOnlyList<(DateTime Date, decimal Amount, int Count)>> GetDailyTotalsAsync(
            int companyId, DateTime? from, DateTime? to, CancellationToken ct)
        {
            var query = _context.Expenses
                .AsNoTracking()
                .Where(e => e.CompanyId == companyId && !e.IsDeleted);

            if (from.HasValue)
                query = query.Where(e => e.ExpenseDate >= from.Value.Date);

            if (to.HasValue)
                query = query.Where(e => e.ExpenseDate <= to.Value.Date);

            var result = await query
                .GroupBy(e => e.ExpenseDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key,
                    Amount = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .ToListAsync(ct);

            return result.Select(r => (r.Date, r.Amount, r.Count)).ToList();
        }

        public async Task<IReadOnlyList<(int CategoryId, string CategoryName, decimal Amount, int Count)>> GetCategoryTotalsAsync(
            int companyId, DateTime? from, DateTime? to, CancellationToken ct)
        {
            var query = _context.Expenses
                .AsNoTracking()
                .Include(e => e.Category)
                .Where(e => e.CompanyId == companyId && !e.IsDeleted);

            if (from.HasValue)
                query = query.Where(e => e.ExpenseDate >= from.Value.Date);

            if (to.HasValue)
                query = query.Where(e => e.ExpenseDate <= to.Value.Date);

            var result = await query
                .GroupBy(e => new { e.ExpenseCategoryId, e.Category.Name })
                .Select(g => new
                {
                    CategoryId = g.Key.ExpenseCategoryId,
                    CategoryName = g.Key.Name,
                    Amount = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Amount)
                .ToListAsync(ct);

            return result.Select(r => (r.CategoryId, r.CategoryName, r.Amount, r.Count)).ToList();
        }
    }
}