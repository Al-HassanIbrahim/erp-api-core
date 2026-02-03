using ERPSystem.Domain.Entities.Expenses;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories.Expenses;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace ERPSystem.Tests.Unit.Repositories
{
    public class ExpenseCategoryRepositoryTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnCompanyCategories()
        {
            // Given
            var context = CreateContext();
            var repository = new ExpenseCategoryRepository(context);
            context.ExpenseCategories.AddRange(new List<ExpenseCategory>
            {
                new ExpenseCategory { Id = 1, Name = "Cat 1", CompanyId = 1 },
                new ExpenseCategory { Id = 2, Name = "Cat 2", CompanyId = 1 },
                new ExpenseCategory { Id = 3, Name = "Cat 3", CompanyId = 2 }
            });
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetAllAsync(1, CancellationToken.None);

            // Then
            result.Should().HaveCount(2);
            result.All(c => c.CompanyId == 1).Should().BeTrue();
        }

        [Fact]
        public async Task GetByNameAsync_ShouldBeCaseInsensitive()
        {
            // Given
            var context = CreateContext();
            var repository = new ExpenseCategoryRepository(context);
            context.ExpenseCategories.Add(new ExpenseCategory { Id = 1, Name = "Office", CompanyId = 1 });
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetByNameAsync(1, "OFFICE", CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            result!.Name.Should().Be("Office");
        }

        [Fact]
        public async Task GetCategoryStatsAsync_ShouldReturnCorrectStats()
        {
            // Given
            var context = CreateContext();
            var repository = new ExpenseCategoryRepository(context);
            context.ExpenseCategories.Add(new ExpenseCategory { Id = 1, Name = "Cat 1", CompanyId = 1 });
            context.Expenses.AddRange(new List<Expense>
            {
                new Expense { Id = 1, ExpenseCategoryId = 1, Amount = 100, CompanyId = 1, Description = "E1" },
                new Expense { Id = 2, ExpenseCategoryId = 1, Amount = 200, CompanyId = 1, Description = "E2" },
                new Expense { Id = 3, ExpenseCategoryId = 2, Amount = 300, CompanyId = 1, Description = "E3" }
            });
            await context.SaveChangesAsync();

            // When
            var (count, total) = await repository.GetCategoryStatsAsync(1, 1, CancellationToken.None);

            // Then
            count.Should().Be(2);
            total.Should().Be(300);
        }

        [Fact]
        public async Task SoftDeleteAsync_ShouldMarkAsDeleted()
        {
            // Given
            var context = CreateContext();
            var repository = new ExpenseCategoryRepository(context);
            var category = new ExpenseCategory { Id = 1, Name = "Cat 1", CompanyId = 1 };
            context.ExpenseCategories.Add(category);
            await context.SaveChangesAsync();

            // When
            await repository.SoftDeleteAsync(category, CancellationToken.None);

            // Then
            var deleted = await context.ExpenseCategories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == 1);
            deleted!.IsDeleted.Should().BeTrue();
        }
    }
}
