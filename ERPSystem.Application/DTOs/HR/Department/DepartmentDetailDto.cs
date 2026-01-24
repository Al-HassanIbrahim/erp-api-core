using ERPSystem.Application.DTOs.HR.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Department
{
    public class DepartmentDetailDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public int EmployeeCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public EmployeeListDto? Manager { get; set; }
        public List<EmployeeListDto> Employees { get; set; } = new();
    }
}
