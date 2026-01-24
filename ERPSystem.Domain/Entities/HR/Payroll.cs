using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Domain.Entities.HR
{
    public class Payroll
    {
        public Guid Id { get; set; }

        [Required, Range(1, 12)]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public DateOnly PayPeriodStart { get; set; }

        [Required]
        public DateOnly PayPeriodEnd { get; set; }

        // Salary Breakdown
        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal BasicSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAllowances { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSalary { get; set; }

        // Attendance Details
        public int WorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int UnpaidLeaveDays { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal OvertimeHours { get; set; }

        // Status
        [Required]
        public PayrollStatus Status { get; set; } = PayrollStatus.Draft;

        public DateTime? ProcessedDate { get; set; }
        public DateTime? PaidDate { get; set; }

        // Payment Details
        public PaymentMethod? PaymentMethod { get; set; }

        [MaxLength(100)]
        public string? BankAccountNumber { get; set; }

        [MaxLength(100)]
        public string? TransactionReference { get; set; }

        //Relation
        [ForeignKey("Employee")]
        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        // Collections
        public ICollection<PayrollLineItem> LineItems { get; set; } = new List<PayrollLineItem>();

        // Audit
        [MaxLength(100)]
        public string? GeneratedBy { get; set; }

        [MaxLength(100)]
        public string? ProcessedBy { get; set; }

        [MaxLength(100)]
        public string? PaidBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
