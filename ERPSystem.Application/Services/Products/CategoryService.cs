using ERPSystem.Application.DTOs.Products;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Products;

namespace ERPSystem.Application.Services.Products
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public CategoryService(
            ICategoryRepository repository,
            ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var items = await _repository.GetAllByCompanyAsync(_currentUser.CompanyId, cancellationToken);

            return items.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ParentCategoryId = c.ParentCategoryId,
                ParentCategoryName = c.ParentCategory?.Name
            }).ToList();
        }

        public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var item = await _repository.GetByIdAsync(id, cancellationToken);

            if (item == null || item.CompanyId != _currentUser.CompanyId)
                return null;

            return new CategoryDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                ParentCategoryId = item.ParentCategoryId,
                ParentCategoryName = item.ParentCategory?.Name
            };
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            // Validate parent category if provided
            if (request.ParentCategoryId.HasValue)
            {
                var parent = await _repository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
                if (parent == null || parent.CompanyId != _currentUser.CompanyId)
                    throw new InvalidOperationException("Parent category not found.");
            }

            var entity = new Category
            {
                CompanyId = _currentUser.CompanyId,
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                ParentCategoryId = request.ParentCategoryId
            };

            await _repository.AddAsync(entity, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return new CategoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                ParentCategoryId = entity.ParentCategoryId
            };
        }

        public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("Category not found.");

            if (entity.CompanyId != _currentUser.CompanyId)
                throw new UnauthorizedAccessException("Category does not belong to your company.");

            // Validate parent category if provided
            if (request.ParentCategoryId.HasValue)
            {
                if (request.ParentCategoryId.Value == id)
                    throw new InvalidOperationException("Category cannot be its own parent.");

                var parent = await _repository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
                if (parent == null || parent.CompanyId != _currentUser.CompanyId)
                    throw new InvalidOperationException("Parent category not found.");
            }

            entity.Name = request.Name.Trim();
            entity.Description = request.Description?.Trim();
            entity.ParentCategoryId = request.ParentCategoryId;
            entity.UpdatedAt = DateTime.UtcNow;

            _repository.Update(entity);
            await _repository.SaveChangesAsync(cancellationToken);

            return new CategoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                ParentCategoryId = entity.ParentCategoryId
            };
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("Category not found.");

            if (entity.CompanyId != _currentUser.CompanyId)
                throw new UnauthorizedAccessException("Category does not belong to your company.");

            _repository.Delete(entity);
            await _repository.SaveChangesAsync(cancellationToken);
        }
    }
}