using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Domain.Abstractions
{
    public interface ISalesDeliveryRepository
    {
        Task<SalesDelivery?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<SalesDelivery?> GetByIdWithLinesAsync(int id, CancellationToken cancellationToken = default);
        Task<List<SalesDelivery>> GetAllByCompanyAsync(
            int companyId,
            int? invoiceId = null,
            SalesDeliveryStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default);
        Task AddAsync(SalesDelivery delivery, CancellationToken cancellationToken = default);
        void Update(SalesDelivery delivery);
        void Delete(SalesDelivery delivery);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Loads a SalesDelivery with all navigations required for PDF generation:
        /// Customer, SalesInvoice, Warehouse, Lines (with Product, Unit, SalesInvoiceLine).
        /// Returns null if not found or if the record is soft-deleted.
        /// </summary>
        Task<SalesDelivery?> GetByIdWithDetailsAsync(int id, int companyId, CancellationToken ct);
    }
}
