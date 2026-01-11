using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Entities.Inventory;

namespace ERPSystem.Domain.Abstractions
{ 
    public interface IInventoryReportsRepository
    {
        /// <summary>
        /// Returns stock items filtered by optional product and warehouse.
        /// If both parameters are null, returns all stock items.
        /// </summary>
        Task<List<StockItem>> GetStockItemsAsync(
            int? productId,
            int? warehouseId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns stock items with quantity below or equal to MinQuantity.
        /// If warehouseId is provided, filters by that warehouse.
        /// </summary>
        Task<List<StockItem>> GetLowStockItemsAsync(
            int? warehouseId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns document lines for a given product and date range,
        /// optionally filtered by warehouse. The related document, product,
        /// and warehouse should be loaded (via includes) in the implementation.
        /// </summary>
        Task<List<InventoryDocumentLine>> GetMovementLinesAsync(
            int productId,
            int? warehouseId,
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default);
    }
}


