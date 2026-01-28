using ERPSystem.Application.DTOs.HR.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Payroll
{
    public class PayrollDetailDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string EmployeeName { get; set; } = null!;
        public int Month { get; set; }
        public int Year { get; set; }
        public DateOnly PayPeriodStart { get; set; }
        public DateOnly PayPeriodEnd { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal TotalAllowances { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? ProcessedDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public EmployeeListDto? Employee { get; set; }
        public int WorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int UnpaidLeaveDays { get; set; }
        public decimal OvertimeHours { get; set; }
        public List<PayrollItemDto> Allowances { get; set; } = new();
        public List<PayrollItemDto> Deductions { get; set; } = new();
        public string? PaymentMethod { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? TransactionReference { get; set; }
        public string? GeneratedBy { get; set; }
        public string? ProcessedBy { get; set; }
        public string? PaidBy { get; set; }
    }
}
