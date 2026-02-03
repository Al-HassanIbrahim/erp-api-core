using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories.Sales;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace ERPSystem.Tests.Unit.Repositories
{
    public class SalesReceiptRepositoryTests
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
            var repository = new SalesReceiptRepository(context);
            context.Customers.Add(new Customer { Id = 1, Name = "C1", CompanyId = 1, Code = "C1" });
            context.SalesReceipts.AddRange(new List<SalesReceipt>
            {
                new SalesReceipt { Id = 1, ReceiptNumber = "R1", CompanyId = 1, CustomerId = 1, Status = SalesReceiptStatus.Draft },
                new SalesReceipt { Id = 2, ReceiptNumber = "R2", CompanyId = 1, CustomerId = 1, Status = SalesReceiptStatus.Posted },
                new SalesReceipt { Id = 3, ReceiptNumber = "R3", CompanyId = 2, CustomerId = 2 }
            });
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetAllByCompanyAsync(1, status: SalesReceiptStatus.Draft);

            // Then
            result.Should().HaveCount(1);
            result[0].Id.Should().Be(1);
        }

        [Fact]
        public async Task GetByIdWithAllocationsAsync_ShouldIncludeRelatedData()
        {
            // Given
            var context = CreateContext();
            var repository = new SalesReceiptRepository(context);
            context.Customers.Add(new Customer { Id = 1, Name = "C1", CompanyId = 1, Code = "C1" });
            context.SalesInvoices.Add(new SalesInvoice { Id = 1, InvoiceNumber = "INV1", CompanyId = 1, CustomerId = 1 });
            var receipt = new SalesReceipt { Id = 1, ReceiptNumber = "R1", CompanyId = 1, CustomerId = 1 };
            receipt.Allocations.Add(new SalesReceiptAllocation { Id = 1, SalesInvoiceId = 1, AllocatedAmount = 100 });
            context.SalesReceipts.Add(receipt);
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetByIdWithAllocationsAsync(1);

            // Then
            result.Should().NotBeNull();
            result!.Allocations.Should().HaveCount(1);
            result.Allocations.First().SalesInvoice.Should().NotBeNull();
            result.Customer.Should().NotBeNull();
        }

        [Fact]
        public async Task GenerateReceiptNumberAsync_ShouldIncrement()
        {
            // Given
            var context = CreateContext();
            var repository = new SalesReceiptRepository(context);
            context.SalesReceipts.Add(new SalesReceipt { Id = 1, ReceiptNumber = "R1", CompanyId = 1, CustomerId = 1 });
            await context.SaveChangesAsync();

            // When
            var number = await repository.GenerateReceiptNumberAsync(1);

            // Then
            number.Should().Contain("-00002");
        }
    }
}
