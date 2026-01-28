using ERPSystem.Application.DTOs.HR.Attendance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ERPSystem.Application.DTOs.HR.Attendance.Check;

namespace ERPSystem.Application.Interfaces
{
    public interface IAttendanceService
    {
        Task<AttendanceDto> CheckInAsync(CheckInDto dto, string createdBy);

        Task<AttendanceDto> CheckOutAsync(CheckOutDto dto, string modifiedBy);

        Task<AttendanceDto> CreateManualEntryAsync(ManualAttendanceDto dto, string createdBy);

        Task<AttendanceDto> UpdateAsync(Guid id, UpdateAttendanceDto dto, string modifiedBy);

        Task<AttendanceSummaryDto> GetSummaryAsync(Guid employeeId, int month, int year);
    }
}
