using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Entities.Inventory;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories.Sales;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace ERPSystem.Tests.Unit.Repositories
{
    public class SalesDeliveryRepositoryTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetAllByCompanyAsync_ShouldFilterCorrectly()
        {
            // Given
            var context = CreateContext();
            var repository = new SalesDeliveryRepository(context);
            context.Customers.Add(new Customer { Id = 1, Name = "C1", CompanyId = 1, Code = "C1" });
            context.Warehouses.Add(new Warehouse { Id = 1, Name = "W1", CompanyId = 1, Code = "W1" });
            context.SalesInvoices.Add(new SalesInvoice { Id = 1, InvoiceNumber = "INV1", CompanyId = 1, CustomerId = 1 });
            
            context.SalesDeliveries.AddRange(new List<SalesDelivery>
            {
                new SalesDelivery { Id = 1, DeliveryNumber = "D1", CompanyId = 1, SalesInvoiceId = 1, CustomerId = 1, WarehouseId = 1, Status = SalesDeliveryStatus.Draft },
                new SalesDelivery { Id = 2, DeliveryNumber = "D2", CompanyId = 1, SalesInvoiceId = 1, CustomerId = 1, WarehouseId = 1, Status = SalesDeliveryStatus.Posted },
                new SalesDelivery { Id = 3, DeliveryNumber = "D3", CompanyId = 2, SalesInvoiceId = 2, CustomerId = 2, WarehouseId = 2 }
            });
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetAllByCompanyAsync(1, status: SalesDeliveryStatus.Draft);

            // Then
            result.Should().HaveCount(1);
            result[0].Id.Should().Be(1);
        }

        [Fact]
        public async Task GetByIdWithLinesAsync_ShouldIncludeRelatedData()
        {
            // Given
            var context = CreateContext();
            var repository = new SalesDeliveryRepository(context);
            context.Customers.Add(new Customer { Id = 1, Name = "C1", CompanyId = 1, Code = "C1" });
            context.Warehouses.Add(new Warehouse { Id = 1, Name = "W1", CompanyId = 1, Code = "W1" });
            context.SalesInvoices.Add(new SalesInvoice { Id = 1, InvoiceNumber = "INV1", CompanyId = 1, CustomerId = 1 });
            var delivery = new SalesDelivery { Id = 1, DeliveryNumber = "D1", CompanyId = 1, SalesInvoiceId = 1, CustomerId = 1, WarehouseId = 1 };
            delivery.Lines.Add(new SalesDeliveryLine { Id = 1, ProductId = 1, Quantity = 10, UnitId = 1 });
            context.SalesDeliveries.Add(delivery);
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetByIdWithLinesAsync(1);

            // Then
            result.Should().NotBeNull();
            result!.Lines.Should().HaveCount(1);
            result.Customer.Should().NotBeNull();
            result.Warehouse.Should().NotBeNull();
            result.SalesInvoice.Should().NotBeNull();
        }

        [Fact]
        public async Task GenerateDeliveryNumberAsync_ShouldIncrement()
        {
            // Given
            var context = CreateContext();
            var repository = new SalesDeliveryRepository(context);
            context.SalesDeliveries.Add(new SalesDelivery { Id = 1, DeliveryNumber = "D1", CompanyId = 1, CustomerId = 1, WarehouseId = 1, SalesInvoiceId = 1 });
            await context.SaveChangesAsync();

            // When
            var number = await repository.GenerateDeliveryNumberAsync(1);

            // Then
            number.Should().Contain("-00002");
        }
    }
}
