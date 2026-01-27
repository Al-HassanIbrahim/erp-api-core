using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Application.DTOs.Sales;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Application.Interfaces
{
    public interface ISalesReturnService
    {
        Task<IReadOnlyList<SalesReturnListDto>> GetAllAsync(
            int? customerId = null,
            SalesReturnStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default);
        Task<SalesReturnDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<SalesReturnDto> CreateAsync(CreateSalesReturnRequest request, CancellationToken cancellationToken = default);
        Task<PostReturnResponse> PostAsync(int id, CancellationToken cancellationToken = default);
        Task<SalesReturnDto> CancelAsync(int id, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
