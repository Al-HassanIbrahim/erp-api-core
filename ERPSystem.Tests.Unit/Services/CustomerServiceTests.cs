using Moq;
using FluentAssertions;
using ERPSystem.Application.Services.Sales;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Application.DTOs.Sales;
using ERPSystem.Application.Exceptions;

namespace ERPSystem.Tests.Unit.Services
{
    public class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _repoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly Mock<IModuleAccessService> _moduleAccessMock;
        private readonly CustomerService _service;
        private readonly int _companyId = 1;
        private readonly Guid _userId = Guid.NewGuid();

        public CustomerServiceTests()
        {
            _repoMock = new Mock<ICustomerRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();
            _moduleAccessMock = new Mock<IModuleAccessService>();

            _currentUserMock.Setup(x => x.CompanyId).Returns(_companyId);
            _currentUserMock.Setup(x => x.UserId).Returns(_userId);

            _service = new CustomerService(
                _repoMock.Object,
                _currentUserMock.Object,
                _moduleAccessMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnCustomers()
        {
            // Given
            var ct = CancellationToken.None;
            var customers = new List<Customer>
            {
                new Customer { Id = 1, Name = "Cust 1", CompanyId = _companyId },
                new Customer { Id = 2, Name = "Cust 2", CompanyId = _companyId }
            };

            _repoMock.Setup(r => r.GetAllByCompanyAsync(_companyId, null, ct))
                .ReturnsAsync(customers);

            // When
            var result = await _service.GetAllAsync(null, ct);

            // Then
            result.Should().HaveCount(2);
            _moduleAccessMock.Verify(m => m.EnsureSalesEnabledAsync(ct), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCustomer_WhenExists()
        {
            // Given
            var ct = CancellationToken.None;
            var customer = new Customer { Id = 1, Name = "Cust 1", CompanyId = _companyId };

            _repoMock.Setup(r => r.GetByIdAsync(1, ct))
                .ReturnsAsync(customer);

            // When
            var result = await _service.GetByIdAsync(1, ct);

            // Then
            result.Should().NotBeNull();
            result!.Name.Should().Be("Cust 1");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenCodeExists()
        {
            // Given
            var request = new CreateCustomerRequest { Code = "C001", Name = "Cust 1" };
            var ct = CancellationToken.None;

            _repoMock.Setup(r => r.ExistsAsync(_companyId, "C001", null, ct))
                .ReturnsAsync(true);

            // When
            var act = () => _service.CreateAsync(request, ct);

            // Then
            await act.Should().ThrowAsync<BusinessException>()
                .Where(e => e.Code == "DUPLICATE_CODE");
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnCreatedCustomer_WhenValid()
        {
            // Given
            var request = new CreateCustomerRequest { Code = "C001", Name = "Cust 1" };
            var ct = CancellationToken.None;

            _repoMock.Setup(r => r.ExistsAsync(_companyId, "C001", null, ct))
                .ReturnsAsync(false);

            // When
            var result = await _service.CreateAsync(request, ct);

            // Then
            result.Code.Should().Be("C001");
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Customer>(), ct), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenCustomerNotFound()
        {
            // Given
            var request = new UpdateCustomerRequest { Name = "New Name" };
            var ct = CancellationToken.None;

            _repoMock.Setup(r => r.GetByIdAsync(1, ct))
                .ReturnsAsync((Customer?)null);

            // When
            var act = () => _service.UpdateAsync(1, request, ct);

            // Then
            await act.Should().ThrowAsync<BusinessException>()
                .Where(e => e.Code == "CUSTOMER_NOT_FOUND");
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDelete_WhenExists()
        {
            // Given
            var ct = CancellationToken.None;
            var customer = new Customer { Id = 1, CompanyId = _companyId };

            _repoMock.Setup(r => r.GetByIdAsync(1, ct))
                .ReturnsAsync(customer);

            // When
            await _service.DeleteAsync(1, ct);

            // Then
            _repoMock.Verify(r => r.Delete(customer), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }
    }
}
