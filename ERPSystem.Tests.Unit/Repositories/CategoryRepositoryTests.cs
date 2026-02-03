using ERPSystem.Domain.Entities.Products;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace ERPSystem.Tests.Unit.Repositories
{
    public class CategoryRepositoryTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetAllByCompanyAsync_ShouldReturnCompanyCategories()
        {
            // Given
            var context = CreateContext();
            var repository = new CategoryRepository(context);
            context.Categories.AddRange(new List<Category>
            {
                new Category { Id = 1, Name = "C1", CompanyId = 1 },
                new Category { Id = 2, Name = "C2", CompanyId = 1 },
                new Category { Id = 3, Name = "C3", CompanyId = 2 }
            });
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetAllByCompanyAsync(1);

            // Then
            result.Should().HaveCount(2);
            result.All(c => c.CompanyId == 1).Should().BeTrue();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCategoryWithParent()
        {
            // Given
            var context = CreateContext();
            var repository = new CategoryRepository(context);
            var parent = new Category { Id = 1, Name = "Parent", CompanyId = 1 };
            var child = new Category { Id = 2, Name = "Child", CompanyId = 1, ParentCategoryId = 1 };
            context.Categories.AddRange(parent, child);
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetByIdAsync(2);

            // Then
            result.Should().NotBeNull();
            result!.ParentCategory.Should().NotBeNull();
            result.ParentCategory!.Name.Should().Be("Parent");
        }

        [Fact]
        public async Task Delete_ShouldMarkAsDeleted()
        {
            // Given
            var context = CreateContext();
            var repository = new CategoryRepository(context);
            var category = new Category { Id = 1, Name = "C1", CompanyId = 1 };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            // When
            repository.Delete(category);
            await repository.SaveChangesAsync();

            // Then
            var deleted = await context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == 1);
            deleted!.IsDeleted.Should().BeTrue();
        }
    }
}
