using ERPSystem.Domain.Entities.Expenses;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Domain.Abstractions
{
    public interface IExpenseRepository
    {
        Task<(IReadOnlyList<Expense> Items, int TotalCount)> GetPagedAsync(
            int companyId,
            string? search,
            int? categoryId,
            ExpenseStatus? status,
            PaymentMethod? paymentMethod,
            DateTime? fromDate,
            DateTime? toDate,
            decimal? minAmount,
            decimal? maxAmount,
            string? sortBy,
            string? sortDir,
            int page,
            int pageSize,
            CancellationToken ct);

        Task<Expense?> GetByIdAsync(int companyId, int id, CancellationToken ct);
        Task<Expense?> GetByIdForUpdateAsync(int companyId, int id, CancellationToken ct);
        Task<Expense> CreateAsync(Expense entity, CancellationToken ct);
        Task UpdateAsync(Expense entity, CancellationToken ct);
        Task SoftDeleteAsync(Expense entity, CancellationToken ct);

        // Stats - return primitives/tuples, Application layer builds DTOs
        Task<(decimal Total, decimal Max, decimal Min, int Count)> GetSummaryAsync(
            int companyId, DateTime? from, DateTime? to, CancellationToken ct);

        Task<IReadOnlyList<(DateTime Date, decimal Amount, int Count)>> GetDailyTotalsAsync(
            int companyId, DateTime? from, DateTime? to, CancellationToken ct);

        Task<IReadOnlyList<(int CategoryId, string CategoryName, decimal Amount, int Count)>> GetCategoryTotalsAsync(
            int companyId, DateTime? from, DateTime? to, CancellationToken ct);
    }
}