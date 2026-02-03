using Moq;
using FluentAssertions;
using ERPSystem.Application.Services.Products;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Entities.Products;
using ERPSystem.Application.DTOs.Products;

namespace ERPSystem.Tests.Unit.Services
{
    public class UnitOfMeasureServiceTests
    {
        private readonly Mock<IUnitOfMeasureRepository> _repoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly UnitOfMeasureService _service;
        private readonly int _companyId = 1;

        public UnitOfMeasureServiceTests()
        {
            _repoMock = new Mock<IUnitOfMeasureRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();

            _currentUserMock.Setup(x => x.CompanyId).Returns(_companyId);

            _service = new UnitOfMeasureService(
                _repoMock.Object,
                _currentUserMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnUnits()
        {
            // Given
            var ct = CancellationToken.None;
            var units = new List<UnitOfMeasure>
            {
                new UnitOfMeasure { Id = 1, Name = "Kilogram", Symbol = "kg", CompanyId = _companyId },
                new UnitOfMeasure { Id = 2, Name = "Piece", Symbol = "pcs", CompanyId = _companyId }
            };

            _repoMock.Setup(r => r.GetAllByCompanyAsync(_companyId, ct))
                .ReturnsAsync(units);

            // When
            var result = await _service.GetAllAsync(ct);

            // Then
            result.Should().HaveCount(2);
            result[0].Symbol.Should().Be("kg");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUnit_WhenExists()
        {
            // Given
            var ct = CancellationToken.None;
            var unit = new UnitOfMeasure { Id = 1, Name = "Kilogram", Symbol = "kg", CompanyId = _companyId };

            _repoMock.Setup(r => r.GetByIdAsync(1, ct))
                .ReturnsAsync(unit);

            // When
            var result = await _service.GetByIdAsync(1, ct);

            // Then
            result.Should().NotBeNull();
            result!.Name.Should().Be("Kilogram");
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnCreatedUnit()
        {
            // Given
            var request = new CreateUnitOfMeasureRequest { Name = "Liter", Symbol = "L" };
            var ct = CancellationToken.None;

            // When
            var result = await _service.CreateAsync(request, ct);

            // Then
            result.Name.Should().Be("Liter");
            _repoMock.Verify(r => r.AddAsync(It.IsAny<UnitOfMeasure>(), ct), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenNotFound()
        {
            // Given
            var request = new UpdateUnitOfMeasureRequest { Name = "New Name", Symbol = "NN" };
            var ct = CancellationToken.None;

            _repoMock.Setup(r => r.GetByIdAsync(1, ct))
                .ReturnsAsync((UnitOfMeasure?)null);

            // When
            var act = () => _service.UpdateAsync(1, request, ct);

            // Then
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Unit of measure not found.");
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDelete_WhenExists()
        {
            // Given
            var ct = CancellationToken.None;
            var unit = new UnitOfMeasure { Id = 1, CompanyId = _companyId };

            _repoMock.Setup(r => r.GetByIdAsync(1, ct))
                .ReturnsAsync(unit);

            // When
            await _service.DeleteAsync(1, ct);

            // Then
            _repoMock.Verify(r => r.Delete(unit), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(ct), Times.Once);
        }
    }
}
