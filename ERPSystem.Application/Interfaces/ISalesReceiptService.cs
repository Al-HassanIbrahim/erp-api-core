using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Application.DTOs.Sales;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Application.Interfaces
{
    public interface ISalesReceiptService
    {
        Task<IReadOnlyList<SalesReceiptListDto>> GetAllAsync(
            int? customerId = null,
            SalesReceiptStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default);
        Task<SalesReceiptDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<SalesReceiptDto> CreateAsync(CreateSalesReceiptRequest request, CancellationToken cancellationToken = default);
        Task<SalesReceiptDto> PostAsync(int id, CancellationToken cancellationToken = default);
        Task<SalesReceiptDto> CancelAsync(int id, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
