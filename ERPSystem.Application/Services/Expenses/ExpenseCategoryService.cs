using ERPSystem.Application.DTOs.Expenses;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Expenses;

namespace ERPSystem.Application.Services.Expenses
{
    public class ExpenseCategoryService : IExpenseCategoryService
    {
        private readonly IExpenseCategoryRepository _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly IModuleAccessService _moduleAccess;

        public ExpenseCategoryService(
            IExpenseCategoryRepository repository,
            ICurrentUserService currentUser,
            IModuleAccessService moduleAccess)
        {
            _repository = repository;
            _currentUser = currentUser;
            _moduleAccess = moduleAccess;
        }

        public async Task<IReadOnlyList<ExpenseCategoryDto>> GetAllAsync(CancellationToken ct)
        {
            await _moduleAccess.EnsureExpensesEnabledAsync(ct);

            var categories = await _repository.GetAllAsync(_currentUser.CompanyId, ct);

            return categories.Select(c => new ExpenseCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            }).ToList();
        }

        public async Task<ExpenseCategoryDetailsDto?> GetByIdAsync(int id, CancellationToken ct)
        {
            await _moduleAccess.EnsureExpensesEnabledAsync(ct);

            var category = await _repository.GetByIdAsync(_currentUser.CompanyId, id, ct);
            if (category == null)
                return null;

            var (expenseCount, totalAmount) = await _repository.GetCategoryStatsAsync(_currentUser.CompanyId, id, ct);

            return new ExpenseCategoryDetailsDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ExpenseCount = expenseCount,
                TotalAmount = totalAmount
            };
        }

        public async Task<ExpenseCategoryDto> CreateAsync(CreateExpenseCategoryDto dto, CancellationToken ct)
        {
            await _moduleAccess.EnsureExpensesEnabledAsync(ct);

            // Check for duplicate name
            var existing = await _repository.GetByNameAsync(_currentUser.CompanyId, dto.Name, ct);
            if (existing != null)
                throw BusinessErrors.DuplicateExpenseCategoryName();

            var entity = new ExpenseCategory
            {
                CompanyId = _currentUser.CompanyId,
                Name = dto.Name,
                Description = dto.Description,
                IsDeleted = false
            };

            var created = await _repository.CreateAsync(entity, ct);

            return new ExpenseCategoryDto
            {
                Id = created.Id,
                Name = created.Name,
                Description = created.Description
            };
        }

        public async Task<ExpenseCategoryDto> UpdateAsync(int id, UpdateExpenseCategoryDto dto, CancellationToken ct)
        {
            await _moduleAccess.EnsureExpensesEnabledAsync(ct);

            var entity = await _repository.GetByIdForUpdateAsync(_currentUser.CompanyId, id, ct)
                ?? throw BusinessErrors.ExpenseCategoryNotFound();

            // Check for duplicate name (exclude current)
            var existing = await _repository.GetByNameAsync(_currentUser.CompanyId, dto.Name, ct);
            if (existing != null && existing.Id != id)
                throw BusinessErrors.DuplicateExpenseCategoryName();

            entity.Name = dto.Name;
            entity.Description = dto.Description;

            await _repository.UpdateAsync(entity, ct);

            return new ExpenseCategoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description
            };
        }

        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            await _moduleAccess.EnsureExpensesEnabledAsync(ct);

            var entity = await _repository.GetByIdForUpdateAsync(_currentUser.CompanyId, id, ct)
                ?? throw BusinessErrors.ExpenseCategoryNotFound();

            // Check if category has expenses
            var hasExpenses = await _repository.HasExpensesAsync(_currentUser.CompanyId, id, ct);
            if (hasExpenses)
                throw BusinessErrors.ExpenseCategoryInUse();

            await _repository.SoftDeleteAsync(entity, ct);
        }
    }
}