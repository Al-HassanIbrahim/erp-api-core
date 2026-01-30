using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Domain.Entities.Expenses
{
    public class Expense : AuditableEntity, ICompanyEntity
    {
        public int CompanyId { get; set; }
        public int ExpenseCategoryId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Vendor { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public ExpenseStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? Notes { get; set; }
        public string? ReferenceNumber { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation
        public ExpenseCategory Category { get; set; } = null!;
    }
}