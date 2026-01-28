using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Payroll
{
    public class PayrollSummaryDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalEmployees { get; set; }
        public decimal TotalGrossPay { get; set; }
        public decimal TotalAllowances { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetPay { get; set; }
        public int DraftCount { get; set; }
        public int ProcessedCount { get; set; }
        public int PaidCount { get; set; }
    }
}
