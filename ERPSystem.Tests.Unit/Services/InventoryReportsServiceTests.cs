using Moq;
using FluentAssertions;
using ERPSystem.Application.Services.Inventory;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Entities.Inventory;
using ERPSystem.Domain.Entities.Products;
using ERPSystem.Application.DTOs.Inventory;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Tests.Unit.Services
{
    public class InventoryReportsServiceTests
    {
        private readonly Mock<IInventoryReportsRepository> _repoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly InventoryReportsService _service;
        private readonly int _companyId = 1;

        public InventoryReportsServiceTests()
        {
            _repoMock = new Mock<IInventoryReportsRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();

            _currentUserMock.Setup(x => x.CompanyId).Returns(_companyId);

            _service = new InventoryReportsService(
                _repoMock.Object,
                _currentUserMock.Object);
        }

        [Fact]
        public async Task GetStockBalanceAsync_ShouldReturnStockBalances()
        {
            // Given
            var ct = CancellationToken.None;
            var stockItems = new List<StockItem>
            {
                new StockItem
                {
                    ProductId = 1,
                    Product = new Product { Code = "P1", Name = "Product 1", UnitOfMeasure = new UnitOfMeasure { Name = "Unit 1", Symbol = "U1" } },
                    WarehouseId = 10,
                    Warehouse = new Warehouse { Name = "Warehouse 1" },
                    QuantityOnHand = 100,
                    AverageUnitCost = 10
                }
            };

            _repoMock.Setup(r => r.GetStockItemsAsync(_companyId, 1, 10, ct))
                .ReturnsAsync(stockItems);

            // When
            var result = await _service.GetStockBalanceAsync(1, 10, ct);

            // Then
            result.Should().HaveCount(1);
            result[0].ProductCode.Should().Be("P1");
            result[0].WarehouseName.Should().Be("Warehouse 1");
        }

        [Fact]
        public async Task GetWarehouseStockAsync_ShouldCallGetStockBalanceAsync()
        {
            // Given
            var ct = CancellationToken.None;
            var warehouseId = 10;
            _repoMock.Setup(r => r.GetStockItemsAsync(_companyId, null, warehouseId, ct))
                .ReturnsAsync(new List<StockItem>());

            // When
            await _service.GetWarehouseStockAsync(warehouseId, ct);

            // Then
            _repoMock.Verify(r => r.GetStockItemsAsync(_companyId, null, warehouseId, ct), Times.Once);
        }

        [Fact]
        public async Task GetProductStockAsync_ShouldCallGetStockBalanceAsync()
        {
            // Given
            var ct = CancellationToken.None;
            var productId = 1;
            _repoMock.Setup(r => r.GetStockItemsAsync(_companyId, productId, null, ct))
                .ReturnsAsync(new List<StockItem>());

            // When
            await _service.GetProductStockAsync(productId, ct);

            // Then
            _repoMock.Verify(r => r.GetStockItemsAsync(_companyId, productId, null, ct), Times.Once);
        }

        [Fact]
        public async Task GetMovementsAsync_ShouldReturnMovements()
        {
            // Given
            var ct = CancellationToken.None;
            var productId = 1;
            var fromDate = DateTime.UtcNow.AddDays(-7);
            var toDate = DateTime.UtcNow;
            var lines = new List<InventoryDocumentLine>
            {
                new InventoryDocumentLine
                {
                    InventoryDocumentId = 100,
                    Document = new InventoryDocument { DocNumber = "DOC1", DocDate = DateTime.UtcNow, DocType = InventoryDocType.In },
                    ProductId = productId,
                    Product = new Product { Code = "P1", Name = "Product 1" },
                    WarehouseId = 10,
                    Warehouse = new Warehouse { Name = "Warehouse 1" },
                    LineType = InventoryLineType.In,
                    Quantity = 50,
                    UnitCost = 10
                }
            };

            _repoMock.Setup(r => r.GetMovementLinesAsync(_companyId, productId, null, fromDate, toDate, ct))
                .ReturnsAsync(lines);

            // When
            var result = await _service.GetMovementsAsync(productId, null, fromDate, toDate, ct);

            // Then
            result.Should().HaveCount(1);
            result[0].DocNumber.Should().Be("DOC1");
            result[0].LineType.Should().Be(InventoryLineType.In.ToString());
        }

        [Fact]
        public async Task GetLowStockAsync_ShouldReturnLowStockItems()
        {
            // Given
            var ct = CancellationToken.None;
            var items = new List<StockItem>
            {
                new StockItem
                {
                    ProductId = 1,
                    Product = new Product { Code = "P1", Name = "Product 1" },
                    WarehouseId = 10,
                    Warehouse = new Warehouse { Name = "Warehouse 1" },
                    QuantityOnHand = 5,
                    MinQuantity = 10
                }
            };

            _repoMock.Setup(r => r.GetLowStockItemsAsync(_companyId, null, ct))
                .ReturnsAsync(items);

            // When
            var result = await _service.GetLowStockAsync(null, ct);

            // Then
            result.Should().HaveCount(1);
            result[0].QuantityOnHand.Should().Be(5);
        }

        [Fact]
        public async Task GetInventoryValuationAsync_ShouldCallGetStockBalanceAsync()
        {
            // Given
            var ct = CancellationToken.None;
            var warehouseId = 10;
            _repoMock.Setup(r => r.GetStockItemsAsync(_companyId, null, warehouseId, ct))
                .ReturnsAsync(new List<StockItem>());

            // When
            await _service.GetInventoryValuationAsync(warehouseId, ct);

            // Then
            _repoMock.Verify(r => r.GetStockItemsAsync(_companyId, null, warehouseId, ct), Times.Once);
        }
    }
}
