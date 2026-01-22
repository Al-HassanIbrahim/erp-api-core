using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Inventory;
using ERPSystem.Domain.Enums;
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
            int companyId,
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
                .Where(s => s.CompanyId == companyId && !s.IsDeleted);

            if (productId.HasValue)
                query = query.Where(s => s.ProductId == productId.Value);

            if (warehouseId.HasValue)
                query = query.Where(s => s.WarehouseId == warehouseId.Value);

            return await query
                .OrderBy(s => s.Product.Name)
                .ThenBy(s => s.Warehouse.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<StockItem>> GetLowStockItemsAsync(
            int companyId,
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
                .Where(s => s.CompanyId == companyId
                         && !s.IsDeleted
                         && s.MinQuantity.HasValue
                         && s.QuantityOnHand <= s.MinQuantity.Value);

            if (warehouseId.HasValue)
                query = query.Where(s => s.WarehouseId == warehouseId.Value);

            return await query
                .OrderBy(s => s.QuantityOnHand)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<InventoryDocumentLine>> GetMovementLinesAsync(
            int companyId,
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
                .Where(l => l.Document.CompanyId == companyId
                         && !l.Document.IsDeleted
                         && l.Document.Status == InventoryDocumentStatus.Posted
                         && l.ProductId == productId
                         && l.Document.DocDate >= fromDate
                         && l.Document.DocDate <= toDate);

            if (warehouseId.HasValue)
                query = query.Where(l => l.WarehouseId == warehouseId.Value);

            return await query
                .OrderBy(l => l.Document.DocDate)
                .ThenBy(l => l.InventoryDocumentId)
                .ThenBy(l => l.Id)
                .ToListAsync(cancellationToken);
        }
    }
}