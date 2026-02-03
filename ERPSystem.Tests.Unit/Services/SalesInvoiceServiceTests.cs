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

namespace ERPSystem.Tests.Unit.Services
{
    public class SalesInvoiceServiceTests
    {
        private readonly Mock<ISalesInvoiceRepository> _repoMock;
        private readonly Mock<ICustomerRepository> _customerRepoMock;
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly Mock<IUnitOfMeasureRepository> _unitRepoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly Mock<IModuleAccessService> _moduleAccessMock;
        private readonly SalesInvoiceService _service;
        private readonly int _companyId = 1;
        private readonly Guid _userId = Guid.NewGuid();

        public SalesInvoiceServiceTests()
        {
            _repoMock = new Mock<ISalesInvoiceRepository>();
            _customerRepoMock = new Mock<ICustomerRepository>();
            _productRepoMock = new Mock<IProductRepository>();
            _unitRepoMock = new Mock<IUnitOfMeasureRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();
            _moduleAccessMock = new Mock<IModuleAccessService>();

            _currentUserMock.Setup(x => x.CompanyId).Returns(_companyId);
            _currentUserMock.Setup(x => x.UserId).Returns(_userId);

            _service = new SalesInvoiceService(
                _repoMock.Object,
                _customerRepoMock.Object,
                _productRepoMock.Object,
                _unitRepoMock.Object,
                _currentUserMock.Object,
                _moduleAccessMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenNoLines()
        {
            // Given
            var request = new CreateSalesInvoiceRequest 
            { 
                CustomerId = 1, 
                Lines = new List<CreateSalesInvoiceLineRequest>() 
            };
            var ct = CancellationToken.None;

            _customerRepoMock.Setup(r => r.GetByIdAsync(1, ct))
                .ReturnsAsync(new Customer { Id = 1, CompanyId = _companyId });

            // When
            var act = () => _service.CreateAsync(request, ct);

            // Then
            await act.Should().ThrowAsync<BusinessException>()
                .Where(e => e.Code == "NO_LINES");
        }

        [Fact]
        public async Task PostAsync_ShouldUpdateStatusToPosted()
        {
            // Given
            var invoiceId = 1;
            var ct = CancellationToken.None;
            var invoice = new SalesInvoice 
            { 
                Id = invoiceId, 
                CompanyId = _companyId, 
                Status = SalesInvoiceStatus.Draft,
                Customer = new Customer { Name = "Cust" },
                Lines = new List<SalesInvoiceLine>()
            };

            _repoMock.Setup(r => r.GetByIdWithLinesAsync(invoiceId, ct))
                .ReturnsAsync(invoice);

            // When
            await _service.PostAsync(invoiceId, ct);

            // Then
            invoice.Status.Should().Be(SalesInvoiceStatus.Posted);
            _repoMock.Verify(r => r.Update(invoice), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task PostAsync_ShouldThrowException_WhenNotDraft()
        {
            // Given
            var invoiceId = 1;
            var ct = CancellationToken.None;
            var invoice = new SalesInvoice 
            { 
                Id = invoiceId, 
                CompanyId = _companyId, 
                Status = SalesInvoiceStatus.Posted 
            };

            _repoMock.Setup(r => r.GetByIdWithLinesAsync(invoiceId, ct))
                .ReturnsAsync(invoice);

            // When
            var act = () => _service.PostAsync(invoiceId, ct);

            // Then
            await act.Should().ThrowAsync<BusinessException>()
                .Where(e => e.Code == "INVALID_STATUS");
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenPosted()
        {
            // Given
            var invoiceId = 1;
            var ct = CancellationToken.None;
            var invoice = new SalesInvoice 
            { 
                Id = invoiceId, 
                CompanyId = _companyId, 
                Status = SalesInvoiceStatus.Posted 
            };

            _repoMock.Setup(r => r.GetByIdAsync(invoiceId, ct))
                .ReturnsAsync(invoice);

            // When
            var act = () => _service.DeleteAsync(invoiceId, ct);

            // Then
            await act.Should().ThrowAsync<BusinessException>()
                .Where(e => e.Code == "INVALID_STATUS");
        }
    }
}
