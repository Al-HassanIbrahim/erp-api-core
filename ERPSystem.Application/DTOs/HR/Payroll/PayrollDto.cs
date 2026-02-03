using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Payroll
{
    public class PayrollDto
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
    }
}
