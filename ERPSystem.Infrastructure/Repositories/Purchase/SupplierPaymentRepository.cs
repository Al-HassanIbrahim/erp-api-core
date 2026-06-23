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
    public sealed class SupplierPaymentRepository : ISupplierPaymentRepository
    {
        private readonly AppDbContext _db;
        public SupplierPaymentRepository(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<SupplierPayment>> GetAllByCompanyAsync(
            int companyId, CancellationToken ct = default)
            => await _db.SupplierPayments
                .Where(p => p.CompanyId == companyId && !p.IsDeleted)
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.Id)
                .AsNoTracking()
                .ToListAsync(ct);

        public Task<SupplierPayment?> GetByIdAsync(int id, int companyId, CancellationToken ct = default)
            => _db.SupplierPayments
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId && !p.IsDeleted, ct);

        public Task<SupplierPayment?> GetByIdWithAllocationsAsync(int id, int companyId, CancellationToken ct = default)
            => _db.SupplierPayments
                .Include(p => p.Supplier)
                .Include(p => p.Allocations)
                    .ThenInclude(a => a.PurchaseInvoice)
                .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId && !p.IsDeleted, ct);

        public async Task AddAsync(SupplierPayment payment, CancellationToken ct = default)
            => await _db.SupplierPayments.AddAsync(payment, ct);

        public void Update(SupplierPayment payment)
            => _db.SupplierPayments.Update(payment);

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
