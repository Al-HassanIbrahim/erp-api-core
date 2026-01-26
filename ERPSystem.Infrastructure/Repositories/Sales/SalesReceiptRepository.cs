using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Sales
{
    public class SalesReceiptRepository : ISalesReceiptRepository
    {
        private readonly AppDbContext _context;

        public SalesReceiptRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SalesReceipt?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.SalesReceipts
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
        }

        public async Task<SalesReceipt?> GetByIdWithAllocationsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.SalesReceipts
                .Include(r => r.Customer)
                .Include(r => r.Allocations)
                    .ThenInclude(a => a.SalesInvoice)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
        }

        public async Task<List<SalesReceipt>> GetAllByCompanyAsync(
            int companyId,
            int? customerId = null,
            SalesReceiptStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.SalesReceipts
                .AsNoTracking()
                .Include(r => r.Customer)
                .Where(r => r.CompanyId == companyId && !r.IsDeleted);

            if (customerId.HasValue)
                query = query.Where(r => r.CustomerId == customerId.Value);

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            if (fromDate.HasValue)
                query = query.Where(r => r.ReceiptDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(r => r.ReceiptDate <= toDate.Value);

            return await query
                .OrderByDescending(r => r.ReceiptDate)
                .ThenByDescending(r => r.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<string> GenerateReceiptNumberAsync(int companyId, CancellationToken cancellationToken = default)
        {
            var count = await _context.SalesReceipts
                .CountAsync(r => r.CompanyId == companyId, cancellationToken);

            return $"RCP-{DateTime.UtcNow:yyyyMM}-{(count + 1):D5}";
        }

        public async Task AddAsync(SalesReceipt receipt, CancellationToken cancellationToken = default)
        {
            await _context.SalesReceipts.AddAsync(receipt, cancellationToken);
        }

        public void Update(SalesReceipt receipt)
        {
            _context.SalesReceipts.Update(receipt);
        }

        public void Delete(SalesReceipt receipt)
        {
            receipt.IsDeleted = true;
            _context.SalesReceipts.Update(receipt);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}