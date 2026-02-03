using Moq;
using FluentAssertions;
using ERPSystem.Application.Services.Sales;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Application.DTOs.Sales;
using ERPSystem.Application.Exceptions;
using ERPSystem.Domain.Enums;
using ERPSystem.Domain.Entities.Inventory;
using ERPSystem.Application.DTOs.Inventory;
using ERPSystem.Domain.Entities.Products;

namespace ERPSystem.Tests.Unit.Services
{
    public class SalesDeliveryServiceTests
    {
        private readonly Mock<ISalesDeliveryRepository> _repoMock;
        private readonly Mock<ISalesInvoiceRepository> _invoiceRepoMock;
        private readonly Mock<IWarehouseRepository> _warehouseRepoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly Mock<IModuleAccessService> _moduleAccessMock;
        private readonly Mock<IInventoryService> _inventoryServiceMock;
        private readonly SalesDeliveryService _service;
        private readonly int _companyId = 1;

        public SalesDeliveryServiceTests()
        {
            _repoMock = new Mock<ISalesDeliveryRepository>();
            _invoiceRepoMock = new Mock<ISalesInvoiceRepository>();
            _warehouseRepoMock = new Mock<IWarehouseRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();
            _moduleAccessMock = new Mock<IModuleAccessService>();
            _inventoryServiceMock = new Mock<IInventoryService>();

            _currentUserMock.Setup(x => x.CompanyId).Returns(_companyId);

            _service = new SalesDeliveryService(
                _repoMock.Object,
                _invoiceRepoMock.Object,
                _warehouseRepoMock.Object,
                _currentUserMock.Object,
                _moduleAccessMock.Object,
                _inventoryServiceMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnDeliveries()
        {
            // Given
            var ct = CancellationToken.None;
            var deliveries = new List<SalesDelivery>
            {
                new SalesDelivery { Id = 1, DeliveryNumber = "D1", SalesInvoice = new SalesInvoice { InvoiceNumber = "INV1" }, Customer = new Customer { Name = "C1" }, Warehouse = new Warehouse { Name = "W1" }, Status = SalesDeliveryStatus.Draft, CompanyId = _companyId },
                new SalesDelivery { Id = 2, DeliveryNumber = "D2", SalesInvoice = new SalesInvoice { InvoiceNumber = "INV2" }, Customer = new Customer { Name = "C2" }, Warehouse = new Warehouse { Name = "W2" }, Status = SalesDeliveryStatus.Posted, CompanyId = _companyId }
            };

            _repoMock.Setup(r => r.GetAllByCompanyAsync(_companyId, null, null, null, null, ct))
                .ReturnsAsync(deliveries);

            // When
            var result = await _service.GetAllAsync(cancellationToken: ct);

            // Then
            result.Should().HaveCount(2);
            _moduleAccessMock.Verify(m => m.EnsureSalesEnabledAsync(ct), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDelivery_WhenExists()
        {
            // Given
            var id = 1;
            var ct = CancellationToken.None;
            var delivery = new SalesDelivery { Id = id, DeliveryNumber = "D1", SalesInvoice = new SalesInvoice { InvoiceNumber = "INV1" }, Customer = new Customer { Name = "C1" }, Warehouse = new Warehouse { Name = "W1" }, CompanyId = _companyId };

            _repoMock.Setup(r => r.GetByIdWithLinesAsync(id, ct)).ReturnsAsync(delivery);

            // When
            var result = await _service.GetByIdAsync(id, ct);

            // Then
            result.Should().NotBeNull();
            result!.DeliveryNumber.Should().Be("D1");
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnCreatedDelivery()
        {
            // Given
            var ct = CancellationToken.None;
            var invoiceId = 10;
            var warehouseId = 20;
            var invoice = new SalesInvoice { Id = invoiceId, CompanyId = _companyId, Status = SalesInvoiceStatus.Posted, CustomerId = 5, Lines = new List<SalesInvoiceLine> { new SalesInvoiceLine { Id = 1, ProductId = 1, Quantity = 10, DeliveredQuantity = 0 } } };
            var warehouse = new Warehouse { Id = warehouseId, CompanyId = _companyId, IsActive = true };
            var request = new CreateSalesDeliveryRequest
            {
                SalesInvoiceId = invoiceId,
                WarehouseId = warehouseId,
                DeliveryDate = DateTime.UtcNow,
                Lines = new List<CreateSalesDeliveryLineRequest>
                {
                    new CreateSalesDeliveryLineRequest { SalesInvoiceLineId = 1, Quantity = 5 }
                }
            };

            _invoiceRepoMock.Setup(r => r.GetByIdWithLinesAsync(invoiceId, ct)).ReturnsAsync(invoice);
            _warehouseRepoMock.Setup(r => r.GetByIdAsync(warehouseId)).ReturnsAsync(warehouse);
            _repoMock.Setup(r => r.GenerateDeliveryNumberAsync(_companyId, ct)).ReturnsAsync("DEL-001");
            
            var createdDelivery = new SalesDelivery 
            { 
                Id = 1, 
                DeliveryNumber = "DEL-001", 
                SalesInvoice = invoice, 
                Customer = new Customer { Name = "C1" }, 
                Warehouse = warehouse,
                Lines = new List<SalesDeliveryLine> { new SalesDeliveryLine { Product = new Product { Name = "P1", Code = "PC1" }, Unit = new UnitOfMeasure { Name = "U1" } } }
            };
            _repoMock.Setup(r => r.GetByIdWithLinesAsync(It.IsAny<int>(), ct)).ReturnsAsync(createdDelivery);

            // When
            var result = await _service.CreateAsync(request, ct);

            // Then
            result.Should().NotBeNull();
            result.DeliveryNumber.Should().Be("DEL-001");
            _repoMock.Verify(r => r.AddAsync(It.IsAny<SalesDelivery>(), ct), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task PostAsync_ShouldUpdateStatusAndReturnResponse()
        {
            // Given
            var id = 1;
            var ct = CancellationToken.None;
            var invoice = new SalesInvoice 
            { 
                Id = 10, 
                Lines = new List<SalesInvoiceLine> 
                { 
                    new SalesInvoiceLine { Id = 1, Quantity = 10, DeliveredQuantity = 0 } 
                } 
            };
            var delivery = new SalesDelivery 
            { 
                Id = id, 
                CompanyId = _companyId, 
                Status = SalesDeliveryStatus.Draft,
                SalesInvoiceId = 10,
                Lines = new List<SalesDeliveryLine> 
                { 
                    new SalesDeliveryLine { SalesInvoiceLineId = 1, Quantity = 5, ProductId = 1, UnitId = 1 } 
                }
            };

            _repoMock.Setup(r => r.GetByIdWithLinesAsync(id, ct)).ReturnsAsync(delivery);
            _invoiceRepoMock.Setup(r => r.GetByIdWithLinesAsync(10, ct)).ReturnsAsync(invoice);
            _inventoryServiceMock.Setup(s => s.StockOutAsync(It.IsAny<StockOutRequest>(), ct))
                .ReturnsAsync(new InventoryDocumentResponse { DocumentId = 100, DocNumber = "SD-100" });

            // When
            var result = await _service.PostAsync(id, ct);

            // Then
            result.DeliveryId.Should().Be(id);
            delivery.Status.Should().Be(SalesDeliveryStatus.Posted);
            invoice.Lines.First().DeliveredQuantity.Should().Be(5);
            _moduleAccessMock.Verify(m => m.EnsureInventoryEnabledAsync(ct), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task CancelAsync_ShouldUpdateStatus_WhenDraft()
        {
            // Given
            var id = 1;
            var ct = CancellationToken.None;
            var delivery = new SalesDelivery 
            { 
                Id = id, 
                CompanyId = _companyId, 
                Status = SalesDeliveryStatus.Draft,
                SalesInvoice = new SalesInvoice { InvoiceNumber = "INV1" },
                Customer = new Customer { Name = "C1" },
                Warehouse = new Warehouse { Name = "W1" },
                Lines = new List<SalesDeliveryLine>()
            };
            _repoMock.Setup(r => r.GetByIdWithLinesAsync(id, ct)).ReturnsAsync(delivery);

            // When
            var result = await _service.CancelAsync(id, ct);

            // Then
            result.Status.Should().Be(SalesDeliveryStatus.Cancelled.ToString());
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDelete_WhenDraft()
        {
            // Given
            var id = 1;
            var ct = CancellationToken.None;
            var delivery = new SalesDelivery { Id = id, CompanyId = _companyId, Status = SalesDeliveryStatus.Draft };
            _repoMock.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync(delivery);

            // When
            await _service.DeleteAsync(id, ct);

            // Then
            _repoMock.Verify(r => r.Delete(delivery), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }
    }
}
