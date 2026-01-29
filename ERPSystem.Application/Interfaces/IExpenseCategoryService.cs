using ERPSystem.Application.DTOs.Expenses;

namespace ERPSystem.Application.Interfaces
{
    public interface IExpenseCategoryService
    {
        Task<IReadOnlyList<ExpenseCategoryDto>> GetAllAsync(CancellationToken ct);
        Task<ExpenseCategoryDetailsDto?> GetByIdAsync(int id, CancellationToken ct);
        Task<ExpenseCategoryDto> CreateAsync(CreateExpenseCategoryDto dto, CancellationToken ct);
        Task<ExpenseCategoryDto> UpdateAsync(int id, UpdateExpenseCategoryDto dto, CancellationToken ct);
        Task DeleteAsync(int id, CancellationToken ct);
    }
}