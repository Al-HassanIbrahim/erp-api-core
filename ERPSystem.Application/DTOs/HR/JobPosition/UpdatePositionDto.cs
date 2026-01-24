using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.JobPosition
{
    public class UpdatePositionDto
    {
        [Required, MaxLength(100)]
        public string Title { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public PositionLevel Level { get; set; }

        public Guid? DepartmentId { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal MinSalary { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal MaxSalary { get; set; }

        public bool IsActive { get; set; }
    }
}
