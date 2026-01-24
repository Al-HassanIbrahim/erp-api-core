using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Application.DTOs.HR.Department;
using ERPSystem.Application.DTOs.HR.JobPosition;

namespace ERPSystem.Application.DTOs.HR.Employee
{
    public class EmployeeDetailDto
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = null!;
        public string Nationality { get; set; } = null!;
        public string NationalId { get; set; } = null!;
        public string MaritalStatus { get; set; } = null!;

        public DateTime HireDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string Status { get; set; } = null!;

        public DepartmentDto? Department { get; set; }
        public PositionDto? Position { get; set; }
        public EmployeeListDto? ReportsTo { get; set; }
        public List<EmployeeListDto> DirectReports { get; set; } = new();

        public AddressDto CurrentAddress { get; set; } = null!;

        public decimal Salary { get; set; }
        public string Currency { get; set; } = null!;


        public string? ProfileImageUrl { get; set; }
        public List<DocumentDto> Documents { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
