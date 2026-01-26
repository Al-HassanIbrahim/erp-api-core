using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Application.DTOs.Sales;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Application.Interfaces
{
    public interface ISalesInvoiceService
    {
        Task<IReadOnlyList<SalesInvoiceListDto>> GetAllAsync(
            int? customerId = null,
            SalesInvoiceStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default);
        Task<SalesInvoiceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<SalesInvoiceDto> CreateAsync(CreateSalesInvoiceRequest request, CancellationToken cancellationToken = default);
        Task<SalesInvoiceDto> UpdateAsync(int id, UpdateSalesInvoiceRequest request, CancellationToken cancellationToken = default);
        Task<SalesInvoiceDto> PostAsync(int id, CancellationToken cancellationToken = default);
        Task<SalesInvoiceDto> CancelAsync(int id, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
