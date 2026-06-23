using ERPSystem.Application.DTOs.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.Interfaces
{

    public interface IPurchaseInvoiceService
    {
        Task<IReadOnlyList<PurchaseInvoiceListDto>> GetAllAsync(CancellationToken ct = default);
        Task<PurchaseInvoiceDto> GetByIdAsync(int id, CancellationToken ct = default);
        Task<PurchaseInvoiceDto> CreateAsync(CreatePurchaseInvoiceDto request, CancellationToken ct = default);
        Task<PurchaseInvoiceDto> UpdateAsync(int id, UpdatePurchaseInvoiceDto request, CancellationToken ct = default);
        Task PostAsync(int id, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
