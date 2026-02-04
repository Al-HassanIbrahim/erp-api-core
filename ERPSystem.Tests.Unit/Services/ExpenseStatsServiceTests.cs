using Moq;
using FluentAssertions;
using ERPSystem.Application.Services.Expenses;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Application.DTOs.Expenses;

namespace ERPSystem.Tests.Unit.Services
{
    public class ExpenseStatsServiceTests
    {
        private readonly Mock<IExpenseRepository> _repoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly Mock<IModuleAccessService> _moduleAccessMock;
        private readonly ExpenseStatsService _service;
        private readonly int _companyId = 1;

        public ExpenseStatsServiceTests()
        {
            _repoMock = new Mock<IExpenseRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();
            _moduleAccessMock = new Mock<IModuleAccessService>();

            _currentUserMock.Setup(x => x.CompanyId).Returns(_companyId);

            _service = new ExpenseStatsService(
                _repoMock.Object,
                _currentUserMock.Object,
                _moduleAccessMock.Object);
        }

        [Fact]
        public async Task GetSummaryAsync_ShouldReturnSummary()
        {
            // Given
            var ct = CancellationToken.None;
            var query = new StatsQuery { FromDate = DateTime.UtcNow.AddDays(-30), ToDate = DateTime.UtcNow };
            _repoMock.Setup(r => r.GetSummaryAsync(_companyId, query.FromDate, query.ToDate, ct))
                .ReturnsAsync((1000m, 500m, 10m, 10));

            // When
            var result = await _service.GetSummaryAsync(query, ct);

            // Then
            result.TotalSpending.Should().Be(1000m);
            result.HighestExpense.Should().Be(500m);
            result.LowestExpense.Should().Be(10m);
            result.ExpenseCount.Should().Be(10);
            result.AverageExpense.Should().Be(100m);
            _moduleAccessMock.Verify(m => m.EnsureExpensesEnabledAsync(ct), Times.Once);
        }

        [Fact]
        public async Task GetOverTimeAsync_ShouldReturnGroupedSeries_WhenDayGrouping()
        {
            // Given
            var ct = CancellationToken.None;
            var query = new StatsQuery { Grouping = "Day", FromDate = DateTime.UtcNow.AddDays(-2), ToDate = DateTime.UtcNow };
            var dailyTotals = new List<(DateTime Date, decimal Amount, int Count)>
            {
                (DateTime.UtcNow.Date.AddDays(-1), 100m, 1),
                (DateTime.UtcNow.Date, 200m, 2)
            };

            _repoMock.Setup(r => r.GetDailyTotalsAsync(_companyId, query.FromDate, query.ToDate, ct))
                .ReturnsAsync(dailyTotals);

            // When
            var result = await _service.GetOverTimeAsync(query, ct);

            // Then
            result.Series.Should().HaveCount(2);
            result.Series[0].Period.Should().Be(DateTime.UtcNow.Date.AddDays(-1).ToString("yyyy-MM-dd"));
            result.Series[0].Amount.Should().Be(100m);
        }

        [Fact]
        public async Task GetOverTimeAsync_ShouldReturnGroupedSeries_WhenWeekGrouping()
        {
            // Given
            var ct = CancellationToken.None;
            var query = new StatsQuery { Grouping = "Week", FromDate = new DateTime(2026, 1, 1), ToDate = new DateTime(2026, 1, 31) };
            var dailyTotals = new List<(DateTime Date, decimal Amount, int Count)>
            {
                (new DateTime(2026, 1, 1), 100m, 1),
                (new DateTime(2026, 1, 8), 200m, 2)
            };

            _repoMock.Setup(r => r.GetDailyTotalsAsync(_companyId, query.FromDate, query.ToDate, ct))
                .ReturnsAsync(dailyTotals);

            // When
            var result = await _service.GetOverTimeAsync(query, ct);

            // Then
            result.Series.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetOverTimeAsync_ShouldReturnGroupedSeries_WhenMonthGrouping()
        {
            // Given
            var ct = CancellationToken.None;
            var query = new StatsQuery { Grouping = "Month", FromDate = new DateTime(2026, 1, 1), ToDate = new DateTime(2026, 3, 1) };
            var dailyTotals = new List<(DateTime Date, decimal Amount, int Count)>
            {
                (new DateTime(2026, 1, 1), 100m, 1),
                (new DateTime(2026, 2, 1), 200m, 2)
            };

            _repoMock.Setup(r => r.GetDailyTotalsAsync(_companyId, query.FromDate, query.ToDate, ct))
                .ReturnsAsync(dailyTotals);

            // When
            var result = await _service.GetOverTimeAsync(query, ct);

            // Then
            result.Series.Should().HaveCount(2);
            result.Series[0].Period.Should().Be("2026-01");
        }

        [Fact]
        public async Task GetOverTimeAsync_ShouldReturnEmpty_WhenNoDailyTotals()
        {
            // Given
            var ct = CancellationToken.None;
            var query = new StatsQuery { Grouping = "Day" };
            _repoMock.Setup(r => r.GetDailyTotalsAsync(_companyId, query.FromDate, query.ToDate, ct))
                .ReturnsAsync(new List<(DateTime Date, decimal Amount, int Count)>());

            // When
            var result = await _service.GetOverTimeAsync(query, ct);

            // Then
            result.Series.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByCategoryAsync_ShouldReturnCategoryBreakdown()
        {
            // Given
            var ct = CancellationToken.None;
            var query = new StatsQuery();
            var categoryTotals = new List<(int CategoryId, string CategoryName, decimal Amount, int Count)>
            {
                (1, "Cat 1", 600m, 6),
                (2, "Cat 2", 400m, 4)
            };

            _repoMock.Setup(r => r.GetCategoryTotalsAsync(_companyId, query.FromDate, query.ToDate, ct))
                .ReturnsAsync(categoryTotals);

            // When
            var result = await _service.GetByCategoryAsync(query, ct);

            // Then
            result.Categories.Should().HaveCount(2);
            result.Categories.First(c => c.CategoryId == 1).Percentage.Should().Be(60);
            result.Categories.First(c => c.CategoryId == 2).Percentage.Should().Be(40);
        }
    }
}
