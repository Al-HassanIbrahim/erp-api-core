using ERPSystem.Application.DTOs.HR.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Attendance
{
    public class AttendanceDetailDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string EmployeeName { get; set; } = null!;
        public DateOnly Date { get; set; }
        public TimeOnly? CheckInTime { get; set; }
        public TimeOnly? CheckOutTime { get; set; }
        public string Status { get; set; } = null!;
        public decimal WorkedHours { get; set; }
        public decimal OvertimeHours { get; set; }
        public string? Notes { get; set; }
        public bool IsManualEntry { get; set; }
        public EmployeeListDto? Employee { get; set; }
        public TimeOnly ExpectedCheckIn { get; set; }
        public TimeOnly ExpectedCheckOut { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyLeaveMinutes { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
