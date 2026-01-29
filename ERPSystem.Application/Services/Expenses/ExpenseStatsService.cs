using ERPSystem.Application.DTOs.Expenses;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using System.Globalization;

namespace ERPSystem.Application.Services.Expenses
{
    public class ExpenseStatsService : IExpenseStatsService
    {
        private readonly IExpenseRepository _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly IModuleAccessService _moduleAccess;

        public ExpenseStatsService(
            IExpenseRepository repository,
            ICurrentUserService currentUser,
            IModuleAccessService moduleAccess)
        {
            _repository = repository;
            _currentUser = currentUser;
            _moduleAccess = moduleAccess;
        }

        public async Task<ExpenseSummaryDto> GetSummaryAsync(StatsQuery query, CancellationToken ct)
        {
            await _moduleAccess.EnsureExpensesEnabledAsync(ct);

            var (total, max, min, count) = await _repository.GetSummaryAsync(
                _currentUser.CompanyId,
                query.FromDate,
                query.ToDate,
                ct);

            return new ExpenseSummaryDto
            {
                TotalSpending = total,
                HighestExpense = max,
                LowestExpense = min,
                ExpenseCount = count,
                AverageExpense = count > 0 ? total / count : 0
            };
        }

        public async Task<ExpenseOverTimeDto> GetOverTimeAsync(StatsQuery query, CancellationToken ct)
        {
            await _moduleAccess.EnsureExpensesEnabledAsync(ct);

            // Parse grouping
            if (!Enum.TryParse<TimeGrouping>(query.Grouping, true, out var grouping))
                throw BusinessErrors.InvalidTimeGrouping();

            // Get daily totals from repository
            var dailyTotals = await _repository.GetDailyTotalsAsync(
                _currentUser.CompanyId,
                query.FromDate,
                query.ToDate,
                ct);

            // Group by requested period in application layer
            var series = GroupByPeriod(dailyTotals, grouping);

            return new ExpenseOverTimeDto
            {
                Series = series
            };
        }

        public async Task<ExpenseByCategoryDto> GetByCategoryAsync(StatsQuery query, CancellationToken ct)
        {
            await _moduleAccess.EnsureExpensesEnabledAsync(ct);

            var categoryTotals = await _repository.GetCategoryTotalsAsync(
                _currentUser.CompanyId,
                query.FromDate,
                query.ToDate,
                ct);

            var grandTotal = categoryTotals.Sum(c => c.Amount);

            var categories = categoryTotals.Select(c => new CategoryBreakdownItem
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Amount = c.Amount,
                Count = c.Count,
                Percentage = grandTotal > 0 ? Math.Round(c.Amount / grandTotal * 100, 2) : 0
            }).ToList();

            return new ExpenseByCategoryDto
            {
                Categories = categories
            };
        }

        private static IReadOnlyList<ExpenseTimeSeriesItem> GroupByPeriod(
            IReadOnlyList<(DateTime Date, decimal Amount, int Count)> dailyTotals,
            TimeGrouping grouping)
        {
            if (!dailyTotals.Any())
                return Array.Empty<ExpenseTimeSeriesItem>();

            return grouping switch
            {
                TimeGrouping.Day => dailyTotals.Select(d => new ExpenseTimeSeriesItem
                {
                    Period = d.Date.ToString("yyyy-MM-dd"),
                    Amount = d.Amount,
                    Count = d.Count
                }).ToList(),

                TimeGrouping.Week => dailyTotals
                    .GroupBy(d => GetWeekKey(d.Date))
                    .OrderBy(g => g.Key)
                    .Select(g => new ExpenseTimeSeriesItem
                    {
                        Period = g.Key,
                        Amount = g.Sum(x => x.Amount),
                        Count = g.Sum(x => x.Count)
                    }).ToList(),

                TimeGrouping.Month => dailyTotals
                    .GroupBy(d => new { d.Date.Year, d.Date.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => new ExpenseTimeSeriesItem
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Amount = g.Sum(x => x.Amount),
                        Count = g.Sum(x => x.Count)
                    }).ToList(),

                _ => Array.Empty<ExpenseTimeSeriesItem>()
            };
        }

        private static string GetWeekKey(DateTime date)
        {
            var cal = CultureInfo.InvariantCulture.Calendar;
            var week = cal.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return $"{date.Year}-W{week:D2}";
        }
    }
}