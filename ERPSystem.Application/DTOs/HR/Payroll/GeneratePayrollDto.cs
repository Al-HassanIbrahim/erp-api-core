using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Payroll
{
    public class GeneratePayrollDto
    {
        [Required, Range(1, 12)]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        public List<Guid>? EmployeeIds { get; set; }
        public List<Guid>? DepartmentIds { get; set; }
        public bool IncludeInactive { get; set; } = false;
    }
}
