using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Sales
{
    public class SalesDeliveryRepository : ISalesDeliveryRepository
    {
        private readonly AppDbContext _context;

        public SalesDeliveryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SalesDelivery?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.SalesDeliveries
                .Include(d => d.Customer)
                .Include(d => d.SalesInvoice)
                .Include(d => d.Warehouse)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);
        }

        public async Task<SalesDelivery?> GetByIdWithLinesAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.SalesDeliveries
                .Include(d => d.Customer)
                .Include(d => d.SalesInvoice)
                    .ThenInclude(i => i.Lines)
                .Include(d => d.Warehouse)
                .Include(d => d.Lines)
                    .ThenInclude(l => l.Product)
                .Include(d => d.Lines)
                    .ThenInclude(l => l.Unit)
                .Include(d => d.Lines)
                    .ThenInclude(l => l.SalesInvoiceLine)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);
        }

        public async Task<List<SalesDelivery>> GetAllByCompanyAsync(
            int companyId,
            int? invoiceId = null,
            SalesDeliveryStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.SalesDeliveries
                .AsNoTracking()
                .Include(d => d.Customer)
                .Include(d => d.SalesInvoice)
                .Include(d => d.Warehouse)
                .Where(d => d.CompanyId == companyId && !d.IsDeleted);

            if (invoiceId.HasValue)
                query = query.Where(d => d.SalesInvoiceId == invoiceId.Value);

            if (status.HasValue)
                query = query.Where(d => d.Status == status.Value);

            if (fromDate.HasValue)
                query = query.Where(d => d.DeliveryDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(d => d.DeliveryDate <= toDate.Value);

            return await query
                .OrderByDescending(d => d.DeliveryDate)
                .ThenByDescending(d => d.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<string> GenerateDeliveryNumberAsync(int companyId, CancellationToken cancellationToken = default)
        {
            var count = await _context.SalesDeliveries
                .CountAsync(d => d.CompanyId == companyId, cancellationToken);

            return $"DEL-{DateTime.UtcNow:yyyyMM}-{(count + 1):D5}";
        }

        public async Task AddAsync(SalesDelivery delivery, CancellationToken cancellationToken = default)
        {
            await _context.SalesDeliveries.AddAsync(delivery, cancellationToken);
        }

        public void Update(SalesDelivery delivery)
        {
            _context.SalesDeliveries.Update(delivery);
        }

        public void Delete(SalesDelivery delivery)
        {
            delivery.IsDeleted = true;
            _context.SalesDeliveries.Update(delivery);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}