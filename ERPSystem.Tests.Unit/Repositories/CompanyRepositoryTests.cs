using ERPSystem.Domain.Entities.Core;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories.Core;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;

namespace ERPSystem.Tests.Unit.Repositories
{
    public class CompanyRepositoryTests
    {
        private static AppDbContext CreateContext(string? dbName = null)
        {
            dbName ??= $"CompanyTests_{Guid.NewGuid():N}";

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetByIdAsync_returns_company_when_exists()
        {
            // Arrange
            await using var context = CreateContext();
            var company = new Company 
            { 
                Id = 5, 
                Name = "Test Corp", 
                IsDeleted = false 
            };
            context.Companies.Add(company);
            await context.SaveChangesAsync();

            var repository = new CompanyRepository(context);

            // Act
            var result = await repository.GetByIdAsync(5);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(5);
            result.Name.Should().Be("Test Corp");
            result.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task GetByIdAsync_returns_null_when_company_not_found()
        {
            await using var context = CreateContext();
            var repository = new CompanyRepository(context);

            var result = await repository.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdTrackingAsync_returns_tracked_entity_and_supports_updates()
        {
            await using var context = CreateContext();
            var company = new Company 
            { 
                Id = 7, 
                Name = "Tracked Co", 
                IsDeleted = false 
            };
            context.Companies.Add(company);
            await context.SaveChangesAsync();

            var repository = new CompanyRepository(context);

            var result = await repository.GetByIdTrackingAsync(7);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Tracked Co");

            // Change should be tracked
            result.Name = "Updated Tracked Co";
            await context.SaveChangesAsync();

            var reloaded = await context.Companies.FindAsync(7);
            reloaded.Should().NotBeNull();
            reloaded!.Name.Should().Be("Updated Tracked Co");
        }

        [Fact]
        public async Task GetAllAsync_returns_only_non_deleted_companies_sorted_by_id_descending()
        {
            await using var context = CreateContext();
            context.Companies.AddRange(
                new Company { Id = 1, Name = "Active1", IsDeleted = false },
                new Company { Id = 2, Name = "Deleted",   IsDeleted = true  },
                new Company { Id = 3, Name = "Active2", IsDeleted = false }
            );
            await context.SaveChangesAsync();

            var repository = new CompanyRepository(context);

            var result = await repository.GetAllAsync();

            result.Should().HaveCount(2);
            result.Select(c => c.Name).Should().BeEquivalentTo(new[] { "Active1", "Active2" });
            result.First().Id.Should().Be(3); // descending by Id
        }

        [Fact]
        public async Task AddAsync_persists_new_company_correctly()
        {
            await using var context = CreateContext();
            var repository = new CompanyRepository(context);

            var newCompany = new Company
            {
                Name = "New Corporation",
                CommercialName = "New Corp Ltd",
                Address = "123 Test St",
                IsActive = true,
                IsDeleted = false
            };

            await repository.AddAsync(newCompany);
            await repository.SaveChangesAsync();

            var saved = await context.Companies
                .FirstOrDefaultAsync(c => c.Name == "New Corporation");

            saved.Should().NotBeNull();
            saved!.Name.Should().Be("New Corporation");
            saved.CommercialName.Should().Be("New Corp Ltd");
            saved.IsActive.Should().BeTrue();
            saved.IsDeleted.Should().BeFalse();
        }

        
        [Fact]
        public async Task SoftDeleteAsync_does_nothing_when_company_not_found()
        {
            await using var context = CreateContext();
            var repository = new CompanyRepository(context);

            // No exception expected - just no-op
            await repository.SoftDeleteAsync(999, Guid.NewGuid());

            var anyDeleted = await context.Companies.AnyAsync(c => c.IsDeleted);
            anyDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsAsync_returns_true_when_company_exists()
        {
            await using var context = CreateContext();
            context.Companies.Add(new Company 
            { 
                Id = 42, 
                Name = "Existing Company", 
                IsDeleted = false 
            });
            await context.SaveChangesAsync();

            var repository = new CompanyRepository(context);

            var exists = await repository.ExistsAsync(42);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_returns_false_when_company_does_not_exist_or_is_deleted()
        {
            await using var context = CreateContext();
            context.Companies.Add(new Company 
            { 
                Id = 100, 
                Name = "Deleted One", 
                IsDeleted = true 
            });
            await context.SaveChangesAsync();

            var repository = new CompanyRepository(context);

            var existsActive   = await repository.ExistsAsync(999);
            var existsDeleted  = await repository.ExistsAsync(100);

            existsActive.Should().BeFalse();
            existsDeleted.Should().BeFalse();
        }
    }
}