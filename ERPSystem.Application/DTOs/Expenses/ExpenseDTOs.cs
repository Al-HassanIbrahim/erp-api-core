using System.ComponentModel.DataAnnotations;

namespace ERPSystem.Application.DTOs.Expenses
{
    // List view (lightweight)
    public class ExpenseListItemDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Vendor { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Status { get; set; } = string.Empty;       // "Paid" / "Pending"
        public string PaymentMethod { get; set; } = string.Empty; // "Cash" / "CreditCard" etc.
    }

    // Detail view (full)
    public class ExpenseDetailsDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Vendor { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

    public class CreateExpenseDto
    {
        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Vendor { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime ExpenseDate { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // "Paid" or "Pending"

        [Required]
        public string PaymentMethod { get; set; } = "Cash";

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }
    }

    public class UpdateExpenseDto
    {
        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Vendor { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime ExpenseDate { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }
    }

    public class UpdateExpenseStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }

    // Query/filter model
    public class ExpenseQuery
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public string? Status { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string SortBy { get; set; } = "ExpenseDate"; // ExpenseDate, Amount
        public string SortDir { get; set; } = "desc";        // asc, desc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // Paged response
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}