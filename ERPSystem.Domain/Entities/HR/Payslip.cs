using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Entities.HR
{
    public class Payslip
    {
        public int Id { get; set; }

        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public int Month { get; set; }    // 1-12
        public int Year { get; set; }

        public decimal BasicSalary { get; set; }           //Basic Salary
        public int WorkingDaysInMonth { get; set; } = 30;  

        public int ActualWorkedDays { get; set; }          // we can calculate from attendence
        public decimal DailyRate => BasicSalary / WorkingDaysInMonth;

        public decimal GrossSalary => ActualWorkedDays * DailyRate;
        public decimal TotalDeductions => BasicSalary - GrossSalary; // الباقي خصم غياب
        public decimal NetSalary => GrossSalary;

        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public string? Notes { get; set; }
    }
}
