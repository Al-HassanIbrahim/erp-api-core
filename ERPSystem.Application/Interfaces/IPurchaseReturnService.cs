using ERPSystem.Application.DTOs.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.Interfaces
{
    public interface IPurchaseReturnService
    {
        Task<IReadOnlyList<PurchaseReturnListDto>> GetAllAsync(CancellationToken ct = default);
        Task<PurchaseReturnDto> GetByIdAsync(int id, CancellationToken ct = default);
        Task<PurchaseReturnDto> CreateAsync(CreatePurchaseReturnDto request, CancellationToken ct = default);
        Task PostAsync(int id, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
