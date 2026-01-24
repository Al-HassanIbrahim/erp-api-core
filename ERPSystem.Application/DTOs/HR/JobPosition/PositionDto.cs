using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.JobPosition
{
    public class PositionDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Level { get; set; } = null!;
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public bool IsActive { get; set; }
    }
}
