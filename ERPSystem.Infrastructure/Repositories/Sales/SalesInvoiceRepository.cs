using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Sales
{
    public class SalesInvoiceRepository : ISalesInvoiceRepository
    {
        private readonly AppDbContext _context;

        public SalesInvoiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SalesInvoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.SalesInvoices
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);
        }

        public async Task<SalesInvoice?> GetByIdWithLinesAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.SalesInvoices
                .Include(i => i.Customer)
                .Include(i => i.Lines)
                    .ThenInclude(l => l.Product)
                .Include(i => i.Lines)
                    .ThenInclude(l => l.Unit)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);
        }

        public async Task<List<SalesInvoice>> GetAllByCompanyAsync(
            int companyId,
            int? customerId = null,
            SalesInvoiceStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.SalesInvoices
                .AsNoTracking()
                .Include(i => i.Customer)
                .Where(i => i.CompanyId == companyId && !i.IsDeleted);

            if (customerId.HasValue)
                query = query.Where(i => i.CustomerId == customerId.Value);

            if (status.HasValue)
                query = query.Where(i => i.Status == status.Value);

            if (fromDate.HasValue)
                query = query.Where(i => i.InvoiceDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => i.InvoiceDate <= toDate.Value);

            return await query
                .OrderByDescending(i => i.InvoiceDate)
                .ThenByDescending(i => i.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<string> GenerateInvoiceNumberAsync(int companyId, CancellationToken cancellationToken = default)
        {
            var count = await _context.SalesInvoices
                .CountAsync(i => i.CompanyId == companyId, cancellationToken);

            return $"INV-{DateTime.UtcNow:yyyyMM}-{(count + 1):D5}";
        }

        public async Task AddAsync(SalesInvoice invoice, CancellationToken cancellationToken = default)
        {
            await _context.SalesInvoices.AddAsync(invoice, cancellationToken);
        }

        public void Update(SalesInvoice invoice)
        {
            _context.SalesInvoices.Update(invoice);
        }

        public void Delete(SalesInvoice invoice)
        {
            invoice.IsDeleted = true;
            _context.SalesInvoices.Update(invoice);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}