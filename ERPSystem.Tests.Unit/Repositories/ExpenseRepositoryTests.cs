using ERPSystem.Domain.Entities.Expenses;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories.Expenses;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Tests.Unit.Repositories
{
    public class ExpenseRepositoryTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetPagedAsync_ShouldReturnPagedExpenses()
        {
            // Given
            var context = CreateContext();
            var repository = new ExpenseRepository(context);
            var category = new ExpenseCategory { Id = 1, Name = "Cat 1", CompanyId = 1 };
            context.ExpenseCategories.Add(category);
            
            context.Expenses.AddRange(new List<Expense>
            {
                new Expense { Id = 1, Description = "Exp 1", Amount = 100, ExpenseDate = DateTime.UtcNow, CompanyId = 1, ExpenseCategoryId = 1, Status = ExpenseStatus.Paid },
                new Expense { Id = 2, Description = "Exp 2", Amount = 200, ExpenseDate = DateTime.UtcNow, CompanyId = 1, ExpenseCategoryId = 1, Status = ExpenseStatus.Paid },
                new Expense { Id = 3, Description = "Other", Amount = 300, ExpenseDate = DateTime.UtcNow, CompanyId = 2, ExpenseCategoryId = 1, Status = ExpenseStatus.Paid }
            });
            await context.SaveChangesAsync();

            // When
            var (items, totalCount) = await repository.GetPagedAsync(1, null, null, null, null, null, null, null, null, null, null, 1, 10, CancellationToken.None);

            // Then
            totalCount.Should().Be(2);
            items.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetSummaryAsync_ShouldReturnCorrectStats()
        {
            // Given
            var context = CreateContext();
            var repository = new ExpenseRepository(context);
            context.Expenses.AddRange(new List<Expense>
            {
                new Expense { Id = 1, Description = "E1", Amount = 100, ExpenseDate = DateTime.UtcNow, CompanyId = 1, ExpenseCategoryId = 1 },
                new Expense { Id = 2, Description = "E2", Amount = 500, ExpenseDate = DateTime.UtcNow, CompanyId = 1, ExpenseCategoryId = 1 }
            });
            await context.SaveChangesAsync();

            // When
            var (total, max, min, count) = await repository.GetSummaryAsync(1, null, null, CancellationToken.None);

            // Then
            total.Should().Be(600);
            max.Should().Be(500);
            min.Should().Be(100);
            count.Should().Be(2);
        }

        [Fact]
        public async Task GetDailyTotalsAsync_ShouldGroupByDate()
        {
            // Given
            var context = CreateContext();
            var repository = new ExpenseRepository(context);
            var date1 = new DateTime(2023, 1, 1);
            var date2 = new DateTime(2023, 1, 2);
            
            context.Expenses.AddRange(new List<Expense>
            {
                new Expense { Id = 1, Description = "E1", Amount = 100, ExpenseDate = date1, CompanyId = 1, ExpenseCategoryId = 1 },
                new Expense { Id = 2, Description = "E2", Amount = 200, ExpenseDate = date1, CompanyId = 1, ExpenseCategoryId = 1 },
                new Expense { Id = 3, Description = "E3", Amount = 500, ExpenseDate = date2, CompanyId = 1, ExpenseCategoryId = 1 }
            });
            await context.SaveChangesAsync();

            // When
            var results = await repository.GetDailyTotalsAsync(1, null, null, CancellationToken.None);

            // Then
            results.Should().HaveCount(2);
            results.First(r => r.Date == date1).Amount.Should().Be(300);
            results.First(r => r.Date == date2).Amount.Should().Be(500);
        }
    }
}
