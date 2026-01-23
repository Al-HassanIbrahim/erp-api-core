using ERPSystem.Application.DTOs.Products;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Products;

namespace ERPSystem.Application.Services.Products
{
    public class UnitOfMeasureService : IUnitOfMeasureService
    {
        private readonly IUnitOfMeasureRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public UnitOfMeasureService(
            IUnitOfMeasureRepository repository,
            ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyList<UnitOfMeasureDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var items = await _repository.GetAllByCompanyAsync(_currentUser.CompanyId, cancellationToken);

            return items.Select(u => new UnitOfMeasureDto
            {
                Id = u.Id,
                Name = u.Name,
                Symbol = u.Symbol
            }).ToList();
        }

        public async Task<UnitOfMeasureDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var item = await _repository.GetByIdAsync(id, cancellationToken);

            if (item == null || item.CompanyId != _currentUser.CompanyId)
                return null;

            return new UnitOfMeasureDto
            {
                Id = item.Id,
                Name = item.Name,
                Symbol = item.Symbol
            };
        }

        public async Task<UnitOfMeasureDto> CreateAsync(CreateUnitOfMeasureRequest request, CancellationToken cancellationToken = default)
        {
            var entity = new UnitOfMeasure
            {
                CompanyId = _currentUser.CompanyId,
                Name = request.Name.Trim(),
                Symbol = request.Symbol.Trim()
            };

            await _repository.AddAsync(entity, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return new UnitOfMeasureDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Symbol = entity.Symbol
            };
        }

        public async Task<UnitOfMeasureDto> UpdateAsync(int id, UpdateUnitOfMeasureRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("Unit of measure not found.");

            if (entity.CompanyId != _currentUser.CompanyId)
                throw new UnauthorizedAccessException("Unit of measure does not belong to your company.");

            entity.Name = request.Name.Trim();
            entity.Symbol = request.Symbol.Trim();
            entity.UpdatedAt = DateTime.UtcNow;

            _repository.Update(entity);
            await _repository.SaveChangesAsync(cancellationToken);

            return new UnitOfMeasureDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Symbol = entity.Symbol
            };
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("Unit of measure not found.");

            if (entity.CompanyId != _currentUser.CompanyId)
                throw new UnauthorizedAccessException("Unit of measure does not belong to your company.");

            _repository.Delete(entity);
            await _repository.SaveChangesAsync(cancellationToken);
        }
    }
}