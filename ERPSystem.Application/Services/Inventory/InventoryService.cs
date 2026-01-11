using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Application.DTOs.Inventory;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Inventory;
using ERPSystem.Domain.Entities.Products;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Application.Services.Inventory
{
   public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IWarehouseRepository _warehouseRepository;

        public InventoryService(
            IInventoryRepository inventoryRepository,
            IProductRepository productRepository,
            IWarehouseRepository warehouseRepository)
        {
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _warehouseRepository = warehouseRepository;
        }

        private string GenerateDocumentNumber(string prefix)
        {
            // Simple document numbering generator (can be replaced with a proper sequence)
            return $"{prefix}-{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        /// <summary>
        /// Loads a product and ensures it is active.
        /// </summary>
        private async Task<Product> GetValidProductAsync(int productId, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(productId)
                            ?? throw new InvalidOperationException("Product not found.");

            if (!product.IsActive)
                throw new InvalidOperationException("Product is not active.");

            return product;
        }

        /// <summary>
        /// Loads a warehouse, ensures it is active and belongs to the given company/branch.
        /// </summary>
        private async Task<Warehouse> GetValidWarehouseAsync(
            int warehouseId,
            int companyId,
            int? branchId,
            CancellationToken cancellationToken)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId)
                            ?? throw new InvalidOperationException("Warehouse not found.");

            if (!warehouse.IsActive)
                throw new InvalidOperationException("Warehouse is not active.");

            if (warehouse.CompanyId != companyId)
                throw new InvalidOperationException("Warehouse does not belong to the specified company.");

            if (branchId.HasValue && warehouse.BranchId.HasValue && warehouse.BranchId != branchId)
                throw new InvalidOperationException("Warehouse does not belong to the specified branch.");

            return warehouse;
        }

        /// <summary>
        /// Load a stock item from cache or repository; optionally create if missing.
        /// </summary>
        private async Task<StockItem> GetOrCreateStockItemAsync(
            int productId,
            int warehouseId,
            Dictionary<(int, int), StockItem> cache,
            bool createIfMissing,
            CancellationToken cancellationToken)
        {
            var key = (productId, warehouseId);

            if (cache.TryGetValue(key, out var cached))
                return cached;

            var stockItem = await _inventoryRepository.GetStockItemAsync(productId, warehouseId, cancellationToken);

            if (stockItem == null)
            {
                if (!createIfMissing)
                    throw new InvalidOperationException("Stock item does not exist for this product in this warehouse.");

                stockItem = new StockItem
                {
                    WarehouseId = warehouseId,
                    ProductId = productId,
                    QuantityOnHand = 0,
                    AverageUnitCost = 0,
                    LastUpdatedAt = DateTime.UtcNow
                };

                await _inventoryRepository.AddStockItemAsync(stockItem, cancellationToken);
            }

            cache[key] = stockItem;
            return stockItem;
        }

        /// <summary>
        /// Recalculates moving average unit cost for a stock item when new quantity is received.
        /// </summary>
        private void ApplyIncomingCost(StockItem stockItem, decimal qtyIn, decimal unitCostIn)
        {
            if (qtyIn <= 0)
                throw new InvalidOperationException("Incoming quantity must be greater than zero.");
            if (unitCostIn < 0)
                throw new InvalidOperationException("Unit cost cannot be negative.");

            var currentQty = stockItem.QuantityOnHand;
            var currentAvg = stockItem.AverageUnitCost;

            var newQty = currentQty + qtyIn;

            if (currentQty <= 0)
            {
                // First quantity: use incoming cost
                stockItem.AverageUnitCost = unitCostIn;
            }
            else
            {
                var totalValueBefore = currentQty * currentAvg;
                var totalValueIncoming = qtyIn * unitCostIn;
                var newAvg = (totalValueBefore + totalValueIncoming) / newQty;

                stockItem.AverageUnitCost = newAvg;
            }

            stockItem.QuantityOnHand = newQty;
            stockItem.LastUpdatedAt = DateTime.UtcNow;
        }

        // ================== STOCK IN ==================

        public async Task<InventoryDocumentResponse> StockInAsync(
            StockInRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.Lines == null || !request.Lines.Any())
                throw new InvalidOperationException("At least one line is required.");

            var document = new InventoryDocument
            {
                CompanyId = request.CompanyId,
                BranchId = request.BranchId,
                DocDate = request.DocDate,
                DocType = InventoryDocType.In,
                Status = InventoryDocumentStatus.Posted,
                SourceType = request.SourceType,
                SourceId = request.SourceId,
                Notes = request.Notes,
                CreatedByUserId = request.CreatedByUserId,
                PostedByUserId = request.CreatedByUserId,
                PostedAt = DateTime.UtcNow,
                Lines = new List<InventoryDocumentLine>()
            };

            var stockCache = new Dictionary<(int, int), StockItem>();
            var productCache = new Dictionary<int, Product>();
            var warehouseCache = new Dictionary<int, Warehouse>();

            foreach (var line in request.Lines)
            {
                if (line.Quantity <= 0)
                    throw new InvalidOperationException("Quantity must be greater than zero.");
                if (line.UnitCost < 0)
                    throw new InvalidOperationException("Unit cost cannot be negative.");

                if (!productCache.TryGetValue(line.ProductId, out var product))
                {
                    product = await GetValidProductAsync(line.ProductId, cancellationToken);
                    productCache[line.ProductId] = product;
                }

                if (!warehouseCache.TryGetValue(line.WarehouseId, out var warehouse))
                {
                    warehouse = await GetValidWarehouseAsync(line.WarehouseId, request.CompanyId, request.BranchId, cancellationToken);
                    warehouseCache[line.WarehouseId] = warehouse;
                }

                var stockItem = await GetOrCreateStockItemAsync(
                    line.ProductId,
                    line.WarehouseId,
                    stockCache,
                    createIfMissing: true,
                    cancellationToken);

                // Moving average costing
                ApplyIncomingCost(stockItem, line.Quantity, line.UnitCost);

                var docLine = new InventoryDocumentLine
                {
                    ProductId = line.ProductId,
                    WarehouseId = line.WarehouseId,
                    Quantity = line.Quantity,
                    UnitId = line.UnitId,
                    LineType = InventoryLineType.In,
                    UnitCost = line.UnitCost,
                    Notes = line.Notes
                };

                document.Lines.Add(docLine);
            }

            document.DocNumber = GenerateDocumentNumber("IN");

            await _inventoryRepository.AddDocumentAsync(document, cancellationToken);
            await _inventoryRepository.SaveChangesAsync(cancellationToken);

            return new InventoryDocumentResponse
            {
                DocumentId = document.Id,
                DocNumber = document.DocNumber
            };
        }

        // ================== STOCK OUT ==================

        public async Task<InventoryDocumentResponse> StockOutAsync(
            StockOutRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.Lines == null || !request.Lines.Any())
                throw new InvalidOperationException("At least one line is required.");

            var document = new InventoryDocument
            {
                CompanyId = request.CompanyId,
                BranchId = request.BranchId,
                DocDate = request.DocDate,
                DocType = InventoryDocType.Out,
                Status = InventoryDocumentStatus.Posted,
                SourceType = request.SourceType,
                SourceId = request.SourceId,
                Notes = request.Notes,
                CreatedByUserId = request.CreatedByUserId,
                PostedByUserId = request.CreatedByUserId,
                PostedAt = DateTime.UtcNow,
                Lines = new List<InventoryDocumentLine>()
            };

            var stockCache = new Dictionary<(int, int), StockItem>();
            var productCache = new Dictionary<int, Product>();
            var warehouseCache = new Dictionary<int, Warehouse>();

            foreach (var line in request.Lines)
            {
                if (line.Quantity <= 0)
                    throw new InvalidOperationException("Quantity must be greater than zero.");

                if (!productCache.TryGetValue(line.ProductId, out var product))
                {
                    product = await GetValidProductAsync(line.ProductId, cancellationToken);
                    productCache[line.ProductId] = product;
                }

                if (!warehouseCache.TryGetValue(line.WarehouseId, out var warehouse))
                {
                    warehouse = await GetValidWarehouseAsync(line.WarehouseId, request.CompanyId, request.BranchId, cancellationToken);
                    warehouseCache[line.WarehouseId] = warehouse;
                }

                var stockItem = await GetOrCreateStockItemAsync(
                    line.ProductId,
                    line.WarehouseId,
                    stockCache,
                    createIfMissing: false,
                    cancellationToken);

                // Negative stock policy: do not allow negative
                if (stockItem.QuantityOnHand < line.Quantity)
                    throw new InvalidOperationException("Not enough stock.");

                var unitCost = stockItem.AverageUnitCost;

                stockItem.QuantityOnHand -= line.Quantity;
                stockItem.LastUpdatedAt = DateTime.UtcNow;

                var docLine = new InventoryDocumentLine
                {
                    ProductId = line.ProductId,
                    WarehouseId = line.WarehouseId,
                    Quantity = line.Quantity,
                    UnitId = line.UnitId,
                    LineType = InventoryLineType.Out,
                    UnitCost = unitCost,
                    Notes = line.Notes
                };

                document.Lines.Add(docLine);
            }

            document.DocNumber = GenerateDocumentNumber("OUT");

            await _inventoryRepository.AddDocumentAsync(document, cancellationToken);
            await _inventoryRepository.SaveChangesAsync(cancellationToken);

            return new InventoryDocumentResponse
            {
                DocumentId = document.Id,
                DocNumber = document.DocNumber
            };
        }

        // ================== TRANSFER ==================

        public async Task<InventoryDocumentResponse> TransferAsync(
            StockTransferRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.Lines == null || !request.Lines.Any())
                throw new InvalidOperationException("At least one line is required.");

            var document = new InventoryDocument
            {
                CompanyId = request.CompanyId,
                BranchId = request.BranchId,
                DocDate = request.DocDate,
                DocType = InventoryDocType.Transfer,
                Status = InventoryDocumentStatus.Posted,
                SourceType = request.SourceType,
                Notes = request.Notes,
                CreatedByUserId = request.CreatedByUserId,
                PostedByUserId = request.CreatedByUserId,
                PostedAt = DateTime.UtcNow,
                Lines = new List<InventoryDocumentLine>()
            };

            var stockCache = new Dictionary<(int, int), StockItem>();
            var productCache = new Dictionary<int, Product>();
            var warehouseCache = new Dictionary<int, Warehouse>();

            foreach (var line in request.Lines)
            {
                if (line.Quantity <= 0)
                    throw new InvalidOperationException("Quantity must be greater than zero.");

                if (line.FromWarehouseId == line.ToWarehouseId)
                    throw new InvalidOperationException("From and To warehouses must be different.");

                if (!productCache.TryGetValue(line.ProductId, out var product))
                {
                    product = await GetValidProductAsync(line.ProductId, cancellationToken);
                    productCache[line.ProductId] = product;
                }

                if (!warehouseCache.TryGetValue(line.FromWarehouseId, out var fromWarehouse))
                {
                    fromWarehouse = await GetValidWarehouseAsync(line.FromWarehouseId, request.CompanyId, request.BranchId, cancellationToken);
                    warehouseCache[line.FromWarehouseId] = fromWarehouse;
                }

                if (!warehouseCache.TryGetValue(line.ToWarehouseId, out var toWarehouse))
                {
                    toWarehouse = await GetValidWarehouseAsync(line.ToWarehouseId, request.CompanyId, request.BranchId, cancellationToken);
                    warehouseCache[line.ToWarehouseId] = toWarehouse;
                }

                // OUT from source warehouse
                var fromStock = await GetOrCreateStockItemAsync(
                    line.ProductId,
                    line.FromWarehouseId,
                    stockCache,
                    createIfMissing: false,
                    cancellationToken);

                if (fromStock.QuantityOnHand < line.Quantity)
                    throw new InvalidOperationException("Not enough stock in source warehouse.");

                var transferUnitCost = fromStock.AverageUnitCost;

                fromStock.QuantityOnHand -= line.Quantity;
                fromStock.LastUpdatedAt = DateTime.UtcNow;

                var outLine = new InventoryDocumentLine
                {
                    ProductId = line.ProductId,
                    WarehouseId = line.FromWarehouseId,
                    Quantity = line.Quantity,
                    UnitId = line.UnitId,
                    LineType = InventoryLineType.Out,
                    UnitCost = transferUnitCost,
                    Notes = line.Notes
                };
                document.Lines.Add(outLine);

                // IN to destination warehouse
                var toStock = await GetOrCreateStockItemAsync(
                    line.ProductId,
                    line.ToWarehouseId,
                    stockCache,
                    createIfMissing: true,
                    cancellationToken);

                ApplyIncomingCost(toStock, line.Quantity, transferUnitCost);

                var inLine = new InventoryDocumentLine
                {
                    ProductId = line.ProductId,
                    WarehouseId = line.ToWarehouseId,
                    Quantity = line.Quantity,
                    UnitId = line.UnitId,
                    LineType = InventoryLineType.In,
                    UnitCost = transferUnitCost,
                    Notes = line.Notes
                };
                document.Lines.Add(inLine);
            }

            document.DocNumber = GenerateDocumentNumber("TRF");

            await _inventoryRepository.AddDocumentAsync(document, cancellationToken);
            await _inventoryRepository.SaveChangesAsync(cancellationToken);

            return new InventoryDocumentResponse
            {
                DocumentId = document.Id,
                DocNumber = document.DocNumber
            };
        }

        // ================== OPENING BALANCE ==================

        public async Task<InventoryDocumentResponse> OpeningBalanceAsync(
            OpeningBalanceRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.Lines == null || !request.Lines.Any())
                throw new InvalidOperationException("At least one line is required.");

            var document = new InventoryDocument
            {
                CompanyId = request.CompanyId,
                BranchId = request.BranchId,
                DocDate = request.DocDate,
                DocType = InventoryDocType.Opening,
                Status = InventoryDocumentStatus.Posted,
                SourceType = "Opening",
                Notes = request.Notes,
                CreatedByUserId = request.CreatedByUserId,
                PostedByUserId = request.CreatedByUserId,
                PostedAt = DateTime.UtcNow,
                Lines = new List<InventoryDocumentLine>()
            };

            var stockCache = new Dictionary<(int, int), StockItem>();
            var productCache = new Dictionary<int, Product>();
            var warehouseCache = new Dictionary<int, Warehouse>();

            foreach (var line in request.Lines)
            {
                if (line.Quantity < 0)
                    throw new InvalidOperationException("Opening quantity cannot be negative.");
                if (line.Quantity == 0)
                    continue;
                if (line.UnitCost < 0)
                    throw new InvalidOperationException("Opening unit cost cannot be negative.");

                if (!productCache.TryGetValue(line.ProductId, out var product))
                {
                    product = await GetValidProductAsync(line.ProductId, cancellationToken);
                    productCache[line.ProductId] = product;
                }

                if (!warehouseCache.TryGetValue(line.WarehouseId, out var warehouse))
                {
                    warehouse = await GetValidWarehouseAsync(line.WarehouseId, request.CompanyId, request.BranchId, cancellationToken);
                    warehouseCache[line.WarehouseId] = warehouse;
                }

                var stockItem = await GetOrCreateStockItemAsync(
                    line.ProductId,
                    line.WarehouseId,
                    stockCache,
                    createIfMissing: true,
                    cancellationToken);

                ApplyIncomingCost(stockItem, line.Quantity, line.UnitCost);

                var docLine = new InventoryDocumentLine
                {
                    ProductId = line.ProductId,
                    WarehouseId = line.WarehouseId,
                    Quantity = line.Quantity,
                    UnitId = line.UnitId,
                    LineType = InventoryLineType.In,
                    UnitCost = line.UnitCost,
                    Notes = line.Notes
                };

                document.Lines.Add(docLine);
            }

            document.DocNumber = GenerateDocumentNumber("OPEN");

            await _inventoryRepository.AddDocumentAsync(document, cancellationToken);
            await _inventoryRepository.SaveChangesAsync(cancellationToken);

            return new InventoryDocumentResponse
            {
                DocumentId = document.Id,
                DocNumber = document.DocNumber
            };
        }

        // ================== ADJUSTMENT ==================

        public async Task<InventoryDocumentResponse> AdjustmentAsync(
            StockAdjustmentRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.Lines == null || !request.Lines.Any())
                throw new InvalidOperationException("At least one line is required.");

            var document = new InventoryDocument
            {
                CompanyId = request.CompanyId,
                BranchId = request.BranchId,
                DocDate = request.DocDate,
                DocType = InventoryDocType.Adjustment,
                Status = InventoryDocumentStatus.Posted,
                SourceType = "Adjustment",
                Notes = request.Notes,
                CreatedByUserId = request.CreatedByUserId,
                PostedByUserId = request.CreatedByUserId,
                PostedAt = DateTime.UtcNow,
                Lines = new List<InventoryDocumentLine>()
            };

            var stockCache = new Dictionary<(int, int), StockItem>();
            var productCache = new Dictionary<int, Product>();
            var warehouseCache = new Dictionary<int, Warehouse>();

            foreach (var line in request.Lines)
            {
                if (line.ActualQuantity < 0)
                    throw new InvalidOperationException("Actual quantity cannot be negative.");

                if (!productCache.TryGetValue(line.ProductId, out var product))
                {
                    product = await GetValidProductAsync(line.ProductId, cancellationToken);
                    productCache[line.ProductId] = product;
                }

                if (!warehouseCache.TryGetValue(line.WarehouseId, out var warehouse))
                {
                    warehouse = await GetValidWarehouseAsync(line.WarehouseId, request.CompanyId, request.BranchId, cancellationToken);
                    warehouseCache[line.WarehouseId] = warehouse;
                }

                var key = (line.ProductId, line.WarehouseId);

                if (!stockCache.TryGetValue(key, out var stockItem))
                {
                    stockItem = await _inventoryRepository.GetStockItemAsync(line.ProductId, line.WarehouseId, cancellationToken);
                    stockCache[key] = stockItem!;
                }

                var currentQty = stockItem?.QuantityOnHand ?? 0m;
                var diff = line.ActualQuantity - currentQty;

                if (diff == 0)
                    continue;

                if (diff > 0)
                {
                    // Positive adjustment (gain): we treat as stock in with cost
                    if (stockItem == null)
                    {
                        stockItem = new StockItem
                        {
                            ProductId = line.ProductId,
                            WarehouseId = line.WarehouseId,
                            QuantityOnHand = 0,
                            AverageUnitCost = 0,
                            LastUpdatedAt = DateTime.UtcNow
                        };

                        await _inventoryRepository.AddStockItemAsync(stockItem, cancellationToken);
                        stockCache[key] = stockItem;
                    }

                    var unitCost = line.UnitCost ?? stockItem.AverageUnitCost;
                    if (unitCost < 0)
                        throw new InvalidOperationException("Unit cost cannot be negative.");

                    ApplyIncomingCost(stockItem, diff, unitCost);

                    var inLine = new InventoryDocumentLine
                    {
                        ProductId = line.ProductId,
                        WarehouseId = line.WarehouseId,
                        Quantity = diff,
                        UnitId = line.UnitId,
                        LineType = InventoryLineType.In,
                        UnitCost = unitCost,
                        Notes = line.Notes
                    };
                    document.Lines.Add(inLine);
                }
                else
                {
                    // Negative adjustment (loss): treat as stock out using current average cost
                    var outQty = Math.Abs(diff);

                    if (stockItem == null)
                        throw new InvalidOperationException("Cannot adjust negative stock when no stock item exists.");

                    var unitCost = stockItem.AverageUnitCost;

                    stockItem.QuantityOnHand -= outQty;
                    stockItem.LastUpdatedAt = DateTime.UtcNow;

                    var outLine = new InventoryDocumentLine
                    {
                        ProductId = line.ProductId,
                        WarehouseId = line.WarehouseId,
                        Quantity = outQty,
                        UnitId = line.UnitId,
                        LineType = InventoryLineType.Out,
                        UnitCost = unitCost,
                        Notes = line.Notes
                    };
                    document.Lines.Add(outLine);
                }
            }

            document.DocNumber = GenerateDocumentNumber("ADJ");

            await _inventoryRepository.AddDocumentAsync(document, cancellationToken);
            await _inventoryRepository.SaveChangesAsync(cancellationToken);

            return new InventoryDocumentResponse
            {
                DocumentId = document.Id,
                DocNumber = document.DocNumber
            };
        }
    }
}
