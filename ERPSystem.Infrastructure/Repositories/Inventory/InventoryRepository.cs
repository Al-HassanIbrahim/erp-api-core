using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Inventory;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Inventory
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _dbContext;

        public InventoryRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddDocumentAsync(InventoryDocument document, CancellationToken cancellationToken = default)
        {
            await _dbContext.InventoryDocuments.AddAsync(document, cancellationToken);
        }

        public async Task<StockItem?> GetStockItemAsync(int productId, int warehouseId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.StockItems
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId, cancellationToken);
        }

        public async Task AddStockItemAsync(StockItem stockItem, CancellationToken cancellationToken = default)
        {
            await _dbContext.StockItems.AddAsync(stockItem, cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
    //    public async Task<string> GenerateDocumentNumberAsync(string prefix, int companyId, CancellationToken cancellationToken = default)
    //    {
    //        var nextVal = await _dbContext.Database
    //            .SqlQueryRaw<int>("SELECT NEXT VALUE FOR InventoryDocumentSeq")
    //            .SingleAsync(cancellationToken);

    //        // ex: IN-202606-C1-00001 | OUT-202606-C1-00002
    //        return $"{prefix}-{DateTime.UtcNow:yyyyMM}-C{companyId}-{nextVal:D5}";
    //    }
    }
}
