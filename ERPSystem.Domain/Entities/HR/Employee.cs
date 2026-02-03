using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.HR;
using ERPSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

public class Employee: ICompanyEntity
{
    public Guid Id { get; set; }
    public int CompanyId { get; set; }

    [Required, MaxLength(50)]
    public string EmployeeCode { get; set; } = null!;

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = null!;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = null!;

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = null!;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    [Required]
    public DateTime DateOfBirth { get; set; }

    // Bank Details
    [MaxLength(50)]
    public string? BankAccountNumber { get; set; }

    [MaxLength(100)]
    public string? BankName { get; set; }
    [MaxLength(100)]
    public string? BankBranch { get; set; }

    [Required]
    public Gender Gender { get; set; }

    [Required, MaxLength(100)]
    public string Nationality { get; set; } = null!;

    [Required, MaxLength(50)]
    public string NationalId { get; set; } = null!;

    
    public MaritalStatus MaritalStatus { get; set; }

    // Employment Details
    [Required]
    public DateTime HireDate { get; set; }

    public DateTime? ProbationEndDate { get; set; }
    public DateTime? TerminationDate { get; set; }

    [Required]
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

    // Relationships
    [Required]
    [ForeignKey("Department")]
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    [Required]
    [ForeignKey("Position")]
    public Guid PositionId { get; set; }
    public JobPosition Position { get; set; } = null!;

    [ForeignKey("Manager")]
    public Guid? ReportsToId { get; set; }
    public Employee? Manager { get; set; }

    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();

    // Address Information
    [MaxLength(200)]
    public string? CurrentAddressLine { get; set; }

    [MaxLength(100)]
    public string? CurrentCity { get; set; } 

    [MaxLength(100)]
    public string? CurrentCountry { get; set; }

    [MaxLength(20)]
    public string? CurrentPostalCode { get; set; }

    // Salary
    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal Salary { get; set; }

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "EGP";


    // Profile
    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    // Collections
    public ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [MaxLength(100)]
    public string? ModifiedBy { get; set; }
}