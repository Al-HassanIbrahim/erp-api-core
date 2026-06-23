using ERPSystem.Domain.Entities.Purchase;

namespace ERPSystem.Domain.Abstractions
{
    public interface IPurchaseInvoiceRepository
    {
        Task<IReadOnlyList<PurchaseInvoice>> GetAllByCompanyAsync(int companyId, CancellationToken ct = default);

        /// <summary>Light fetch — no line items; used for state-guard checks.</summary>
        Task<PurchaseInvoice?> GetByIdAsync(int id, int companyId, CancellationToken ct = default);

        /// <summary>Full fetch including Lines + navigation props; used for posting and mapping.</summary>
        Task<PurchaseInvoice?> GetByIdWithLinesAsync(int id, int companyId, CancellationToken ct = default);

        Task AddAsync(PurchaseInvoice invoice, CancellationToken ct = default);
        void Update(PurchaseInvoice invoice);
        Task SaveChangesAsync(CancellationToken ct = default);
    }   
}