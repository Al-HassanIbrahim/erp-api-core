using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Application.DTOs.Sales;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Application.Interfaces
{
    public interface ISalesDeliveryService
    {
        Task<IReadOnlyList<SalesDeliveryListDto>> GetAllAsync(
            int? invoiceId = null,
            SalesDeliveryStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default);
        Task<SalesDeliveryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<SalesDeliveryDto> CreateAsync(CreateSalesDeliveryRequest request, CancellationToken cancellationToken = default);
        Task<PostDeliveryResponse> PostAsync(int id, CancellationToken cancellationToken = default);
        Task<SalesDeliveryDto> CancelAsync(int id, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
