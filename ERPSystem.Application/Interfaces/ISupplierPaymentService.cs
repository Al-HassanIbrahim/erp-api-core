using ERPSystem.Application.DTOs.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.Interfaces
{
    public interface ISupplierPaymentService
    {
        Task<IReadOnlyList<SupplierPaymentListDto>> GetAllAsync(CancellationToken ct = default);
        Task<SupplierPaymentDto> GetByIdAsync(int id, CancellationToken ct = default);
        Task<SupplierPaymentDto> CreateAsync(CreateSupplierPaymentDto request, CancellationToken ct = default);
        Task PostAsync(int id, CancellationToken ct = default);
        Task CancelAsync(int id, CancellationToken ct = default);
    }
}
