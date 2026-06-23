using ERPSystem.Domain.Entities.Purchase;

namespace ERPSystem.Domain.Abstractions
{
    public interface ISupplierPaymentRepository
    {
        Task<IReadOnlyList<SupplierPayment>> GetAllByCompanyAsync(int companyId, CancellationToken ct = default);
        Task<SupplierPayment?> GetByIdAsync(int id, int companyId, CancellationToken ct = default);

        /// <summary>Includes Allocations + each Allocation.PurchaseInvoice nav prop.</summary>
        Task<SupplierPayment?> GetByIdWithAllocationsAsync(int id, int companyId, CancellationToken ct = default);

        Task AddAsync(SupplierPayment payment, CancellationToken ct = default);
        void Update(SupplierPayment payment);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}