using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories.Sales;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using ERPSystem.Domain.Entities.Products;

namespace ERPSystem.Tests.Unit.Repositories
{
    public class SalesInvoiceRepositoryTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetByIdWithLinesAsync_ShouldReturnInvoiceAndLines()
        {
            // Given
            var context = CreateContext();
            var repository = new SalesInvoiceRepository(context);
            var customer = new Customer { Id = 1, Name = "C1", Code = "C1", CompanyId = 1 };
            var product = new Product { Id = 1, Name = "P1", Code = "P1", CompanyId = 1, UnitOfMeasureId = 1 };
            var uom = new UnitOfMeasure { Id = 1, Name = "U1", Symbol = "U1", CompanyId = 1 };
            context.Customers.Add(customer);
            context.Products.Add(product);
            context.UnitsOfMeasure.Add(uom);

            var invoice = new SalesInvoice
            {
                Id = 1,
                InvoiceNumber = "INV-001",
                CompanyId = 1,
                CustomerId = 1,
                Lines = new List<SalesInvoiceLine>
                {
                    new SalesInvoiceLine { Id = 1, ProductId = 1, UnitId = 1, Quantity = 10 }
                }
            };
            context.SalesInvoices.Add(invoice);
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetByIdWithLinesAsync(1);

            // Then
            result.Should().NotBeNull();
            result!.Lines.Should().HaveCount(1);
            result.Lines.First().Product.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllByCompanyAsync_ShouldFilterByStatus()
        {
            // Given
            var context = CreateContext();
            var repository = new SalesInvoiceRepository(context);
            var customer = new Customer { Id = 1, Name = "C1", Code = "C1", CompanyId = 1 };
            context.Customers.Add(customer);
            
            context.SalesInvoices.AddRange(new List<SalesInvoice>
            {
                new SalesInvoice { Id = 1, InvoiceNumber = "INV-1", CompanyId = 1, CustomerId = 1, Status = SalesInvoiceStatus.Draft },
                new SalesInvoice { Id = 2, InvoiceNumber = "INV-2", CompanyId = 1, CustomerId = 1, Status = SalesInvoiceStatus.Posted }
            });
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetAllByCompanyAsync(1, status: SalesInvoiceStatus.Posted);

            // Then
            result.Should().HaveCount(1);
            result[0].Status.Should().Be(SalesInvoiceStatus.Posted);
        }

        [Fact]
        public async Task GenerateInvoiceNumberAsync_ShouldIncrementCount()
        {
            // Given
            var context = CreateContext();
            var repository = new SalesInvoiceRepository(context);
            context.SalesInvoices.Add(new SalesInvoice { Id = 1, InvoiceNumber = "INV-1", CompanyId = 1, CustomerId = 1 });
            await context.SaveChangesAsync();

            // When
            var number = await repository.GenerateInvoiceNumberAsync(1);

            // Then
            number.Should().EndWith("00002");
        }
    }
}
