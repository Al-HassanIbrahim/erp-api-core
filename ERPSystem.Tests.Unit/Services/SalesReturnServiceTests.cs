using Moq;
using FluentAssertions;
using ERPSystem.Application.Services.Sales;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Application.DTOs.Sales;
using ERPSystem.Application.Exceptions;
using ERPSystem.Domain.Enums;
using ERPSystem.Domain.Entities.Products;
using ERPSystem.Domain.Entities.Inventory;
using ERPSystem.Application.DTOs.Inventory;

namespace ERPSystem.Tests.Unit.Services
{
    public class SalesReturnServiceTests
    {
        private readonly Mock<ISalesReturnRepository> _repoMock;
        private readonly Mock<ICustomerRepository> _customerRepoMock;
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly Mock<IWarehouseRepository> _warehouseRepoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly Mock<IModuleAccessService> _moduleAccessMock;
        private readonly Mock<IInventoryService> _inventoryServiceMock;
        private readonly SalesReturnService _service;
        private readonly int _companyId = 1;

        public SalesReturnServiceTests()
        {
            _repoMock = new Mock<ISalesReturnRepository>();
            _customerRepoMock = new Mock<ICustomerRepository>();
            _productRepoMock = new Mock<IProductRepository>();
            _warehouseRepoMock = new Mock<IWarehouseRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();
            _moduleAccessMock = new Mock<IModuleAccessService>();
            _inventoryServiceMock = new Mock<IInventoryService>();

            _currentUserMock.Setup(x => x.CompanyId).Returns(_companyId);

            _service = new SalesReturnService(
                _repoMock.Object,
                _customerRepoMock.Object,
                _productRepoMock.Object,
                _warehouseRepoMock.Object,
                _currentUserMock.Object,
                _moduleAccessMock.Object,
                _inventoryServiceMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnReturns()
        {
            // Given
            var ct = CancellationToken.None;
            var returns = new List<SalesReturn>
            {
                new SalesReturn { Id = 1, ReturnNumber = "R1", Customer = new Customer { Name = "C1" }, Warehouse = new Warehouse { Name = "W1" }, Status = SalesReturnStatus.Draft, CompanyId = _companyId },
                new SalesReturn { Id = 2, ReturnNumber = "R2", Customer = new Customer { Name = "C2" }, Warehouse = new Warehouse { Name = "W2" }, Status = SalesReturnStatus.Posted, CompanyId = _companyId }
            };

            _repoMock.Setup(r => r.GetAllByCompanyAsync(_companyId, null, null, null, null, ct))
                .ReturnsAsync(returns);

            // When
            var result = await _service.GetAllAsync(cancellationToken: ct);

            // Then
            result.Should().HaveCount(2);
            _moduleAccessMock.Verify(m => m.EnsureSalesEnabledAsync(ct), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnReturn_WhenExists()
        {
            // Given
            var id = 1;
            var ct = CancellationToken.None;
            var salesReturn = new SalesReturn 
            { 
                Id = id, 
                ReturnNumber = "R1", 
                Customer = new Customer { Name = "C1" }, 
                Warehouse = new Warehouse { Name = "W1" },
                CompanyId = _companyId,
                Lines = new List<SalesReturnLine>()
            };

            _repoMock.Setup(r => r.GetByIdWithLinesAsync(id, ct)).ReturnsAsync(salesReturn);

            // When
            var result = await _service.GetByIdAsync(id, ct);

            // Then
            result.Should().NotBeNull();
            result!.ReturnNumber.Should().Be("R1");
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnCreatedReturn()
        {
            // Given
            var ct = CancellationToken.None;
            var customerId = 5;
            var warehouseId = 20;
            var productId = 100;
            var customer = new Customer { Id = customerId, CompanyId = _companyId, Name = "C1" };
            var warehouse = new Warehouse { Id = warehouseId, CompanyId = _companyId, IsActive = true, Name = "W1" };
            var product = new Product { Id = productId, CompanyId = _companyId, Name = "P1", Code = "PC1" };
            
            var request = new CreateSalesReturnRequest
            {
                CustomerId = customerId,
                WarehouseId = warehouseId,
                ReturnDate = DateTime.UtcNow,
                Lines = new List<CreateSalesReturnLineRequest>
                {
                    new CreateSalesReturnLineRequest { ProductId = productId, Quantity = 5, UnitPrice = 10, TaxPercent = 10, UnitId = 1 }
                }
            };

            _customerRepoMock.Setup(r => r.GetByIdAsync(customerId, ct)).ReturnsAsync(customer);
            _warehouseRepoMock.Setup(r => r.GetByIdAsync(warehouseId)).ReturnsAsync(warehouse);
            _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
            _repoMock.Setup(r => r.GenerateReturnNumberAsync(_companyId, ct)).ReturnsAsync("RET-001");
            
            var createdReturn = new SalesReturn 
            { 
                Id = 1, 
                ReturnNumber = "RET-001", 
                Customer = customer, 
                Warehouse = warehouse,
                Status = SalesReturnStatus.Draft,
                Lines = new List<SalesReturnLine> 
                { 
                    new SalesReturnLine { Product = product, Quantity = 5, UnitPrice = 10, Unit = new UnitOfMeasure { Name = "U1" } } 
                } 
            };
            _repoMock.Setup(r => r.GetByIdWithLinesAsync(It.IsAny<int>(), ct)).ReturnsAsync(createdReturn);

            // When
            var result = await _service.CreateAsync(request, ct);

            // Then
            result.Should().NotBeNull();
            result.ReturnNumber.Should().Be("RET-001");
            _repoMock.Verify(r => r.AddAsync(It.IsAny<SalesReturn>(), ct), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task PostAsync_ShouldCallStockInAndReturnResponse()
        {
            // Given
            var id = 1;
            var ct = CancellationToken.None;
            var salesReturn = new SalesReturn 
            { 
                Id = id, 
                CompanyId = _companyId, 
                Status = SalesReturnStatus.Draft,
                ReturnNumber = "RET-001",
                Lines = new List<SalesReturnLine> 
                { 
                    new SalesReturnLine { ProductId = 100, Quantity = 5, UnitId = 1, UnitPrice = 10 } 
                }
            };

            _repoMock.Setup(r => r.GetByIdWithLinesAsync(id, ct)).ReturnsAsync(salesReturn);
            _inventoryServiceMock.Setup(s => s.StockInAsync(It.IsAny<StockInRequest>(), ct))
                .ReturnsAsync(new InventoryDocumentResponse { DocumentId = 200, DocNumber = "SD-200" });

            // When
            var result = await _service.PostAsync(id, ct);

            // Then
            salesReturn.Status.Should().Be(SalesReturnStatus.Posted);
            result.ReturnId.Should().Be(id);
            _moduleAccessMock.Verify(m => m.EnsureInventoryEnabledAsync(ct), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task CancelAsync_ShouldUpdateStatus_WhenDraft()
        {
            // Given
            var id = 1;
            var ct = CancellationToken.None;
            var salesReturn = new SalesReturn 
            { 
                Id = id, 
                CompanyId = _companyId, 
                Status = SalesReturnStatus.Draft,
                Customer = new Customer { Name = "C1" },
                Warehouse = new Warehouse { Name = "W1" },
                Lines = new List<SalesReturnLine>()
            };
            _repoMock.Setup(r => r.GetByIdWithLinesAsync(id, ct)).ReturnsAsync(salesReturn);

            // When
            var result = await _service.CancelAsync(id, ct);

            // Then
            result.Status.Should().Be(SalesReturnStatus.Cancelled.ToString());
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDelete_WhenDraft()
        {
            // Given
            var id = 1;
            var ct = CancellationToken.None;
            var salesReturn = new SalesReturn { Id = id, CompanyId = _companyId, Status = SalesReturnStatus.Draft };
            _repoMock.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync(salesReturn);

            // When
            await _service.DeleteAsync(id, ct);

            // Then
            _repoMock.Verify(r => r.Delete(salesReturn), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }
    }
}
