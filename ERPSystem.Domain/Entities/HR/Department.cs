using ERPSystem.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Entities.HR
{
    public class Department:ICompanyEntity
    {
        public Guid Id { get; set; }
        public int CompanyId { get; set; }

        [Required, MaxLength(20)]
        public string Code { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        // Relations
        [ForeignKey("Manager")]
        public Guid? ManagerId { get; set; }
        public Employee? Manager { get; set; }

        public bool IsActive { get; set; } = true;

        // Collections
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<JobPosition> Positions { get; set; } = new List<JobPosition>();

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
    }
}
