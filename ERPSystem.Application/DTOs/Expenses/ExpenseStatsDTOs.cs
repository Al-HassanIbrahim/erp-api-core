namespace ERPSystem.Application.DTOs.Expenses
{
    public class ExpenseSummaryDto
    {
        public decimal TotalSpending { get; set; }
        public decimal HighestExpense { get; set; }
        public decimal LowestExpense { get; set; }
        public int ExpenseCount { get; set; }
        public decimal AverageExpense { get; set; }
    }

    public class ExpenseOverTimeDto
    {
        public IReadOnlyList<ExpenseTimeSeriesItem> Series { get; set; } = Array.Empty<ExpenseTimeSeriesItem>();
    }

    public class ExpenseTimeSeriesItem
    {
        public string Period { get; set; } = string.Empty; // "2026-01-15", "2026-W03", "2026-01"
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class ExpenseByCategoryDto
    {
        public IReadOnlyList<CategoryBreakdownItem> Categories { get; set; } = Array.Empty<CategoryBreakdownItem>();
    }

    public class CategoryBreakdownItem
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public enum TimeGrouping
    {
        Day,
        Week,
        Month
    }

    public class StatsQuery
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Grouping { get; set; } = "Month"; // Day, Week, Month
    }
}