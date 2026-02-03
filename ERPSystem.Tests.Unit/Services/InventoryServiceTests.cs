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
    public class InventoryServiceTests
    {
        private readonly Mock<IInventoryRepository> _repoMock;
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly Mock<IWarehouseRepository> _warehouseRepoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly InventoryService _service;
        private readonly int _companyId = 1;

        public InventoryServiceTests()
        {
            _repoMock = new Mock<IInventoryRepository>();
            _productRepoMock = new Mock<IProductRepository>();
            _warehouseRepoMock = new Mock<IWarehouseRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();

            _currentUserMock.Setup(x => x.CompanyId).Returns(_companyId);

            _service = new InventoryService(
                _repoMock.Object,
                _productRepoMock.Object,
                _warehouseRepoMock.Object,
                _currentUserMock.Object);
        }

        [Fact]
        public async Task StockInAsync_ShouldIncreaseStockAndCalculateAverageCost()
        {
            // Given
            var ct = CancellationToken.None;
            var productId = 1;
            var warehouseId = 1;
            var request = new StockInRequest
            {
                Lines = new List<StockInLineRequest>
                {
                    new StockInLineRequest 
                    { 
                        ProductId = productId, 
                        WarehouseId = warehouseId, 
                        Quantity = 10, 
                        UnitCost = 100 
                    }
                }
            };

            var product = new Product { Id = productId, CompanyId = _companyId, IsActive = true };
            var warehouse = new Warehouse { Id = warehouseId, CompanyId = _companyId, IsActive = true };
            var stockItem = new StockItem 
            { 
                ProductId = productId, 
                WarehouseId = warehouseId, 
                QuantityOnHand = 5, 
                AverageUnitCost = 50 
            };

            _productRepoMock.Setup(r => r.GetByIdAsync(productId, _companyId))
                .ReturnsAsync(product);
            _warehouseRepoMock.Setup(r => r.GetByIdAsync(warehouseId, _companyId))
                .ReturnsAsync(warehouse);
            _repoMock.Setup(r => r.GetStockItemAsync(productId, warehouseId, ct))
                .ReturnsAsync(stockItem);

            // When
            await _service.StockInAsync(request, ct);

            // Then
            // (5 * 50 + 10 * 100) / 15 = (250 + 1000) / 15 = 1250 / 15 = 83.333
            stockItem.QuantityOnHand.Should().Be(15);
            stockItem.AverageUnitCost.Should().BeApproximately(83.333m, 0.001m);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task StockOutAsync_ShouldThrowException_WhenInsufficientStock()
        {
            // Given
            var ct = CancellationToken.None;
            var productId = 1;
            var warehouseId = 1;
            var request = new StockOutRequest
            {
                Lines = new List<StockOutLineRequest>
                {
                    new StockOutLineRequest 
                    { 
                        ProductId = productId, 
                        WarehouseId = warehouseId, 
                        Quantity = 20 
                    }
                }
            };

            var product = new Product { Id = productId, CompanyId = _companyId, IsActive = true };
            var warehouse = new Warehouse { Id = warehouseId, CompanyId = _companyId, IsActive = true };
            var stockItem = new StockItem { QuantityOnHand = 10 };

            _productRepoMock.Setup(r => r.GetByIdAsync(productId, _companyId))
                .ReturnsAsync(product);
            _warehouseRepoMock.Setup(r => r.GetByIdAsync(warehouseId, _companyId))
                .ReturnsAsync(warehouse);
            _repoMock.Setup(r => r.GetStockItemAsync(productId, warehouseId, ct))
                .ReturnsAsync(stockItem);

            // When
            var act = () => _service.StockOutAsync(request, ct);

            // Then
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Not enough stock*");
        }
    }
}
