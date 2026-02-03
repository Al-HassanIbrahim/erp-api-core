using Moq;
using FluentAssertions;
using ERPSystem.Application.Services.Core;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Application.Exceptions;

namespace ERPSystem.Tests.Unit.Services
{
    public class ModuleAccessServiceTests
    {
        private readonly Mock<ICompanyModuleRepository> _repoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly ModuleAccessService _service;
        private readonly int _companyId = 1;

        public ModuleAccessServiceTests()
        {
            _repoMock = new Mock<ICompanyModuleRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();

            _currentUserMock.Setup(x => x.CompanyId).Returns(_companyId);

            _service = new ModuleAccessService(
                _repoMock.Object,
                _currentUserMock.Object);
        }

        [Fact]
        public async Task IsSalesEnabledAsync_ShouldReturnTrue_WhenEnabled()
        {
            // Given
            var ct = CancellationToken.None;
            _repoMock.Setup(r => r.IsModuleEnabledAsync(_companyId, "SALES", ct))
                .ReturnsAsync(true);

            // When
            var result = await _service.IsSalesEnabledAsync(ct);

            // Then
            result.Should().BeTrue();
        }

        [Fact]
        public async Task EnsureExpensesEnabledAsync_ShouldThrowException_WhenDisabled()
        {
            // Given
            var ct = CancellationToken.None;
            _repoMock.Setup(r => r.IsModuleEnabledAsync(_companyId, "EXPENSES", ct))
                .ReturnsAsync(false);

            // When
            var act = () => _service.EnsureExpensesEnabledAsync(ct);

            // Then
            await act.Should().ThrowAsync<BusinessException>()
                .Where(e => e.Code == "EXPENSES_MODULE_NOT_ENABLED");
        }
    }
}
