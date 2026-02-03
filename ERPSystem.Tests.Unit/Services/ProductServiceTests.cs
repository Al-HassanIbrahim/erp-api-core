using Moq;
using FluentAssertions;
using ERPSystem.Application.Services.Products;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Entities.Products;
using ERPSystem.Application.DTOs;

namespace ERPSystem.Tests.Unit.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _repoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly ProductService _service;
        private readonly int _companyId = 1;

        public ProductServiceTests()
        {
            _repoMock = new Mock<IProductRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();

            _currentUserMock.Setup(x => x.CompanyId).Returns(_companyId);

            _service = new ProductService(_repoMock.Object, _currentUserMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnProductDto_WhenProductExists()
        {
            // Given
            var productId = 1;
            var product = new Product 
            { 
                Id = productId, 
                Code = "P1", 
                Name = "Product 1", 
                CompanyId = _companyId,
                UnitOfMeasure = new UnitOfMeasure { Name = "Unit" }
            };
            _repoMock.Setup(r => r.GetByIdAsync(productId, _companyId)).ReturnsAsync(product);

            // When
            var result = await _service.GetByIdAsync(productId);

            // Then
            result.Should().NotBeNull();
            result!.Id.Should().Be(productId);
            result.Code.Should().Be("P1");
            result.Name.Should().Be("Product 1");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Given
            var productId = 1;
            _repoMock.Setup(r => r.GetByIdAsync(productId, _companyId)).ReturnsAsync((Product?)null);

            // When
            var result = await _service.GetByIdAsync(productId);

            // Then
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnProductDtos_WhenProductsExist()
        {
            // Given
            var products = new List<Product>
            {
                new Product { Id = 1, Code = "P1", Name = "Product 1", CompanyId = _companyId, UnitOfMeasure = new UnitOfMeasure { Name = "Unit" } },
                new Product { Id = 2, Code = "P2", Name = "Product 2", CompanyId = _companyId, UnitOfMeasure = new UnitOfMeasure { Name = "Unit" } }
            };
            _repoMock.Setup(r => r.GetAllByCompanyAsync(_companyId)).ReturnsAsync(products);

            // When
            var result = await _service.GetAllAsync();

            // Then
            result.Should().HaveCount(2);
            result.First().Code.Should().Be("P1");
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnProductId_WhenCodeIsUnique()
        {
            // Given
            var dto = new CreateProductDto { Code = "P1", Name = "Product 1", UnitOfMeasureId = 1 };
            _repoMock.Setup(r => r.CodeExistsAsync(dto.Code, _companyId, null)).ReturnsAsync(false);

            // When
            var result = await _service.CreateAsync(dto);

            // Then
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenCodeAlreadyExists()
        {
            // Given
            var dto = new CreateProductDto { Code = "P1", Name = "Product 1" };
            _repoMock.Setup(r => r.CodeExistsAsync(dto.Code, _companyId, null)).ReturnsAsync(true);

            // When
            var act = () => _service.CreateAsync(dto);

            // Then
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Product code '{dto.Code}' already exists.");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateProduct_WhenProductExistsAndCodeIsUnique()
        {
            // Given
            var dto = new UpdateProductDto { Id = 1, Code = "P1_New", Name = "Product 1 New" };
            var product = new Product { Id = 1, Code = "P1", Name = "Product 1", CompanyId = _companyId };
            
            _repoMock.Setup(r => r.GetByIdAsync(dto.Id, _companyId)).ReturnsAsync(product);
            _repoMock.Setup(r => r.CodeExistsAsync(dto.Code, _companyId, dto.Id)).ReturnsAsync(false);

            // When
            await _service.UpdateAsync(dto);

            // Then
            product.Code.Should().Be(dto.Code);
            product.Name.Should().Be(dto.Name);
            _repoMock.Verify(r => r.UpdateAsync(product), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenProductDoesNotExist()
        {
            // Given
            var dto = new UpdateProductDto { Id = 1, Code = "P1", Name = "Product 1" };
            _repoMock.Setup(r => r.GetByIdAsync(dto.Id, _companyId)).ReturnsAsync((Product?)null);

            // When
            var act = () => _service.UpdateAsync(dto);

            // Then
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Product with ID {dto.Id} not found or does not belong to your company.");
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenCodeAlreadyExists()
        {
            // Given
            var dto = new UpdateProductDto { Id = 1, Code = "P2", Name = "Product 1" };
            var product = new Product { Id = 1, Code = "P1", Name = "Product 1", CompanyId = _companyId };
            
            _repoMock.Setup(r => r.GetByIdAsync(dto.Id, _companyId)).ReturnsAsync(product);
            _repoMock.Setup(r => r.CodeExistsAsync(dto.Code, _companyId, dto.Id)).ReturnsAsync(true);

            // When
            var act = () => _service.UpdateAsync(dto);

            // Then
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Product code '{dto.Code}' already exists.");
        }

        [Fact]
        public async Task DeleteAsync_ShouldMarkAsDeleted_WhenProductExists()
        {
            // Given
            var productId = 1;
            var product = new Product { Id = productId, CompanyId = _companyId, IsDeleted = false };
            _repoMock.Setup(r => r.GetByIdAsync(productId, _companyId)).ReturnsAsync(product);

            // When
            await _service.DeleteAsync(productId);

            // Then
            product.IsDeleted.Should().BeTrue();
            _repoMock.Verify(r => r.UpdateAsync(product), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDoNothing_WhenProductDoesNotExist()
        {
            // Given
            var productId = 1;
            _repoMock.Setup(r => r.GetByIdAsync(productId, _companyId)).ReturnsAsync((Product?)null);

            // When
            await _service.DeleteAsync(productId);

            // Then
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}
