using Moq;
using FluentAssertions;
using ERPSystem.Application.Services.Core;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Core;
using ERPSystem.Application.DTOs.Core;

namespace ERPSystem.Tests.Unit.Services
{
    public class ModuleServiceTests
    {
        private readonly Mock<IModuleRepository> _repoMock;
        private readonly ModuleService _service;

        public ModuleServiceTests()
        {
            _repoMock = new Mock<IModuleRepository>();
            _service = new ModuleService(_repoMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllModules()
        {
            // Given
            var ct = CancellationToken.None;
            var modules = new List<Module>
            {
                new Module { Id = 1, Key = "M1", Name = "Module 1" },
                new Module { Id = 2, Key = "M2", Name = "Module 2" }
            };
            _repoMock.Setup(r => r.GetAllAsync(ct)).ReturnsAsync(modules);

            // When
            var result = await _service.GetAllAsync(ct);

            // Then
            result.Should().HaveCount(2);
            result[0].Key.Should().Be("M1");
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateAndReturnModule()
        {
            // Given
            var ct = CancellationToken.None;
            var dto = new CreateModuleDto { Key = "M1", Name = "Module 1", IsActive = true };
            _repoMock.Setup(r => r.KeyExistsAsync("M1", null, ct)).ReturnsAsync(false);

            // When
            var result = await _service.CreateAsync(dto, ct);

            // Then
            result.Key.Should().Be("M1");
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Module>(), ct), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }
    }
}
