using ERPSystem.Application.DTOs.Expenses;

namespace ERPSystem.Application.Interfaces
{
    public interface IExpenseStatsService
    {
        Task<ExpenseSummaryDto> GetSummaryAsync(StatsQuery query, CancellationToken ct);
        Task<ExpenseOverTimeDto> GetOverTimeAsync(StatsQuery query, CancellationToken ct);
        Task<ExpenseByCategoryDto> GetByCategoryAsync(StatsQuery query, CancellationToken ct);
    }
}