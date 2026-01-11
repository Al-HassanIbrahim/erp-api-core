using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Application.DTOs.Inventory;
namespace ERPSystem.Application.Interfaces
{
    public interface IInventoryService
    {
        Task<InventoryDocumentResponse> StockInAsync(StockInRequest request, CancellationToken cancellationToken = default);
        Task<InventoryDocumentResponse> StockOutAsync(StockOutRequest request, CancellationToken cancellationToken = default);
        Task<InventoryDocumentResponse> TransferAsync(StockTransferRequest request, CancellationToken cancellationToken = default);
        Task<InventoryDocumentResponse> OpeningBalanceAsync(OpeningBalanceRequest request, CancellationToken cancellationToken = default);
        Task<InventoryDocumentResponse> AdjustmentAsync(StockAdjustmentRequest request, CancellationToken cancellationToken = default);
    }
}
