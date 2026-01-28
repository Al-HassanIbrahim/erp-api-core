using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Department
{
    public class CreateDepartmentDto
    {
        [Required, MaxLength(20)]
        public string Code { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public Guid? ManagerId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
