using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Enums;
namespace ERPSystem.Domain.Entities.HR
{
    public class JobPosition
    {
        public Guid Id { get; set; }

        [Required, MaxLength(20)]
        public string Code { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Title { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public PositionLevel Level { get; set; }

        // Relations
        [ForeignKey("Department")]
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal MinSalary { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal MaxSalary { get; set; }

        public bool IsActive { get; set; } = true;

        // Collections
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
    }
}
