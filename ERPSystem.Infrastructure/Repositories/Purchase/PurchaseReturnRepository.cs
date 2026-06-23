using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Purchase;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Infrastructure.Repositories.Purchase
{
    public sealed class PurchaseReturnRepository : IPurchaseReturnRepository
    {
        private readonly AppDbContext _db;
        public PurchaseReturnRepository(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<PurchaseReturn>> GetAllByCompanyAsync(
            int companyId, CancellationToken ct = default)
            => await _db.PurchaseReturns
                .Where(r => r.CompanyId == companyId && !r.IsDeleted)
                .Include(r => r.Supplier)
                .Include(r => r.Warehouse)
                .OrderByDescending(r => r.ReturnDate)
                .ThenByDescending(r => r.Id)
                .AsNoTracking()
                .ToListAsync(ct);

        public Task<PurchaseReturn?> GetByIdAsync(int id, int companyId, CancellationToken ct = default)
            => _db.PurchaseReturns
                .Include(r => r.Supplier)
                .Include(r => r.Warehouse)
                .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId && !r.IsDeleted, ct);

        public Task<PurchaseReturn?> GetByIdWithLinesAsync(int id, int companyId, CancellationToken ct = default)
            => _db.PurchaseReturns
                .Include(r => r.Supplier)
                .Include(r => r.Warehouse)
                .Include(r => r.Lines)
                    .ThenInclude(l => l.Product)
                .Include(r => r.Lines)
                    .ThenInclude(l => l.Unit)
                .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId && !r.IsDeleted, ct);

        public async Task AddAsync(PurchaseReturn ret, CancellationToken ct = default)
            => await _db.PurchaseReturns.AddAsync(ret, ct);

        public void Update(PurchaseReturn ret)
            => _db.PurchaseReturns.Update(ret);

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
        public async Task<Dictionary<(int ProductId, int UnitId), decimal>>
       GetAlreadyReturnedQuantitiesAsync(
           int invoiceId,
           int companyId,
           int? excludeReturnId,
           CancellationToken ct = default)
        {
            // Sum quantities from all non-cancelled, non-deleted returns
            // that are linked to this invoice (excluding the return being
            // created/validated right now to avoid counting itself).
            var query = _db.PurchaseReturnLines
                .Where(l => l.PurchaseReturn.PurchaseInvoiceId == invoiceId
                         && l.PurchaseReturn.CompanyId == companyId
                         && !l.PurchaseReturn.IsDeleted
                         && !l.IsDeleted
                         && l.PurchaseReturn.Status != PurchaseReturnStatus.Cancelled);

            if (excludeReturnId.HasValue)
                query = query.Where(l => l.PurchaseReturn.Id != excludeReturnId.Value);

            var rows = await query
                .GroupBy(l => new { l.ProductId, l.UnitId })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.UnitId,
                    Total = g.Sum(l => l.Quantity)
                })
                .ToListAsync(ct);

            return rows.ToDictionary(
                x => (x.ProductId, x.UnitId),
                x => x.Total);
        }
    }
}
