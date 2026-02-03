using ERPSystem.Domain.Entities.Products;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace ERPSystem.Tests.Unit.Repositories
{
    public class UnitOfMeasureRepositoryTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetAllByCompanyAsync_ShouldReturnCompanyUnits()
        {
            // Given
            var context = CreateContext();
            var repository = new UnitOfMeasureRepository(context);
            context.UnitsOfMeasure.AddRange(new List<UnitOfMeasure>
            {
                new UnitOfMeasure { Id = 1, Name = "Unit 1", Symbol = "U1", CompanyId = 1 },
                new UnitOfMeasure { Id = 2, Name = "Unit 2", Symbol = "U2", CompanyId = 1 },
                new UnitOfMeasure { Id = 3, Name = "Unit 3", Symbol = "U3", CompanyId = 2 }
            });
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetAllByCompanyAsync(1);

            // Then
            result.Should().HaveCount(2);
            result.All(u => u.CompanyId == 1).Should().BeTrue();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUnit()
        {
            // Given
            var context = CreateContext();
            var repository = new UnitOfMeasureRepository(context);
            context.UnitsOfMeasure.Add(new UnitOfMeasure { Id = 1, Name = "Unit 1", Symbol = "U1", CompanyId = 1 });
            await context.SaveChangesAsync();

            // When
            var result = await repository.GetByIdAsync(1);

            // Then
            result.Should().NotBeNull();
            result!.Name.Should().Be("Unit 1");
        }

        [Fact]
        public async Task Delete_ShouldMarkAsDeleted()
        {
            // Given
            var context = CreateContext();
            var repository = new UnitOfMeasureRepository(context);
            var unit = new UnitOfMeasure { Id = 1, Name = "Unit 1", Symbol = "U1", CompanyId = 1 };
            context.UnitsOfMeasure.Add(unit);
            await context.SaveChangesAsync();

            // When
            repository.Delete(unit);
            await repository.SaveChangesAsync();

            // Then
            var deleted = await context.UnitsOfMeasure.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == 1);
            deleted!.IsDeleted.Should().BeTrue();
        }
    }
}
