using ERPSystem.Application.DTOs.Expenses;

namespace ERPSystem.Application.Interfaces
{
    public interface IExpenseService
    {
        Task<PagedResult<ExpenseListItemDto>> GetAllAsync(ExpenseQuery query, CancellationToken ct);
        Task<ExpenseDetailsDto?> GetByIdAsync(int id, CancellationToken ct);
        Task<ExpenseDetailsDto> CreateAsync(CreateExpenseDto dto, CancellationToken ct);
        Task<ExpenseDetailsDto> UpdateAsync(int id, UpdateExpenseDto dto, CancellationToken ct);
        Task<ExpenseDetailsDto> UpdateStatusAsync(int id, UpdateExpenseStatusDto dto, CancellationToken ct);
        Task DeleteAsync(int id, CancellationToken ct);
    }
}