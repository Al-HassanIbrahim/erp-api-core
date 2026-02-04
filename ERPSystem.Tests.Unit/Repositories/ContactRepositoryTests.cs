using ERPSystem.Domain.Entities.Contacts;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories.Contacts;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using ERPSystem.Domain.Abstractions;

namespace ERPSystem.Tests.Unit.Repositories
{
    public class ContactRepositoryTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetAllContactsAsync_ShouldReturnCompanyContacts()
        {
            // Given
            var context = CreateContext();
            IContactRepository repository = new ContactRepository(context);
            context.Contacts.AddRange(new List<Contact>
            {
                new Contact { Id = 1, FullName = "C1", Email = "e1@t.com", Company = "Co1", CompanyId = 1 },
                new Contact { Id = 2, FullName = "C2", Email = "e2@t.com", Company = "Co2", CompanyId = 1 },
                new Contact { Id = 3, FullName = "C3", Email = "e3@t.com", Company = "Co3", CompanyId = 2 }
            });
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetAllContactsAsync(1, CancellationToken.None);

            // Then
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnContact_WhenExists()
        {
            // Given
            var context = CreateContext();
            var repository = new ContactRepository(context);
            context.Contacts.Add(new Contact { Id = 1, FullName = "C1", Email = "e1@t.com", Company = "Co1", CompanyId = 1 });
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetByIdAsync(1, 1, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            result!.FullName.Should().Be("C1");
        }
    }
}
