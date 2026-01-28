using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Attendance
{
    public class ManualAttendanceDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        public TimeOnly? CheckInTime { get; set; }
        public TimeOnly? CheckOutTime { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        [Required, MaxLength(500)]
        public string Notes { get; set; } = null!;
    }
}
