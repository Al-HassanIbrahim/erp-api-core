using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Application.DTOs.Inventory;

namespace ERPSystem.Application.Interfaces
{
   public interface IInventoryReportsService
    {
        Task<IReadOnlyList<StockBalanceDto>> GetStockBalanceAsync(
            int? productId,
            int? warehouseId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<StockBalanceDto>> GetWarehouseStockAsync(
            int warehouseId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<StockBalanceDto>> GetProductStockAsync(
            int productId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<InventoryMovementDto>> GetMovementsAsync(
            int productId,
            int? warehouseId,
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<LowStockItemDto>> GetLowStockAsync(
            int? warehouseId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<StockBalanceDto>> GetInventoryValuationAsync(
            int? warehouseId,
            CancellationToken cancellationToken = default);
    }
}
