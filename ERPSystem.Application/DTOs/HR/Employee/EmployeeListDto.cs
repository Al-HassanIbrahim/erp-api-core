using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Employee
{
    public class EmployeeListDto
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? DepartmentName { get; set; }
        public string? PositionTitle { get; set; }
        public string Status { get; set; } = null!;
        public DateTime HireDate { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
