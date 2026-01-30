using ERPSystem.Domain.Entities.Expenses;

namespace ERPSystem.Domain.Abstractions
{
    public interface IExpenseCategoryRepository
    {
        Task<IReadOnlyList<ExpenseCategory>> GetAllAsync(int companyId, CancellationToken ct);
        Task<ExpenseCategory?> GetByIdAsync(int companyId, int id, CancellationToken ct);
        Task<ExpenseCategory?> GetByIdForUpdateAsync(int companyId, int id, CancellationToken ct);
        Task<ExpenseCategory?> GetByNameAsync(int companyId, string name, CancellationToken ct);
        Task<bool> HasExpensesAsync(int companyId, int categoryId, CancellationToken ct);
        Task<(int ExpenseCount, decimal TotalAmount)> GetCategoryStatsAsync(int companyId, int categoryId, CancellationToken ct);
        Task<ExpenseCategory> CreateAsync(ExpenseCategory entity, CancellationToken ct);
        Task UpdateAsync(ExpenseCategory entity, CancellationToken ct);
        Task SoftDeleteAsync(ExpenseCategory entity, CancellationToken ct);
    }
}