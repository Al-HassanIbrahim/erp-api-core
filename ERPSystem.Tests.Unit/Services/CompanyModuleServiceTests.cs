using Moq;
using FluentAssertions;
using ERPSystem.Application.Services.Core;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Entities.Core;
using ERPSystem.Application.DTOs.Core;

namespace ERPSystem.Tests.Unit.Services
{
    public class CompanyModuleServiceTests
    {
        private readonly Mock<ICompanyModuleRepository> _companyModuleRepoMock;
        private readonly Mock<IModuleRepository> _moduleRepoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly CompanyModuleService _service;
        private readonly int _companyId = 1;

        public CompanyModuleServiceTests()
        {
            _companyModuleRepoMock = new Mock<ICompanyModuleRepository>();
            _moduleRepoMock = new Mock<IModuleRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();

            _currentUserMock.Setup(x => x.CompanyId).Returns(_companyId);

            _service = new CompanyModuleService(
                _companyModuleRepoMock.Object,
                _moduleRepoMock.Object,
                _currentUserMock.Object);
        }

        [Fact]
        public async Task GetMyCompanyModulesAsync_ShouldReturnAllModulesWithEnabledStatus()
        {
            // Given
            var ct = CancellationToken.None;
            var allModules = new List<Module>
            {
                new Module { Id = 1, Key = "M1", Name = "Module 1" },
                new Module { Id = 2, Key = "M2", Name = "Module 2" }
            };
            var companyModules = new List<CompanyModule>
            {
                new CompanyModule { ModuleId = 1, IsEnabled = true }
            };

            _moduleRepoMock.Setup(r => r.GetAllAsync(ct)).ReturnsAsync(allModules);
            _companyModuleRepoMock.Setup(r => r.GetByCompanyAsync(_companyId, ct)).ReturnsAsync(companyModules);

            // When
            var result = await _service.GetMyCompanyModulesAsync(ct);

            // Then
            result.Should().HaveCount(2);
            result.First(m => m.ModuleId == 1).IsEnabled.Should().BeTrue();
            result.First(m => m.ModuleId == 2).IsEnabled.Should().BeFalse();
        }

        [Fact]
        public async Task ToggleModuleAsync_ShouldEnableModule()
        {
            // Given
            var ct = CancellationToken.None;
            var moduleId = 1;
            var module = new Module { Id = moduleId, Key = "M1", Name = "Module 1", IsActive = true };
            _moduleRepoMock.Setup(r => r.GetByIdAsync(moduleId, ct)).ReturnsAsync(module);
            _companyModuleRepoMock.Setup(r => r.GetAsync(_companyId, moduleId, ct))
                .ReturnsAsync(new CompanyModule { ModuleId = moduleId, IsEnabled = true });

            // When
            var result = await _service.ToggleModuleAsync(moduleId, true, ct);

            // Then
            result.IsEnabled.Should().BeTrue();
            _companyModuleRepoMock.Verify(r => r.EnableAsync(_companyId, moduleId, It.IsAny<Guid>(), ct), Times.Once);
            _companyModuleRepoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task ToggleModuleAsync_ShouldDisableModule()
        {
            // Given
            var ct = CancellationToken.None;
            var moduleId = 1;
            var module = new Module { Id = moduleId, Key = "M1", Name = "Module 1", IsActive = true };
            _moduleRepoMock.Setup(r => r.GetByIdAsync(moduleId, ct)).ReturnsAsync(module);
            _companyModuleRepoMock.Setup(r => r.GetAsync(_companyId, moduleId, ct))
                .ReturnsAsync(new CompanyModule { ModuleId = moduleId, IsEnabled = false });

            // When
            var result = await _service.ToggleModuleAsync(moduleId, false, ct);

            // Then
            result.IsEnabled.Should().BeFalse();
            _companyModuleRepoMock.Verify(r => r.DisableAsync(_companyId, moduleId, It.IsAny<Guid>(), ct), Times.Once);
            _companyModuleRepoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }
    }
}
