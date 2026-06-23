using ERPSystem.Domain.Entities.Purchase;

namespace ERPSystem.Domain.Abstractions
{
    public interface IPurchaseReturnRepository
    {
        Task<IReadOnlyList<PurchaseReturn>> GetAllByCompanyAsync(int companyId, CancellationToken ct = default);
        Task<PurchaseReturn?> GetByIdAsync(int id, int companyId, CancellationToken ct = default);
        Task<PurchaseReturn?> GetByIdWithLinesAsync(int id, int companyId, CancellationToken ct = default);
        Task AddAsync(PurchaseReturn ret, CancellationToken ct = default);
        void Update(PurchaseReturn ret);
        Task SaveChangesAsync(CancellationToken ct = default);
        /// <summary>
        /// For the given invoice, sums quantities already returned (across all
        /// non-cancelled, non-deleted returns) grouped by (ProductId, UnitId).
        /// Pass <paramref name="excludeReturnId"/> to skip the return currently
        /// being created/validated (avoids counting itself on re-validation).
        /// </summary>
        Task<Dictionary<(int ProductId, int UnitId), decimal>> GetAlreadyReturnedQuantitiesAsync(
            int invoiceId,
            int companyId,
            int? excludeReturnId,
            CancellationToken ct = default);
    }
}
