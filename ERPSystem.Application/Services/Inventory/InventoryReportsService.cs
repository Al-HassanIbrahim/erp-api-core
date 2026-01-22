using ERPSystem.Application.DTOs.Inventory;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;

namespace ERPSystem.Application.Services.Inventory
{
    public class InventoryReportsService : IInventoryReportsService
    {
        private readonly IInventoryReportsRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public InventoryReportsService(
            IInventoryReportsRepository repository,
            ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyList<StockBalanceDto>> GetStockBalanceAsync(
            int? productId,
            int? warehouseId,
            CancellationToken cancellationToken = default)
        {
            var items = await _repository.GetStockItemsAsync(
                _currentUser.CompanyId,
                productId,
                warehouseId,
                cancellationToken);

            return items.Select(s => new StockBalanceDto
            {
                ProductId = s.ProductId,
                ProductCode = s.Product.Code,
                ProductName = s.Product.Name,
                CategoryName = s.Product.Category?.Name,
                WarehouseId = s.WarehouseId,
                WarehouseName = s.Warehouse.Name,
                UnitName = s.Product.UnitOfMeasure.Name,
                UnitSymbol = s.Product.UnitOfMeasure.Symbol,
                QuantityOnHand = s.QuantityOnHand,
                AverageUnitCost = s.AverageUnitCost
            }).ToList();
        }

        public async Task<IReadOnlyList<StockBalanceDto>> GetWarehouseStockAsync(
            int warehouseId,
            CancellationToken cancellationToken = default)
        {
            return await GetStockBalanceAsync(null, warehouseId, cancellationToken);
        }

        public async Task<IReadOnlyList<StockBalanceDto>> GetProductStockAsync(
            int productId,
            CancellationToken cancellationToken = default)
        {
            return await GetStockBalanceAsync(productId, null, cancellationToken);
        }

        public async Task<IReadOnlyList<InventoryMovementDto>> GetMovementsAsync(
            int productId,
            int? warehouseId,
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default)
        {
            var lines = await _repository.GetMovementLinesAsync(
                _currentUser.CompanyId,
                productId,
                warehouseId,
                fromDate,
                toDate,
                cancellationToken);

            return lines.Select(l => new InventoryMovementDto
            {
                DocumentId = l.InventoryDocumentId,
                DocNumber = l.Document.DocNumber,
                DocDate = l.Document.DocDate,
                DocType = l.Document.DocType.ToString(),
                ProductId = l.ProductId,
                ProductCode = l.Product.Code,
                ProductName = l.Product.Name,
                WarehouseId = l.WarehouseId,
                WarehouseName = l.Warehouse.Name,
                LineType = l.LineType.ToString(),
                Quantity = l.Quantity,
                UnitCost = l.UnitCost
            }).ToList();
        }

        public async Task<IReadOnlyList<LowStockItemDto>> GetLowStockAsync(
            int? warehouseId,
            CancellationToken cancellationToken = default)
        {
            var items = await _repository.GetLowStockItemsAsync(
                _currentUser.CompanyId,
                warehouseId,
                cancellationToken);

            return items.Select(s => new LowStockItemDto
            {
                ProductId = s.ProductId,
                ProductCode = s.Product.Code,
                ProductName = s.Product.Name,
                WarehouseId = s.WarehouseId,
                WarehouseName = s.Warehouse.Name,
                QuantityOnHand = s.QuantityOnHand,
                MinQuantity = s.MinQuantity
            }).ToList();
        }

        public async Task<IReadOnlyList<StockBalanceDto>> GetInventoryValuationAsync(
            int? warehouseId,
            CancellationToken cancellationToken = default)
        {
            return await GetStockBalanceAsync(null, warehouseId, cancellationToken);
        }
    }
}