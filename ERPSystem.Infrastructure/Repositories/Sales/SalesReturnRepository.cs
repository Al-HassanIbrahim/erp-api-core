using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Sales
{
    public class SalesReturnRepository : ISalesReturnRepository
    {
        private readonly AppDbContext _context;

        public SalesReturnRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SalesReturn?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.SalesReturns
                .Include(r => r.Customer)
                .Include(r => r.Warehouse)
                .Include(r => r.SalesInvoice)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
        }

        public async Task<SalesReturn?> GetByIdWithLinesAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.SalesReturns
                .Include(r => r.Customer)
                .Include(r => r.Warehouse)
                .Include(r => r.SalesInvoice)
                .Include(r => r.Lines)
                    .ThenInclude(l => l.Product)
                .Include(r => r.Lines)
                    .ThenInclude(l => l.Unit)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
        }

        public async Task<List<SalesReturn>> GetAllByCompanyAsync(
            int companyId,
            int? customerId = null,
            SalesReturnStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.SalesReturns
                .AsNoTracking()
                .Include(r => r.Customer)
                .Include(r => r.Warehouse)
                .Where(r => r.CompanyId == companyId && !r.IsDeleted);

            if (customerId.HasValue)
                query = query.Where(r => r.CustomerId == customerId.Value);

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            if (fromDate.HasValue)
                query = query.Where(r => r.ReturnDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(r => r.ReturnDate <= toDate.Value);

            return await query
                .OrderByDescending(r => r.ReturnDate)
                .ThenByDescending(r => r.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<string> GenerateReturnNumberAsync(int companyId, CancellationToken cancellationToken = default)
        {
            var count = await _context.SalesReturns
                .CountAsync(r => r.CompanyId == companyId, cancellationToken);

            return $"RET-{DateTime.UtcNow:yyyyMM}-{(count + 1):D5}";
        }

        public async Task AddAsync(SalesReturn salesReturn, CancellationToken cancellationToken = default)
        {
            await _context.SalesReturns.AddAsync(salesReturn, cancellationToken);
        }

        public void Update(SalesReturn salesReturn)
        {
            _context.SalesReturns.Update(salesReturn);
        }

        public void Delete(SalesReturn salesReturn)
        {
            salesReturn.IsDeleted = true;
            _context.SalesReturns.Update(salesReturn);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}