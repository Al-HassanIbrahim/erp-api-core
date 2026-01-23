using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Entities.Inventory;

namespace ERPSystem.Domain.Abstractions
{
    public interface IInventoryRepository
    {
        Task AddDocumentAsync(InventoryDocument document, CancellationToken cancellationToken = default);

        Task<StockItem?> GetStockItemAsync(int productId, int warehouseId, CancellationToken cancellationToken = default);
        Task AddStockItemAsync(StockItem stockItem, CancellationToken cancellationToken = default);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
