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
    public class InventoryReportsRepository : IInventoryReportsRepository
    {
        private readonly AppDbContext _dbContext;

        public InventoryReportsRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<StockItem>> GetStockItemsAsync(
            int? productId,
            int? warehouseId,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.StockItems
                .AsNoTracking()
                .Include(s => s.Product)
                    .ThenInclude(p => p.Category)
                .Include(s => s.Product)
                    .ThenInclude(p => p.UnitOfMeasure)
                .Include(s => s.Warehouse)
                .AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(s => s.ProductId == productId.Value);
            }

            if (warehouseId.HasValue)
            {
                query = query.Where(s => s.WarehouseId == warehouseId.Value);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<List<StockItem>> GetLowStockItemsAsync(
            int? warehouseId,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.StockItems
                .AsNoTracking()
                .Include(s => s.Product)
                    .ThenInclude(p => p.Category)
                .Include(s => s.Product)
                    .ThenInclude(p => p.UnitOfMeasure)
                .Include(s => s.Warehouse)
                .Where(s => s.MinQuantity.HasValue && s.QuantityOnHand <= s.MinQuantity.Value)
                .AsQueryable();

            if (warehouseId.HasValue)
            {
                query = query.Where(s => s.WarehouseId == warehouseId.Value);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<List<InventoryDocumentLine>> GetMovementLinesAsync(
            int productId,
            int? warehouseId,
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.InventoryDocumentLines
                .AsNoTracking()
                .Include(l => l.Document)
                .Include(l => l.Product)
                .Include(l => l.Warehouse)
                .Where(l => l.ProductId == productId &&
                            l.Document.DocDate >= fromDate &&
                            l.Document.DocDate <= toDate)
                .AsQueryable();

            if (warehouseId.HasValue)
            {
                query = query.Where(l => l.WarehouseId == warehouseId.Value);
            }

            // TODO filter only Posted documents
            // e.g. l.Document.Status == InventoryDocumentStatus.Posted

            return await query
                .OrderBy(l => l.Document.DocDate)
                .ThenBy(l => l.InventoryDocumentId)
                .ThenBy(l => l.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
