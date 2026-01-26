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
        Task<string> GenerateDeliveryNumberAsync(int companyId, CancellationToken cancellationToken = default);
        Task AddAsync(SalesDelivery delivery, CancellationToken cancellationToken = default);
        void Update(SalesDelivery delivery);
        void Delete(SalesDelivery delivery);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
