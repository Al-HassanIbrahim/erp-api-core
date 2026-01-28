using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Payroll
{
    public class UpdatePayrollDto
    {
        public List<PayrollItemDto>? Allowances { get; set; }
        public List<PayrollItemDto>? Deductions { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
