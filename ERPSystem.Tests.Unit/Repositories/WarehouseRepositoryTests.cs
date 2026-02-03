using ERPSystem.Domain.Entities.Inventory;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories.Inventory;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace ERPSystem.Tests.Unit.Repositories
{
    public class WarehouseRepositoryTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnCompanyWarehouses()
        {
            // Given
            var context = CreateContext();
            var repository = new WarehouseRepository(context);
            context.Warehouses.AddRange(new List<Warehouse>
            {
                new Warehouse { Id = 1, Name = "W1", Code = "W1", CompanyId = 1 },
                new Warehouse { Id = 2, Name = "W2", Code = "W2", CompanyId = 1 },
                new Warehouse { Id = 3, Name = "W3", Code = "W3", CompanyId = 2 }
            });
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetAllAsync(1);

            // Then
            result.Should().HaveCount(2);
            result.All(w => w.CompanyId == 1).Should().BeTrue();
        }

        [Fact]
        public async Task HasInventoryActivityAsync_ShouldReturnTrue_WhenStockExists()
        {
            // Given
            var context = CreateContext();
            var repository = new WarehouseRepository(context);
            context.Warehouses.Add(new Warehouse { Id = 1, Name = "W1", Code = "W1", CompanyId = 1 });
            context.StockItems.Add(new StockItem { Id = 1, WarehouseId = 1, ProductId = 1, QuantityOnHand = 10, CompanyId = 1 });
            await context.SaveChangesAsync();

            // When
            var hasActivity = await repository.HasInventoryActivityAsync(1);

            // Then
            hasActivity.Should().BeTrue();
        }

        [Fact]
        public async Task HasInventoryActivityAsync_ShouldReturnFalse_WhenNoActivity()
        {
            // Given
            var context = CreateContext();
            var repository = new WarehouseRepository(context);
            context.Warehouses.Add(new Warehouse { Id = 1, Name = "W1", Code = "W1", CompanyId = 1 });
            await context.SaveChangesAsync();

            // When
            var hasActivity = await repository.HasInventoryActivityAsync(1);

            // Then
            hasActivity.Should().BeFalse();
        }

        [Fact]
        public async Task CodeExistsAsync_ShouldCheckExcludeId()
        {
            // Given
            var context = CreateContext();
            var repository = new WarehouseRepository(context);
            context.Warehouses.Add(new Warehouse { Id = 1, Name = "W1", Code = "DUP", CompanyId = 1 });
            await context.SaveChangesAsync();

            // When
            var existsSelf = await repository.CodeExistsAsync("DUP", 1, excludeId: 1);
            var existsOther = await repository.CodeExistsAsync("DUP", 1, excludeId: 2);

            // Then
            existsSelf.Should().BeFalse();
            existsOther.Should().BeTrue();
        }
    }
}
