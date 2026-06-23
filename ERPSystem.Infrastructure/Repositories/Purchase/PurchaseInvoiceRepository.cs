using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Purchase;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Infrastructure.Repositories.Purchase
{
    public sealed class PurchaseInvoiceRepository : IPurchaseInvoiceRepository
    {
        private readonly AppDbContext _db;
        public PurchaseInvoiceRepository(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<PurchaseInvoice>> GetAllByCompanyAsync(
            int companyId, CancellationToken ct = default)
            => await _db.PurchaseInvoices
                .Where(i => i.CompanyId == companyId && !i.IsDeleted)
                .Include(i => i.Supplier)
                .Include(i => i.Warehouse)
                .OrderByDescending(i => i.InvoiceDate)
                .ThenByDescending(i => i.Id)
                .AsNoTracking()
                .ToListAsync(ct);

        public Task<PurchaseInvoice?> GetByIdAsync(int id, int companyId, CancellationToken ct = default)
            => _db.PurchaseInvoices
                .Include(i => i.Supplier)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.Id == id && i.CompanyId == companyId && !i.IsDeleted, ct);

        public Task<PurchaseInvoice?> GetByIdWithLinesAsync(int id, int companyId, CancellationToken ct = default)
            => _db.PurchaseInvoices
                .Include(i => i.Supplier)
                .Include(i => i.Warehouse)
                .Include(i => i.Lines)
                    .ThenInclude(l => l.Product)
                .Include(i => i.Lines)
                    .ThenInclude(l => l.Unit)
                .FirstOrDefaultAsync(i => i.Id == id && i.CompanyId == companyId && !i.IsDeleted, ct);

        public async Task AddAsync(PurchaseInvoice invoice, CancellationToken ct = default)
            => await _db.PurchaseInvoices.AddAsync(invoice, ct);

        public void Update(PurchaseInvoice invoice)
            => _db.PurchaseInvoices.Update(invoice);

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }

}
