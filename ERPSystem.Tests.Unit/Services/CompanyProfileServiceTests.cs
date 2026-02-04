using Moq;
using FluentAssertions;
using ERPSystem.Application.Services.Core;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Entities.Core;
using ERPSystem.Application.DTOs.Core;

namespace ERPSystem.Tests.Unit.Services
{
    public class CompanyProfileServiceTests
    {
        private readonly Mock<ICompanyRepository> _repoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly CompanyProfileService _service;
        private readonly int _companyId = 1;

        public CompanyProfileServiceTests()
        {
            _repoMock = new Mock<ICompanyRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();

            _currentUserMock.Setup(x => x.CompanyId).Returns(_companyId);

            _service = new CompanyProfileService(
                _repoMock.Object,
                _currentUserMock.Object);
        }

        [Fact]
        public async Task GetMyCompanyAsync_ShouldReturnCompany_WhenExists()
        {
            // Given
            var ct = CancellationToken.None;
            var company = new Company { Id = _companyId, Name = "Test Company", IsActive = true };
            _repoMock.Setup(r => r.GetByIdAsync(_companyId, ct)).ReturnsAsync(company);

            // When
            var result = await _service.GetMyCompanyAsync(ct);

            // Then
            result.Should().NotBeNull();
            result!.Name.Should().Be("Test Company");
        }

        [Fact]
        public async Task UpdateMyCompanyAsync_ShouldUpdateAndReturnCompany()
        {
            // Given
            var ct = CancellationToken.None;
            var company = new Company { Id = _companyId, Name = "Old Name" };
            var updateDto = new UpdateCompanyMeDto { Name = "New Name", CommercialName = "New Comm", Address = "New Addr" };
            
            _repoMock.Setup(r => r.GetByIdTrackingAsync(_companyId, ct)).ReturnsAsync(company);

            // When
            var result = await _service.UpdateMyCompanyAsync(updateDto, ct);

            // Then
            result.Name.Should().Be("New Name");
            company.Name.Should().Be("New Name");
            _repoMock.Verify(r => r.Update(company), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }
    }
}
