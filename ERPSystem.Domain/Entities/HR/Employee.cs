// Employee.cs  ← هنضيفله حقل واحد بس
using ERPSystem.Domain.Entities.HR;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Employee
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = null!;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = null!;
    public string FullName => $"{FirstName} {LastName}".Trim();

    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    public DateTime HireDate { get; set; } = DateTime.Today;

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal BasicSalary { get; set; }

    // Common Relation
    [ForeignKey("Department")]
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    [ForeignKey("JobPosition")]
    public int? JobPositionId { get; set; }
    public JobPosition? JobPosition { get; set; }

    [ForeignKey("Manager")]
    public int? ManagerId { get; set; }
    public Employee? Manager { get; set; }

    public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}