using Moq;
using FluentAssertions;
using ERPSystem.Application.Services.Sales;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Application.DTOs.Sales;
using ERPSystem.Application.Exceptions;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Tests.Unit.Services
{
    public class SalesReceiptServiceTests
    {
        private readonly Mock<ISalesReceiptRepository> _repoMock;
        private readonly Mock<ISalesInvoiceRepository> _invoiceRepoMock;
        private readonly Mock<ICustomerRepository> _customerRepoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly Mock<IModuleAccessService> _moduleAccessMock;
        private readonly SalesReceiptService _service;
        private readonly int _companyId = 1;

        public SalesReceiptServiceTests()
        {
            _repoMock = new Mock<ISalesReceiptRepository>();
            _invoiceRepoMock = new Mock<ISalesInvoiceRepository>();
            _customerRepoMock = new Mock<ICustomerRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();
            _moduleAccessMock = new Mock<IModuleAccessService>();

            _currentUserMock.Setup(x => x.CompanyId).Returns(_companyId);

            _service = new SalesReceiptService(
                _repoMock.Object,
                _invoiceRepoMock.Object,
                _customerRepoMock.Object,
                _currentUserMock.Object,
                _moduleAccessMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnReceipts()
        {
            // Given
            var ct = CancellationToken.None;
            var receipts = new List<SalesReceipt>
            {
                new SalesReceipt { Id = 1, ReceiptNumber = "R1", Customer = new Customer { Name = "C1" }, Amount = 100, Status = SalesReceiptStatus.Draft, CompanyId = _companyId },
                new SalesReceipt { Id = 2, ReceiptNumber = "R2", Customer = new Customer { Name = "C2" }, Amount = 200, Status = SalesReceiptStatus.Posted, CompanyId = _companyId }
            };

            _repoMock.Setup(r => r.GetAllByCompanyAsync(_companyId, null, null, null, null, ct))
                .ReturnsAsync(receipts);

            // When
            var result = await _service.GetAllAsync(cancellationToken: ct);

            // Then
            result.Should().HaveCount(2);
            _moduleAccessMock.Verify(m => m.EnsureSalesEnabledAsync(ct), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnReceipt_WhenExists()
        {
            // Given
            var id = 1;
            var ct = CancellationToken.None;
            var receipt = new SalesReceipt 
            { 
                Id = id, 
                ReceiptNumber = "R1", 
                Customer = new Customer { Name = "C1" }, 
                CompanyId = _companyId,
                Allocations = new List<SalesReceiptAllocation>()
            };

            _repoMock.Setup(r => r.GetByIdWithAllocationsAsync(id, ct)).ReturnsAsync(receipt);

            // When
            var result = await _service.GetByIdAsync(id, ct);

            // Then
            result.Should().NotBeNull();
            result!.ReceiptNumber.Should().Be("R1");
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnCreatedReceipt()
        {
            // Given
            var ct = CancellationToken.None;
            var customerId = 5;
            var invoiceId = 10;
            var customer = new Customer { Id = customerId, CompanyId = _companyId, Name = "C1" };
            var invoice = new SalesInvoice { Id = invoiceId, CompanyId = _companyId, CustomerId = customerId, GrandTotal = 1000, PaidAmount = 0 };
            
            var request = new CreateSalesReceiptRequest
            {
                CustomerId = customerId,
                Amount = 500,
                ReceiptDate = DateTime.UtcNow,
                Allocations = new List<CreateReceiptAllocationRequest>
                {
                    new CreateReceiptAllocationRequest { SalesInvoiceId = invoiceId, AllocatedAmount = 300 }
                }
            };

            _customerRepoMock.Setup(r => r.GetByIdAsync(customerId, ct)).ReturnsAsync(customer);
            _invoiceRepoMock.Setup(r => r.GetByIdAsync(invoiceId, ct)).ReturnsAsync(invoice);
            _repoMock.Setup(r => r.GenerateReceiptNumberAsync(_companyId, ct)).ReturnsAsync("REC-001");
            
            var createdReceipt = new SalesReceipt 
            { 
                Id = 1, 
                ReceiptNumber = "REC-001", 
                Customer = customer, 
                Amount = 500,
                Allocations = new List<SalesReceiptAllocation> 
                { 
                    new SalesReceiptAllocation { SalesInvoice = invoice, AllocatedAmount = 300 } 
                } 
            };
            _repoMock.Setup(r => r.GetByIdWithAllocationsAsync(It.IsAny<int>(), ct)).ReturnsAsync(createdReceipt);

            // When
            var result = await _service.CreateAsync(request, ct);

            // Then
            result.Should().NotBeNull();
            result.ReceiptNumber.Should().Be("REC-001");
            _repoMock.Verify(r => r.AddAsync(It.IsAny<SalesReceipt>(), ct), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task PostAsync_ShouldUpdateInvoicesAndReturnReceipt()
        {
            // Given
            var id = 1;
            var ct = CancellationToken.None;
            var invoice = new SalesInvoice { Id = 10, GrandTotal = 1000, PaidAmount = 0, InvoiceNumber = "INV10" };
            var receipt = new SalesReceipt 
            { 
                Id = id, 
                CompanyId = _companyId, 
                Status = SalesReceiptStatus.Draft,
                Allocations = new List<SalesReceiptAllocation> 
                { 
                    new SalesReceiptAllocation { SalesInvoiceId = 10, AllocatedAmount = 300, SalesInvoice = invoice } 
                },
                Customer = new Customer { Name = "C1" }
            };

            _repoMock.Setup(r => r.GetByIdWithAllocationsAsync(id, ct)).ReturnsAsync(receipt);
            _invoiceRepoMock.Setup(r => r.GetByIdAsync(10, ct)).ReturnsAsync(invoice);

            // When
            var result = await _service.PostAsync(id, ct);

            // Then
            receipt.Status.Should().Be(SalesReceiptStatus.Posted);
            invoice.PaidAmount.Should().Be(300);
            invoice.PaymentStatus.Should().Be(PaymentStatus.PartiallyPaid);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task CancelAsync_ShouldReverseAllocations_WhenPosted()
        {
            // Given
            var id = 1;
            var ct = CancellationToken.None;
            var invoice = new SalesInvoice { Id = 10, GrandTotal = 1000, PaidAmount = 300, PaymentStatus = PaymentStatus.PartiallyPaid, InvoiceNumber = "INV10" };
            var receipt = new SalesReceipt 
            { 
                Id = id, 
                CompanyId = _companyId, 
                Status = SalesReceiptStatus.Posted,
                Allocations = new List<SalesReceiptAllocation> 
                { 
                    new SalesReceiptAllocation { SalesInvoiceId = 10, AllocatedAmount = 300, SalesInvoice = invoice } 
                },
                Customer = new Customer { Name = "C1" }
            };

            _repoMock.Setup(r => r.GetByIdWithAllocationsAsync(id, ct)).ReturnsAsync(receipt);
            _invoiceRepoMock.Setup(r => r.GetByIdAsync(10, ct)).ReturnsAsync(invoice);

            // When
            var result = await _service.CancelAsync(id, ct);

            // Then
            receipt.Status.Should().Be(SalesReceiptStatus.Cancelled);
            invoice.PaidAmount.Should().Be(0);
            invoice.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDelete_WhenDraft()
        {
            // Given
            var id = 1;
            var ct = CancellationToken.None;
            var receipt = new SalesReceipt { Id = id, CompanyId = _companyId, Status = SalesReceiptStatus.Draft };
            _repoMock.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync(receipt);

            // When
            await _service.DeleteAsync(id, ct);

            // Then
            _repoMock.Verify(r => r.Delete(receipt), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }
    }
}
