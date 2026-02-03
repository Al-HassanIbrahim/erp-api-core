using ERPSystem.Application.DTOs.HR.Attendance;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.HR;
using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ERPSystem.Application.DTOs.HR.Attendance.Check;

namespace ERPSystem.Application.Services.Hr
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ICurrentUserService _currentUser;
        private readonly IModuleAccessService _moduleAccess;

        private readonly TimeOnly EXPECTED_CHECK_IN = new TimeOnly(9, 0);  // 9:00 AM
        private readonly TimeOnly EXPECTED_CHECK_OUT = new TimeOnly(17, 0); // 5:00 PM
        private readonly int LATE_THRESHOLD_MINUTES = 15;
        private readonly int STANDARD_WORK_HOURS = 8;

        public AttendanceService(
            IAttendanceRepository attendanceRepo,
            IEmployeeRepository employeeRepo,
            ICurrentUserService currentUser,
            IModuleAccessService moduleAccess)
        {
            _attendanceRepo = attendanceRepo;
            _employeeRepo = employeeRepo;
            _currentUser = currentUser;
            _moduleAccess = moduleAccess;
        }

        // ================== GUARDS ==================

        private async Task<Employee> GetValidEmployeeAsync(Guid employeeId, CancellationToken ct = default)
        {
            var employee = await _employeeRepo.GetByIdAsync(employeeId, _currentUser.CompanyId, ct);
            if (employee == null)
                throw new InvalidOperationException("Employee not found or does not belong to your company.");

            return employee;
        }

        private void EnsureEmployeeActive(Employee employee)
        {
            if (employee.Status != EmployeeStatus.Active)
                throw new InvalidOperationException($"Employee is {employee.Status}. Cannot proceed.");
        }

        // ================== CHECK IN ==================

        public async Task<AttendanceDto> CheckInAsync(CheckInDto dto, string createdBy, CancellationToken ct = default)
        {
            await _moduleAccess.EnsureHrAccessAsync(ct);
            var employee = await GetValidEmployeeAsync(dto.EmployeeId);
            EnsureEmployeeActive(employee);

            var date = DateOnly.FromDateTime(dto.CheckInTime ?? DateTime.Now);

            if (date > DateOnly.FromDateTime(DateTime.Today))
                throw new InvalidOperationException("Cannot check-in for future dates");

            // company-scoped
            if (await _attendanceRepo.HasCheckedInTodayAsync(dto.EmployeeId, date, _currentUser.CompanyId))
                throw new InvalidOperationException("Already checked in today");

            var checkInTime = TimeOnly.FromDateTime(dto.CheckInTime ?? DateTime.Now);

            var lateMinutes = (checkInTime - EXPECTED_CHECK_IN).TotalMinutes;
            var status = lateMinutes > LATE_THRESHOLD_MINUTES
                ? AttendanceStatus.Late
                : AttendanceStatus.Present;

            var attendance = new Attendance
            {
                Id = Guid.NewGuid(),
                CompanyId = _currentUser.CompanyId, // ✅ important (even if BaseRepo enforces)
                EmployeeId = dto.EmployeeId,
                Date = date,
                CheckInTime = checkInTime,
                Status = status,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            await _attendanceRepo.AddAsync(attendance);
            return MapToDto(attendance, employee);
        }

        // ================== CHECK OUT ==================

        public async Task<AttendanceDto> CheckOutAsync(CheckOutDto dto, string modifiedBy, CancellationToken ct = default)
        {
            await _moduleAccess.EnsureHrAccessAsync(ct);
            var employee = await GetValidEmployeeAsync(dto.EmployeeId);

            var date = DateOnly.FromDateTime(dto.CheckOutTime ?? DateTime.Now);

            // company-scoped
            var attendance = await _attendanceRepo.GetByEmployeeAndDateAsync(
                dto.EmployeeId, date, _currentUser.CompanyId);

            if (attendance == null || attendance.CheckInTime == null)
                throw new InvalidOperationException("No check-in record found for today");

            if (attendance.CheckOutTime != null)
                throw new InvalidOperationException("Already checked out today");

            var checkOutTime = TimeOnly.FromDateTime(dto.CheckOutTime ?? DateTime.Now);

            if (checkOutTime <= attendance.CheckInTime)
                throw new InvalidOperationException("Check-out time must be after check-in time");

            attendance.CheckOutTime = checkOutTime;

            var totalMinutes = (checkOutTime - attendance.CheckInTime.Value).TotalMinutes;
            var workedHours = (decimal)(totalMinutes - 30) / 60; // Deduct 30 min break

            attendance.WorkedHours = Math.Max(0, workedHours);
            attendance.OvertimeHours = Math.Max(0, attendance.WorkedHours - STANDARD_WORK_HOURS);

            attendance.ModifiedAt = DateTime.UtcNow;
            attendance.ModifiedBy = modifiedBy;

            await _attendanceRepo.UpdateAsync(attendance);
            return MapToDto(attendance, employee);
        }

        // ================== MANUAL ENTRY ==================

        public async Task<AttendanceDto> CreateManualEntryAsync(ManualAttendanceDto dto, string createdBy, CancellationToken ct = default)
        {
            await _moduleAccess.EnsureHrAccessAsync(ct);
            var employee = await GetValidEmployeeAsync(dto.EmployeeId);

            if (dto.Date > DateOnly.FromDateTime(DateTime.Today))
                throw new InvalidOperationException("Cannot create manual entry for future dates");

            if (await _attendanceRepo.IsPayrollProcessedForPeriodAsync(dto.EmployeeId, dto.Date, _currentUser.CompanyId))
                throw new InvalidOperationException("Cannot create manual entry. Payroll already processed for this period");

            var existing = await _attendanceRepo.GetByEmployeeAndDateAsync(dto.EmployeeId, dto.Date, _currentUser.CompanyId);
            if (existing != null)
                throw new InvalidOperationException("Attendance record already exists for this date");

            var attendance = new Attendance
            {
                Id = Guid.NewGuid(),
                CompanyId = _currentUser.CompanyId,
                EmployeeId = dto.EmployeeId,
                Date = dto.Date,
                CheckInTime = dto.CheckInTime,
                CheckOutTime = dto.CheckOutTime,
                Status = dto.Status,
                Notes = dto.Notes,
                IsManualEntry = true,
                ManualEntryReason = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            if (dto.CheckInTime.HasValue && dto.CheckOutTime.HasValue)
            {
                var totalMinutes = (dto.CheckOutTime.Value - dto.CheckInTime.Value).TotalMinutes;
                var workedHours = (decimal)(totalMinutes - 30) / 60;
                attendance.WorkedHours = Math.Max(0, workedHours);
                attendance.OvertimeHours = Math.Max(0, attendance.WorkedHours - STANDARD_WORK_HOURS);
            }

            await _attendanceRepo.AddAsync(attendance);
            return MapToDto(attendance, employee);
        }

        // ================== UPDATE ==================

        public async Task<AttendanceDto> UpdateAsync(Guid id, UpdateAttendanceDto dto, string modifiedBy, CancellationToken ct = default)
        {
            await _moduleAccess.EnsureHrAccessAsync(ct);
            // company-scoped
            var attendance = await _attendanceRepo.GetByIdAsync(id, _currentUser.CompanyId);
            if (attendance == null)
                throw new InvalidOperationException("Attendance record not found");

            // Verify employee belongs to company (prevents tampering via attendanceId)
            var employee = await GetValidEmployeeAsync(attendance.EmployeeId);

            if (await _attendanceRepo.IsPayrollProcessedForPeriodAsync(attendance.EmployeeId, attendance.Date, _currentUser.CompanyId))
                throw new InvalidOperationException("Cannot modify attendance. Payroll already processed");

            if (dto.CheckInTime.HasValue)
                attendance.CheckInTime = dto.CheckInTime;

            if (dto.CheckOutTime.HasValue)
            {
                if (attendance.CheckInTime == null)
                    throw new InvalidOperationException("Cannot set check-out without check-in");

                if (dto.CheckOutTime.Value <= attendance.CheckInTime)
                    throw new InvalidOperationException("Check-out must be after check-in");

                attendance.CheckOutTime = dto.CheckOutTime;
            }

            if (dto.Status.HasValue)
                attendance.Status = dto.Status.Value;

            if (dto.Notes != null)
                attendance.Notes = dto.Notes;

            if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
            {
                var totalMinutes = (attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalMinutes;
                var workedHours = (decimal)(totalMinutes - 30) / 60;
                attendance.WorkedHours = Math.Max(0, workedHours);
                attendance.OvertimeHours = Math.Max(0, attendance.WorkedHours - STANDARD_WORK_HOURS);
            }

            attendance.ModifiedAt = DateTime.UtcNow;
            attendance.ModifiedBy = modifiedBy;

            await _attendanceRepo.UpdateAsync(attendance);
            return MapToDto(attendance, employee);
        }

        // ================== SUMMARY ==================

        public async Task<AttendanceSummaryDto> GetSummaryAsync(Guid employeeId, int month, int year, CancellationToken ct = default)
        {
            await _moduleAccess.EnsureHrAccessAsync(ct);
            var employee = await GetValidEmployeeAsync(employeeId);

            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var attendances = await _attendanceRepo.GetByEmployeeAndPeriodAsync(
                employeeId, startDate, endDate, _currentUser.CompanyId);

            var totalWorkingDays = CalculateWorkingDays(startDate, endDate);
            var presentDays = attendances.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late);
            var absentDays = attendances.Count(a => a.Status == AttendanceStatus.Absent);
            var lateDays = attendances.Count(a => a.Status == AttendanceStatus.Late);
            var leaveDays = attendances.Count(a => a.Status == AttendanceStatus.OnLeave);

            var totalWorkedHours = attendances.Sum(a => a.WorkedHours);
            var totalOvertimeHours = attendances.Sum(a => a.OvertimeHours);
            var attendanceRate = totalWorkingDays > 0
                ? (decimal)presentDays / totalWorkingDays * 100
                : 0;

            return new AttendanceSummaryDto
            {
                EmployeeId = employeeId,
                EmployeeName = employee.FullName,
                Month = month,
                Year = year,
                TotalWorkingDays = totalWorkingDays,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                LateDays = lateDays,
                LeaveDays = leaveDays,
                TotalWorkedHours = totalWorkedHours,
                TotalOvertimeHours = totalOvertimeHours,
                AttendanceRate = Math.Round(attendanceRate, 2)
            };
        }

        // ================== HELPERS ==================

        private int CalculateWorkingDays(DateOnly start, DateOnly end)
        {
            int workingDays = 0;
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                var dayOfWeek = date.DayOfWeek;
                if (dayOfWeek != DayOfWeek.Friday && dayOfWeek != DayOfWeek.Saturday)
                    workingDays++;
            }
            return workingDays;
        }

        private AttendanceDto MapToDto(Attendance a, Employee e)
        {
            return new AttendanceDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeCode = e.EmployeeCode,
                EmployeeName = e.FullName,
                Date = a.Date,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                Status = a.Status.ToString(),
                WorkedHours = a.WorkedHours,
                OvertimeHours = a.OvertimeHours,
                Notes = a.Notes,
                IsManualEntry = a.IsManualEntry
            };
        }
    }
}
