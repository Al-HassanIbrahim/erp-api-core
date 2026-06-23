using ERPSystem.Domain.Entities.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Abstractions
{
    public interface ISupplierRepository
    {
        Task<IReadOnlyList<Supplier>> GetAllByCompanyAsync(int companyId, CancellationToken ct = default);
        Task<Supplier?> GetByIdAsync(int id, int companyId, CancellationToken ct = default);
        Task<bool> CodeExistsAsync(int companyId, string code, int? excludeId, CancellationToken ct = default);
        Task AddAsync(Supplier supplier, CancellationToken ct = default);
        void Update(Supplier supplier);
        Task SaveChangesAsync(CancellationToken ct = default);
        /// <summary>
        /// Returns true if the supplier has any Posted/PartiallyPaid invoices,
        /// any Posted returns, or any Posted payments — i.e. it cannot be safely
        /// soft-deleted without losing referential accounting integrity.
        /// </summary>
        Task<bool> HasActivePurchasingDocumentsAsync(int supplierId, int companyId, CancellationToken ct = default);
    }
}

