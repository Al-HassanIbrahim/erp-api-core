using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Attendance
{
    public class UpdateAttendanceDto
    {
        public TimeOnly? CheckInTime { get; set; }
        public TimeOnly? CheckOutTime { get; set; }
        public AttendanceStatus? Status { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required, MaxLength(500)]
        public string Reason { get; set; } = null!;
    }
}
