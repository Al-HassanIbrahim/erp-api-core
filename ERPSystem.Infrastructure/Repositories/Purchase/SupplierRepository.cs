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
    public sealed class SupplierRepository : ISupplierRepository
    {
        private readonly AppDbContext _db;
        public SupplierRepository(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<Supplier>> GetAllByCompanyAsync(
            int companyId, CancellationToken ct = default)
            => await _db.Suppliers
                .Where(s => s.CompanyId == companyId && !s.IsDeleted)
                .OrderBy(s => s.Name)
                .AsNoTracking()
                .ToListAsync(ct);

        public Task<Supplier?> GetByIdAsync(int id, int companyId, CancellationToken ct = default)
            => _db.Suppliers
                .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == companyId && !s.IsDeleted, ct);

        public Task<bool> CodeExistsAsync(
            int companyId, string code, int? excludeId, CancellationToken ct = default)
            => _db.Suppliers.AnyAsync(
                s => s.CompanyId == companyId
                  && s.Code == code
                  && !s.IsDeleted
                  && (excludeId == null || s.Id != excludeId), ct);

        public async Task AddAsync(Supplier supplier, CancellationToken ct = default)
            => await _db.Suppliers.AddAsync(supplier, ct);

        public void Update(Supplier supplier)
            => _db.Suppliers.Update(supplier);

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
        public async Task<bool> HasActivePurchasingDocumentsAsync(int supplierId, int companyId, CancellationToken ct = default)
        {
            // Any invoice that is Posted (regardless of payment status) — cannot delete supplier
            var hasPostedInvoices = await _db.PurchaseInvoices.AnyAsync(
                i => i.SupplierId == supplierId
                  && i.CompanyId == companyId
                  && !i.IsDeleted
                  && i.Status == PurchaseInvoiceStatus.Posted, ct);

            if (hasPostedInvoices) return true;

            // Any Posted return
            var hasPostedReturns = await _db.PurchaseReturns.AnyAsync(
                r => r.SupplierId == supplierId
                  && r.CompanyId == companyId
                  && !r.IsDeleted
                  && r.Status == PurchaseReturnStatus.Posted, ct);

            if (hasPostedReturns) return true;

            // Any Posted payment
            var hasPostedPayments = await _db.SupplierPayments.AnyAsync(
                p => p.SupplierId == supplierId
                  && p.CompanyId == companyId
                  && !p.IsDeleted
                  && p.Status == SupplierPaymentStatus.Posted, ct);

            return hasPostedPayments;
        }
    }


}
