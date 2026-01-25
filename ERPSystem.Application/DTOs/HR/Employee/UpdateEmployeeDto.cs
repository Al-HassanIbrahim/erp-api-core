using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Employee
{
    public class UpdateEmployeeDto
    {
            [MaxLength(50)]
            public string? FirstName { get; set; }

            [MaxLength(50)]
            public string? LastName { get; set; }

            [EmailAddress]
            public string? Email { get; set; }

            [Phone]
            public string? PhoneNumber { get; set; }


            public DateTime? DateOfBirth { get; set; }
            public Gender? Gender { get; set; }

            [MaxLength(100)]
            public string? Nationality { get; set; }

            public MaritalStatus? MaritalStatus { get; set; }

            public string? BankAccountNumber { get; set; }
            public string? BankName { get; set; }
            public string? BankBranch { get; set; }

        public Guid? DepartmentId { get; set; }
            public Guid? PositionId { get; set; }
            public Guid? ReportsToId { get; set; }

            public AddressDto? CurrentAddress { get; set; }

            [Range(0, double.MaxValue)]
            public decimal? Salary { get; set; }

            [Required]
            public EmployeeStatus Status { get; set; }

            [Required]
            public DateTime EffectiveDate { get; set; }

            [MaxLength(500)]
            public string? Reason { get; set; }
        }
}
