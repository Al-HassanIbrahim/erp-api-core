using Moq;
using FluentAssertions;
using ERPSystem.Application.Services.Expenses;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Entities.Expenses;
using ERPSystem.Application.DTOs.Expenses;
using ERPSystem.Application.Exceptions;

namespace ERPSystem.Tests.Unit.Services
{
    public class ExpenseCategoryServiceTests
    {
        private readonly Mock<IExpenseCategoryRepository> _repoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly Mock<IModuleAccessService> _moduleAccessMock;
        private readonly ExpenseCategoryService _service;
        private readonly int _companyId = 1;

        public ExpenseCategoryServiceTests()
        {
            _repoMock = new Mock<IExpenseCategoryRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();
            _moduleAccessMock = new Mock<IModuleAccessService>();

            _currentUserMock.Setup(x => x.CompanyId).Returns(_companyId);

            _service = new ExpenseCategoryService(
                _repoMock.Object,
                _currentUserMock.Object,
                _moduleAccessMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnCategories_WhenModuleEnabled()
        {
            // Given
            var ct = CancellationToken.None;
            var categories = new List<ExpenseCategory>
            {
                new ExpenseCategory { Id = 1, Name = "Cat 1", CompanyId = _companyId },
                new ExpenseCategory { Id = 2, Name = "Cat 2", CompanyId = _companyId }
            };

            _repoMock.Setup(r => r.GetAllAsync(_companyId, ct)).ReturnsAsync(categories);

            // When
            var result = await _service.GetAllAsync(ct);

            // Then
            result.Should().HaveCount(2);
            result[0].Name.Should().Be("Cat 1");
            _moduleAccessMock.Verify(m => m.EnsureExpensesEnabledAsync(ct), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCategoryDetails_WhenExists()
        {
            // Given
            var ct = CancellationToken.None;
            var categoryId = 1;
            var category = new ExpenseCategory { Id = categoryId, Name = "Cat 1", CompanyId = _companyId };
            
            _repoMock.Setup(r => r.GetByIdAsync(_companyId, categoryId, ct)).ReturnsAsync(category);
            _repoMock.Setup(r => r.GetCategoryStatsAsync(_companyId, categoryId, ct)).ReturnsAsync((5, 1000m));

            // When
            var result = await _service.GetByIdAsync(categoryId, ct);

            // Then
            result.Should().NotBeNull();
            result!.Name.Should().Be("Cat 1");
            result.ExpenseCount.Should().Be(5);
            result.TotalAmount.Should().Be(1000m);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Given
            var ct = CancellationToken.None;
            var categoryId = 1;
            _repoMock.Setup(r => r.GetByIdAsync(_companyId, categoryId, ct)).ReturnsAsync((ExpenseCategory?)null);

            // When
            var result = await _service.GetByIdAsync(categoryId, ct);

            // Then
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnCategoryDto_WhenNameIsUnique()
        {
            // Given
            var ct = CancellationToken.None;
            var dto = new CreateExpenseCategoryDto { Name = "New Cat" };
            _repoMock.Setup(r => r.GetByNameAsync(_companyId, dto.Name, ct)).ReturnsAsync((ExpenseCategory?)null);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<ExpenseCategory>(), ct))
                .ReturnsAsync((ExpenseCategory entity, CancellationToken c) => { entity.Id = 1; return entity; });

            // When
            var result = await _service.CreateAsync(dto, ct);

            // Then
            result.Name.Should().Be(dto.Name);
            _repoMock.Verify(r => r.CreateAsync(It.Is<ExpenseCategory>(e => e.Name == dto.Name), ct), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenNameAlreadyExists()
        {
            // Given
            var ct = CancellationToken.None;
            var dto = new CreateExpenseCategoryDto { Name = "Existing Cat" };
            _repoMock.Setup(r => r.GetByNameAsync(_companyId, dto.Name, ct)).ReturnsAsync(new ExpenseCategory());

            // When
            var act = () => _service.CreateAsync(dto, ct);

            // Then
            await act.Should().ThrowAsync<BusinessException>()
                .Where(e => e.Code == "DUPLICATE_EXPENSE_CATEGORY_NAME");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCategory_WhenExistsAndNameIsUnique()
        {
            // Given
            var ct = CancellationToken.None;
            var categoryId = 1;
            var dto = new UpdateExpenseCategoryDto { Name = "Updated Name" };
            var entity = new ExpenseCategory { Id = categoryId, Name = "Old Name", CompanyId = _companyId };

            _repoMock.Setup(r => r.GetByIdForUpdateAsync(_companyId, categoryId, ct)).ReturnsAsync(entity);
            _repoMock.Setup(r => r.GetByNameAsync(_companyId, dto.Name, ct)).ReturnsAsync((ExpenseCategory?)null);

            // When
            var result = await _service.UpdateAsync(categoryId, dto, ct);

            // Then
            result.Name.Should().Be(dto.Name);
            entity.Name.Should().Be(dto.Name);
            _repoMock.Verify(r => r.UpdateAsync(entity, ct), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenNotFound()
        {
            // Given
            var ct = CancellationToken.None;
            var categoryId = 1;
            var dto = new UpdateExpenseCategoryDto { Name = "Name" };
            _repoMock.Setup(r => r.GetByIdForUpdateAsync(_companyId, categoryId, ct)).ReturnsAsync((ExpenseCategory?)null);

            // When
            var act = () => _service.UpdateAsync(categoryId, dto, ct);

            // Then
            await act.Should().ThrowAsync<BusinessException>()
                .Where(e => e.Code == "EXPENSE_CATEGORY_NOT_FOUND");
        }

        [Fact]
        public async Task DeleteAsync_ShouldSoftDelete_WhenExistsAndNoExpenses()
        {
            // Given
            var ct = CancellationToken.None;
            var categoryId = 1;
            var entity = new ExpenseCategory { Id = categoryId, CompanyId = _companyId };

            _repoMock.Setup(r => r.GetByIdForUpdateAsync(_companyId, categoryId, ct)).ReturnsAsync(entity);
            _repoMock.Setup(r => r.HasExpensesAsync(_companyId, categoryId, ct)).ReturnsAsync(false);

            // When
            await _service.DeleteAsync(categoryId, ct);

            // Then
            _repoMock.Verify(r => r.SoftDeleteAsync(entity, ct), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenCategoryHasExpenses()
        {
            // Given
            var ct = CancellationToken.None;
            var categoryId = 1;
            var entity = new ExpenseCategory { Id = categoryId, CompanyId = _companyId };

            _repoMock.Setup(r => r.GetByIdForUpdateAsync(_companyId, categoryId, ct)).ReturnsAsync(entity);
            _repoMock.Setup(r => r.HasExpensesAsync(_companyId, categoryId, ct)).ReturnsAsync(true);

            // When
            var act = () => _service.DeleteAsync(categoryId, ct);

            // Then
            await act.Should().ThrowAsync<BusinessException>()
                .Where(e => e.Code == "EXPENSE_CATEGORY_IN_USE");
        }
    }
}
