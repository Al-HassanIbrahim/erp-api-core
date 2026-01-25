using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Employee
{
    public class CreateEmployeeDto
    {
        [Required, MaxLength(50)]
        public string EmployeeCode { get; set; } = null!;

        [Required, MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, Phone]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required, MaxLength(100)]
        public string Nationality { get; set; } = null!;

        [Required, MaxLength(50)]
        public string NationalId { get; set; } = null!;

        [Required]
        public MaritalStatus MaritalStatus { get; set; }

        [Required]
        public string BankAccountNumber { get; set; } = null!;

        [Required]
        public string BankName { get; set; } = null!;

        [Required]
        public string BankBranch { get; set; } = null!;

        [Required]
        public DateTime HireDate { get; set; }

        public int ProbationPeriodMonths { get; set; } = 3;

        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public Guid? ReportsToId { get; set; }

        [Required]
        public AddressDto CurrentAddress { get; set; } = null!;



        [Required, Range(0, double.MaxValue)]
        public decimal Salary { get; set; }

        public string Currency { get; set; } = "EGP";

    }
}
