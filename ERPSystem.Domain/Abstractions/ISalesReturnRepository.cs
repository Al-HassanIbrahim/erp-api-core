using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Domain.Abstractions
{
    public interface ISalesReturnRepository
    {
        Task<SalesReturn?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<SalesReturn?> GetByIdWithLinesAsync(int id, CancellationToken cancellationToken = default);
        Task<List<SalesReturn>> GetAllByCompanyAsync(
            int companyId,
            int? customerId = null,
            SalesReturnStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default);
        Task AddAsync(SalesReturn salesReturn, CancellationToken cancellationToken = default);
        void Update(SalesReturn salesReturn);
        void Delete(SalesReturn salesReturn);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Loads a SalesReturn with all navigations required for PDF generation:
        /// Customer, Warehouse, SalesInvoice (optional), Lines (with Product, Unit).
        /// Returns null if not found or if the record is soft-deleted.
        /// </summary>
        Task<SalesReturn?> GetByIdWithDetailsAsync(int id, int companyId, CancellationToken ct);
    }
}
