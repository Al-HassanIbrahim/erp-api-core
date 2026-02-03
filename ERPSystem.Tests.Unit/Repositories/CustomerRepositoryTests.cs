using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories.Sales;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace ERPSystem.Tests.Unit.Repositories
{
    public class CustomerRepositoryTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCustomer_WhenExists()
        {
            // Given
            var context = CreateContext();
            var repository = new CustomerRepository(context);
            var customer = new Customer { Id = 1, Name = "Customer 1", Code = "C1", CompanyId = 1 };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetByIdAsync(1);

            // Then
            result.Should().NotBeNull();
            result!.Name.Should().Be("Customer 1");
        }

        [Fact]
        public async Task GetAllByCompanyAsync_ShouldReturnFilteredCustomers()
        {
            // Given
            var context = CreateContext();
            var repository = new CustomerRepository(context);
            context.Customers.AddRange(new List<Customer>
            {
                new Customer { Id = 1, Name = "C1", Code = "C1", CompanyId = 1, IsActive = true },
                new Customer { Id = 2, Name = "C2", Code = "C2", CompanyId = 1, IsActive = false },
                new Customer { Id = 3, Name = "C3", Code = "C3", CompanyId = 2, IsActive = true }
            });
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetAllByCompanyAsync(1, isActive: true);

            // Then
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("C1");
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenCodeExists()
        {
            // Given
            var context = CreateContext();
            var repository = new CustomerRepository(context);
            context.Customers.Add(new Customer { Id = 1, Name = "C1", Code = "DUPE", CompanyId = 1 });
            await context.SaveChangesAsync();

            // When
            var exists = await repository.ExistsAsync(1, "DUPE");

            // Then
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_ShouldMarkAsDeleted()
        {
            // Given
            var context = CreateContext();
            var repository = new CustomerRepository(context);
            var customer = new Customer { Id = 1, Name = "C1", Code = "C1", CompanyId = 1 };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            // When
            repository.Delete(customer);
            await repository.SaveChangesAsync();

            // Then
            var deletedCustomer = await context.Customers.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == 1);
            deletedCustomer!.IsDeleted.Should().BeTrue();
        }
    }
}
