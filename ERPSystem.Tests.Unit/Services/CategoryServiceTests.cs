using Moq;
using FluentAssertions;
using ERPSystem.Application.Services.Products;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Entities.Products;
using ERPSystem.Application.DTOs.Products;

namespace ERPSystem.Tests.Unit.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _repoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly CategoryService _service;
        private readonly int _companyId = 1;

        public CategoryServiceTests()
        {
            _repoMock = new Mock<ICategoryRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();

            _currentUserMock.Setup(x => x.CompanyId).Returns(_companyId);

            _service = new CategoryService(
                _repoMock.Object,
                _currentUserMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnCategories()
        {
            // Given
            var ct = CancellationToken.None;
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Cat 1", CompanyId = _companyId },
                new Category { Id = 2, Name = "Cat 2", CompanyId = _companyId }
            };

            _repoMock.Setup(r => r.GetAllByCompanyAsync(_companyId, ct))
                .ReturnsAsync(categories);

            // When
            var result = await _service.GetAllAsync(ct);

            // Then
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCategory_WhenExists()
        {
            // Given
            var ct = CancellationToken.None;
            var category = new Category { Id = 1, Name = "Cat 1", CompanyId = _companyId };

            _repoMock.Setup(r => r.GetByIdAsync(1, ct))
                .ReturnsAsync(category);

            // When
            var result = await _service.GetByIdAsync(1, ct);

            // Then
            result.Should().NotBeNull();
            result!.Name.Should().Be("Cat 1");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenParentNotFound()
        {
            // Given
            var request = new CreateCategoryRequest { Name = "Cat 1", ParentCategoryId = 99 };
            var ct = CancellationToken.None;

            _repoMock.Setup(r => r.GetByIdAsync(99, ct))
                .ReturnsAsync((Category?)null);

            // When
            var act = () => _service.CreateAsync(request, ct);

            // Then
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Parent category not found.");
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnCreatedCategory_WhenValid()
        {
            // Given
            var request = new CreateCategoryRequest { Name = "Cat 1" };
            var ct = CancellationToken.None;

            // When
            var result = await _service.CreateAsync(request, ct);

            // Then
            result.Name.Should().Be("Cat 1");
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Category>(), ct), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenItsOwnParent()
        {
            // Given
            var request = new UpdateCategoryRequest { Name = "Cat 1", ParentCategoryId = 1 };
            var ct = CancellationToken.None;
            var category = new Category { Id = 1, CompanyId = _companyId };

            _repoMock.Setup(r => r.GetByIdAsync(1, ct))
                .ReturnsAsync(category);

            // When
            var act = () => _service.UpdateAsync(1, request, ct);

            // Then
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Category cannot be its own parent.");
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDelete_WhenExists()
        {
            // Given
            var ct = CancellationToken.None;
            var category = new Category { Id = 1, CompanyId = _companyId };

            _repoMock.Setup(r => r.GetByIdAsync(1, ct))
                .ReturnsAsync(category);

            // When
            await _service.DeleteAsync(1, ct);

            // Then
            _repoMock.Verify(r => r.Delete(category), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }
    }
}
