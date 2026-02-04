using Moq;
using FluentAssertions;
using ERPSystem.Application.Services.Expenses;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Entities.Expenses;
using ERPSystem.Application.DTOs.Expenses;
using ERPSystem.Application.Exceptions;
using ERPSystem.Domain.Enums;
using ERPSystem.Application.DTOs;

namespace ERPSystem.Tests.Unit.Services
{
    public class ExpenseServiceTests
    {
        private readonly Mock<IExpenseRepository> _repoMock;
        private readonly Mock<IExpenseCategoryRepository> _categoryRepoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly Mock<IModuleAccessService> _moduleAccessMock;
        private readonly ExpenseService _service;
        private readonly int _companyId = 1;

        public ExpenseServiceTests()
        {
            _repoMock = new Mock<IExpenseRepository>();
            _categoryRepoMock = new Mock<IExpenseCategoryRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();
            _moduleAccessMock = new Mock<IModuleAccessService>();

            _currentUserMock.Setup(x => x.CompanyId).Returns(_companyId);

            _service = new ExpenseService(
                _repoMock.Object,
                _categoryRepoMock.Object,
                _currentUserMock.Object,
                _moduleAccessMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnPagedExpenses()
        {
            // Given
            var query = new ExpenseQuery { Page = 1, PageSize = 10 };
            var ct = CancellationToken.None;
            var expenses = new List<Expense>
            {
                new Expense { Id = 1, Description = "Exp 1", Amount = 100, CompanyId = _companyId, Status = ExpenseStatus.Paid },
                new Expense { Id = 2, Description = "Exp 2", Amount = 200, CompanyId = _companyId, Status = ExpenseStatus.Pending }
            };

            _repoMock.Setup(r => r.GetPagedAsync(_companyId, It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<ExpenseStatus?>(), 
                It.IsAny<PaymentMethod?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<decimal?>(), It.IsAny<decimal?>(), 
                It.IsAny<string>(), It.IsAny<string>(), query.Page, query.PageSize, ct))
                .ReturnsAsync((expenses, 2));

            // When
            var result = await _service.GetAllAsync(query, ct);

            // Then
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            _moduleAccessMock.Verify(m => m.EnsureExpensesEnabledAsync(ct), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenCategoryNotFound()
        {
            // Given
            var dto = new CreateExpenseDto { CategoryId = 99, Status = "Paid", PaymentMethod = "Cash" };
            var ct = CancellationToken.None;

            _categoryRepoMock.Setup(r => r.GetByIdAsync(_companyId, dto.CategoryId, ct))
                .ReturnsAsync((ExpenseCategory?)null);

            // When
            var act = () => _service.CreateAsync(dto, ct);

            // Then
            await act.Should().ThrowAsync<BusinessException>()
                .Where(e => e.Code == "EXPENSE_CATEGORY_NOT_FOUND");
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnCreatedExpense_WhenValid()
        {
            // Given
            var dto = new CreateExpenseDto 
            { 
                CategoryId = 1, 
                Status = "Paid", 
                PaymentMethod = "Cash",
                Amount = 100,
                Description = "Test Expense"
            };
            var ct = CancellationToken.None;
            var category = new ExpenseCategory { Id = 1, Name = "Office" };
            var expense = new Expense 
            { 
                Id = 1, 
                ExpenseCategoryId = 1, 
                Amount = 100, 
                Description = "Test Expense", 
                Status = ExpenseStatus.Paid, 
                PaymentMethod = PaymentMethod.Cash 
            };

            _categoryRepoMock.Setup(r => r.GetByIdAsync(_companyId, dto.CategoryId, ct))
                .ReturnsAsync(category);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Expense>(), ct))
                .ReturnsAsync(expense);

            // When
            var result = await _service.CreateAsync(dto, ct);

            // Then
            result.Description.Should().Be(dto.Description);
            result.Amount.Should().Be(dto.Amount);
            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Expense>(), ct), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenExpenseNotFound()
        {
            // Given
            var dto = new UpdateExpenseDto { CategoryId = 1 };
            var ct = CancellationToken.None;

            _repoMock.Setup(r => r.GetByIdForUpdateAsync(_companyId, 1, ct))
                .ReturnsAsync((Expense?)null);

            // When
            var act = () => _service.UpdateAsync(1, dto, ct);

            // Then
            await act.Should().ThrowAsync<BusinessException>()
                .Where(e => e.Code == "EXPENSE_NOT_FOUND");
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallSoftDelete_WhenExists()
        {
            // Given
            var expenseId = 1;
            var ct = CancellationToken.None;
            var expense = new Expense { Id = expenseId, CompanyId = _companyId };

            _repoMock.Setup(r => r.GetByIdForUpdateAsync(_companyId, expenseId, ct))
                .ReturnsAsync(expense);

            // When
            await _service.DeleteAsync(expenseId, ct);

            // Then
            _repoMock.Verify(r => r.SoftDeleteAsync(expense, ct), Times.Once);
        }
    }
}
