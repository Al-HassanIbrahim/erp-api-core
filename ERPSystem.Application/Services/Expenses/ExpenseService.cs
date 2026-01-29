using ERPSystem.Application.DTOs.Expenses;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Expenses;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Application.Services.Expenses
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _repository;
        private readonly IExpenseCategoryRepository _categoryRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IModuleAccessService _moduleAccess;

        public ExpenseService(
            IExpenseRepository repository,
            IExpenseCategoryRepository categoryRepository,
            ICurrentUserService currentUser,
            IModuleAccessService moduleAccess)
        {
            _repository = repository;
            _categoryRepository = categoryRepository;
            _currentUser = currentUser;
            _moduleAccess = moduleAccess;
        }

        public async Task<PagedResult<ExpenseListItemDto>> GetAllAsync(ExpenseQuery query, CancellationToken ct)
        {
            await _moduleAccess.EnsureExpensesEnabledAsync(ct);

            // Parse enums from query strings
            ExpenseStatus? status = null;
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                if (!Enum.TryParse<ExpenseStatus>(query.Status, true, out var parsedStatus))
                    throw BusinessErrors.InvalidExpenseStatus();
                status = parsedStatus;
            }

            PaymentMethod? paymentMethod = null;
            if (!string.IsNullOrWhiteSpace(query.PaymentMethod))
            {
                if (!Enum.TryParse<PaymentMethod>(query.PaymentMethod, true, out var parsedMethod))
                    throw BusinessErrors.InvalidPaymentMethod();
                paymentMethod = parsedMethod;
            }

            var (items, totalCount) = await _repository.GetPagedAsync(
                _currentUser.CompanyId,
                query.Search,
                query.CategoryId,
                status,
                paymentMethod,
                query.FromDate,
                query.ToDate,
                query.MinAmount,
                query.MaxAmount,
                query.SortBy,
                query.SortDir,
                query.Page,
                query.PageSize,
                ct);

            return new PagedResult<ExpenseListItemDto>
            {
                Items = items.Select(MapToListItem).ToList(),
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task<ExpenseDetailsDto?> GetByIdAsync(int id, CancellationToken ct)
        {
            await _moduleAccess.EnsureExpensesEnabledAsync(ct);

            var expense = await _repository.GetByIdAsync(_currentUser.CompanyId, id, ct);
            if (expense == null)
                return null;

            return MapToDetails(expense);
        }

        public async Task<ExpenseDetailsDto> CreateAsync(CreateExpenseDto dto, CancellationToken ct)
        {
            await _moduleAccess.EnsureExpensesEnabledAsync(ct);

            // Validate category exists
            var category = await _categoryRepository.GetByIdAsync(_currentUser.CompanyId, dto.CategoryId, ct)
                ?? throw BusinessErrors.ExpenseCategoryNotFound();

            // Parse enums
            if (!Enum.TryParse<ExpenseStatus>(dto.Status, true, out var status))
                throw BusinessErrors.InvalidExpenseStatus();

            if (!Enum.TryParse<PaymentMethod>(dto.PaymentMethod, true, out var paymentMethod))
                throw BusinessErrors.InvalidPaymentMethod();

            var entity = new Expense
            {
                CompanyId = _currentUser.CompanyId,
                ExpenseCategoryId = dto.CategoryId,
                Description = dto.Description,
                Vendor = dto.Vendor,
                Amount = dto.Amount,
                ExpenseDate = dto.ExpenseDate,
                Status = status,
                PaymentMethod = paymentMethod,
                Notes = dto.Notes,
                ReferenceNumber = dto.ReferenceNumber,
                IsDeleted = false
            };

            var created = await _repository.CreateAsync(entity, ct);
            created.Category = category; // Attach for mapping

            return MapToDetails(created);
        }

        public async Task<ExpenseDetailsDto> UpdateAsync(int id, UpdateExpenseDto dto, CancellationToken ct)
        {
            await _moduleAccess.EnsureExpensesEnabledAsync(ct);

            var entity = await _repository.GetByIdForUpdateAsync(_currentUser.CompanyId, id, ct)
                ?? throw BusinessErrors.ExpenseNotFound();

            // Validate category if changed
            if (entity.ExpenseCategoryId != dto.CategoryId)
            {
                var category = await _categoryRepository.GetByIdAsync(_currentUser.CompanyId, dto.CategoryId, ct)
                    ?? throw BusinessErrors.ExpenseCategoryNotFound();
                entity.Category = category;
            }

            // Parse enums
            if (!Enum.TryParse<ExpenseStatus>(dto.Status, true, out var status))
                throw BusinessErrors.InvalidExpenseStatus();

            if (!Enum.TryParse<PaymentMethod>(dto.PaymentMethod, true, out var paymentMethod))
                throw BusinessErrors.InvalidPaymentMethod();

            entity.ExpenseCategoryId = dto.CategoryId;
            entity.Description = dto.Description;
            entity.Vendor = dto.Vendor;
            entity.Amount = dto.Amount;
            entity.ExpenseDate = dto.ExpenseDate;
            entity.Status = status;
            entity.PaymentMethod = paymentMethod;
            entity.Notes = dto.Notes;
            entity.ReferenceNumber = dto.ReferenceNumber;

            await _repository.UpdateAsync(entity, ct);

            return MapToDetails(entity);
        }

        public async Task<ExpenseDetailsDto> UpdateStatusAsync(int id, UpdateExpenseStatusDto dto, CancellationToken ct)
        {
            await _moduleAccess.EnsureExpensesEnabledAsync(ct);

            var entity = await _repository.GetByIdForUpdateAsync(_currentUser.CompanyId, id, ct)
                ?? throw BusinessErrors.ExpenseNotFound();

            if (!Enum.TryParse<ExpenseStatus>(dto.Status, true, out var status))
                throw BusinessErrors.InvalidExpenseStatus();

            entity.Status = status;

            await _repository.UpdateAsync(entity, ct);

            return MapToDetails(entity);
        }

        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            await _moduleAccess.EnsureExpensesEnabledAsync(ct);

            var entity = await _repository.GetByIdForUpdateAsync(_currentUser.CompanyId, id, ct)
                ?? throw BusinessErrors.ExpenseNotFound();

            await _repository.SoftDeleteAsync(entity, ct);
        }

        private static ExpenseListItemDto MapToListItem(Expense e) => new()
        {
            Id = e.Id,
            Description = e.Description,
            Vendor = e.Vendor,
            CategoryId = e.ExpenseCategoryId,
            CategoryName = e.Category?.Name ?? string.Empty,
            Amount = e.Amount,
            ExpenseDate = e.ExpenseDate,
            Status = e.Status.ToString(),
            PaymentMethod = e.PaymentMethod.ToString()
        };

        private static ExpenseDetailsDto MapToDetails(Expense e) => new()
        {
            Id = e.Id,
            Description = e.Description,
            Vendor = e.Vendor,
            CategoryId = e.ExpenseCategoryId,
            CategoryName = e.Category?.Name ?? string.Empty,
            Amount = e.Amount,
            ExpenseDate = e.ExpenseDate,
            Status = e.Status.ToString(),
            PaymentMethod = e.PaymentMethod.ToString(),
            Notes = e.Notes,
            ReferenceNumber = e.ReferenceNumber,
            CreatedAt = e.CreatedAt
        };
    }
}