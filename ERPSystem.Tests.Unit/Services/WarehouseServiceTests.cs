using Moq;
using FluentAssertions;
using ERPSystem.Application.Services.Inventory;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Entities.Inventory;
using ERPSystem.Application.DTOs.Inventory;

namespace ERPSystem.Tests.Unit.Services
{
    public class WarehouseServiceTests
    {
        private readonly Mock<IWarehouseRepository> _repoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly WarehouseService _service;
        private readonly int _companyId = 1;

        public WarehouseServiceTests()
        {
            _repoMock = new Mock<IWarehouseRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();

            _currentUserMock.Setup(x => x.CompanyId).Returns(_companyId);

            _service = new WarehouseService(_repoMock.Object, _currentUserMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnWarehouseDtos()
        {
            // Given
            var warehouses = new List<Warehouse>
            {
                new Warehouse { Id = 1, Code = "W1", Name = "Warehouse 1", CompanyId = _companyId },
                new Warehouse { Id = 2, Code = "W2", Name = "Warehouse 2", CompanyId = _companyId }
            };
            _repoMock.Setup(r => r.GetAllAsync(_companyId, null)).ReturnsAsync(warehouses);

            // When
            var result = await _service.GetAllAsync();

            // Then
            result.Should().HaveCount(2);
            result[0].Code.Should().Be("W1");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnWarehouseDto_WhenExists()
        {
            // Given
            var warehouseId = 1;
            var warehouse = new Warehouse { Id = warehouseId, Code = "W1", Name = "Warehouse 1", CompanyId = _companyId };
            _repoMock.Setup(r => r.GetByIdAsync(warehouseId, _companyId)).ReturnsAsync(warehouse);

            // When
            var result = await _service.GetByIdAsync(warehouseId);

            // Then
            result.Should().NotBeNull();
            result!.Id.Should().Be(warehouseId);
            result.Code.Should().Be("W1");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Given
            var warehouseId = 1;
            _repoMock.Setup(r => r.GetByIdAsync(warehouseId, _companyId)).ReturnsAsync((Warehouse?)null);

            // When
            var result = await _service.GetByIdAsync(warehouseId);

            // Then
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnWarehouseId_WhenCodeIsUnique()
        {
            // Given
            var dto = new CreateWarehouseDto { Code = "W1", Name = "Warehouse 1" };
            _repoMock.Setup(r => r.CodeExistsAsync(dto.Code, _companyId, null)).ReturnsAsync(false);
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Warehouse>()))
                .Callback<Warehouse>(w => w.Id = 1);

            // When
            var result = await _service.CreateAsync(dto);

            // Then
            result.Should().Be(1);
            _repoMock.Verify(r => r.AddAsync(It.Is<Warehouse>(w => w.Code == dto.Code)), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenCodeAlreadyExists()
        {
            // Given
            var dto = new CreateWarehouseDto { Code = "W1", Name = "Warehouse 1" };
            _repoMock.Setup(r => r.CodeExistsAsync(dto.Code, _companyId, null)).ReturnsAsync(true);

            // When
            var act = () => _service.CreateAsync(dto);

            // Then
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Warehouse code already exists for this company.");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateWarehouse_WhenExistsAndCodeIsUnique()
        {
            // Given
            var warehouseId = 1;
            var dto = new UpdateWarehouseDto { Code = "W1_New", Name = "Warehouse 1 New", IsActive = true };
            var warehouse = new Warehouse { Id = warehouseId, Code = "W1", Name = "Warehouse 1", CompanyId = _companyId };

            _repoMock.Setup(r => r.GetByIdAsync(warehouseId, _companyId)).ReturnsAsync(warehouse);
            _repoMock.Setup(r => r.CodeExistsAsync(dto.Code, _companyId, warehouseId)).ReturnsAsync(false);

            // When
            var result = await _service.UpdateAsync(warehouseId, dto);

            // Then
            result.Should().BeTrue();
            warehouse.Code.Should().Be(dto.Code);
            warehouse.Name.Should().Be(dto.Name);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotFound()
        {
            // Given
            var warehouseId = 1;
            var dto = new UpdateWarehouseDto { Code = "W1", Name = "Name" };
            _repoMock.Setup(r => r.GetByIdAsync(warehouseId, _companyId)).ReturnsAsync((Warehouse?)null);

            // When
            var result = await _service.UpdateAsync(warehouseId, dto);

            // Then
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_ShouldSoftDelete_WhenNoActivity()
        {
            // Given
            var warehouseId = 1;
            var warehouse = new Warehouse { Id = warehouseId, CompanyId = _companyId, IsDeleted = false };
            _repoMock.Setup(r => r.GetByIdAsync(warehouseId, _companyId)).ReturnsAsync(warehouse);
            _repoMock.Setup(r => r.HasInventoryActivityAsync(warehouseId)).ReturnsAsync(false);

            // When
            var result = await _service.DeleteAsync(warehouseId);

            // Then
            result.Should().BeTrue();
            warehouse.IsDeleted.Should().BeTrue();
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeactivate_WhenHasActivity()
        {
            // Given
            var warehouseId = 1;
            var warehouse = new Warehouse { Id = warehouseId, CompanyId = _companyId, IsActive = true, IsDeleted = false };
            _repoMock.Setup(r => r.GetByIdAsync(warehouseId, _companyId)).ReturnsAsync(warehouse);
            _repoMock.Setup(r => r.HasInventoryActivityAsync(warehouseId)).ReturnsAsync(true);

            // When
            var result = await _service.DeleteAsync(warehouseId);

            // Then
            result.Should().BeTrue();
            warehouse.IsActive.Should().BeFalse();
            warehouse.IsDeleted.Should().BeFalse();
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
        {
            // Given
            var warehouseId = 1;
            _repoMock.Setup(r => r.GetByIdAsync(warehouseId, _companyId)).ReturnsAsync((Warehouse?)null);

            // When
            var result = await _service.DeleteAsync(warehouseId);

            // Then
            result.Should().BeFalse();
        }
    }
}
