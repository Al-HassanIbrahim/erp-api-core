using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Attendance
{
    public class Check
    {
        public class CheckInDto
        {
            [Required]
            public Guid EmployeeId { get; set; }

            public DateTime? CheckInTime { get; set; }

            [MaxLength(500)]
            public string? Notes { get; set; }

            [MaxLength(200)]
            public string? Location { get; set; }
        }

        public class CheckOutDto
        {
            [Required]
            public Guid EmployeeId { get; set; }

            public DateTime? CheckOutTime { get; set; }

            [MaxLength(500)]
            public string? Notes { get; set; }
        }
    }
}
