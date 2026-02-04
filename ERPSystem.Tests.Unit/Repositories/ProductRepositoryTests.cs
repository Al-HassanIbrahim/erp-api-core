using ERPSystem.Domain.Entities.Products;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace ERPSystem.Tests.Unit.Repositories
{
    public class ProductRepositoryTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnProduct_WhenExists()
        {
            // Given
            var context = CreateContext();
            var repository = new ProductRepository(context);
            var uom = new UnitOfMeasure { Id = 1, Name = "Unit", Symbol = "U", CompanyId = 1 };
            context.UnitsOfMeasure.Add(uom);
            var product = new Product { Id = 1, Name = "Product 1", Code = "P1", CompanyId = 1, UnitOfMeasureId = 1 };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetByIdAsync(1, 1);

            // Then
            result.Should().NotBeNull();
            result!.Name.Should().Be("Product 1");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenDeleted()
        {
            // Given
            var context = CreateContext();
            var repository = new ProductRepository(context);
            var product = new Product { Id = 1, Name = "Product 1", Code = "P1", CompanyId = 1, UnitOfMeasureId = 1, IsDeleted = true };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetByIdAsync(1, 1);

            // Then
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllByCompanyAsync_ShouldReturnOnlyCompanyProducts()
        {
            // Given
            var context = CreateContext();
            var repository = new ProductRepository(context);
            var uom1 = new UnitOfMeasure { Id = 1, Name = "Unit 1", Symbol = "U1", CompanyId = 1 };
            var uom2 = new UnitOfMeasure { Id = 2, Name = "Unit 2", Symbol = "U2", CompanyId = 2 };
            context.UnitsOfMeasure.AddRange(uom1, uom2);

            context.Products.AddRange(new List<Product>
            {
                new Product { Id = 1, Name = "P1", Code = "C1", CompanyId = 1, UnitOfMeasureId = 1 },
                new Product { Id = 2, Name = "P2", Code = "C2", CompanyId = 1, UnitOfMeasureId = 1 },
                new Product { Id = 3, Name = "P3", Code = "C3", CompanyId = 2, UnitOfMeasureId = 2 }
            });
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetAllByCompanyAsync(1);

            // Then
            result.Should().HaveCount(2);
            result.All(p => p.CompanyId == 1).Should().BeTrue();
        }

        [Fact]
        public async Task CodeExistsAsync_ShouldReturnTrue_WhenCodeExistsInCompany()
        {
            // Given
            var context = CreateContext();
            var repository = new ProductRepository(context);
            context.Products.Add(new Product { Id = 1, Name = "P1", Code = "EX1", CompanyId = 1, UnitOfMeasureId = 1 });
            await context.SaveChangesAsync();

            // When
            var exists = await repository.CodeExistsAsync("EX1", 1);

            // Then
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task CodeExistsAsync_ShouldReturnFalse_WhenCodeExistsInOtherCompany()
        {
            // Given
            var context = CreateContext();
            var repository = new ProductRepository(context);
            context.Products.Add(new Product { Id = 1, Name = "P1", Code = "EX1", CompanyId = 2, UnitOfMeasureId = 1 });
            await context.SaveChangesAsync();

            // When
            var exists = await repository.CodeExistsAsync("EX1", 1);

            // Then
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_ShouldMarkAsDeleted()
        {
            // Given
            var context = CreateContext();
            var repository = new ProductRepository(context);
            var product = new Product { Id = 1, Name = "P1", Code = "C1", CompanyId = 1, UnitOfMeasureId = 1 };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // When
            await repository.DeleteAsync(1);
            await repository.SaveChangesAsync();

            // Then
            var deletedProduct = await context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == 1);
            deletedProduct!.IsDeleted.Should().BeTrue();
        }
    }
}
