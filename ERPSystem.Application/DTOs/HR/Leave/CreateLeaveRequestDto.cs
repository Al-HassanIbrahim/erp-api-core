using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Leave
{
    public class CreateLeaveRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public LeaveType LeaveType { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        [Required, MaxLength(500)]
        public string Reason { get; set; } = null!;
    }
}
