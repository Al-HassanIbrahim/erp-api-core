using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Entities.HR
{
    public class JobPosition
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [ForeignKey("Employee")]
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
