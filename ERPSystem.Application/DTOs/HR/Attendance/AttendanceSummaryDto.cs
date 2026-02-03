using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Attendance
{
    public class AttendanceSummaryDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalWorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LateDays { get; set; }
        public int LeaveDays { get; set; }
        public decimal TotalWorkedHours { get; set; }
        public decimal TotalOvertimeHours { get; set; }
        public decimal AttendanceRate { get; set; }
    }
}
