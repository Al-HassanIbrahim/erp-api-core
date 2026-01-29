using ERPSystem.Application.DTOs.HR;
using ERPSystem.Application.DTOs.HR.Employee;
using ERPSystem.Application.DTOs.HR.Leave;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.HR;
using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ERPSystem.Application.Services.Hr
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly ILeaveRequestRepository _leaveRepo;
        private readonly ILeaveBalanceRepository _balanceRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ICurrentUserService _currentUser;

        private const int MIN_NOTICE_DAYS = 2;
        private const int SICK_LEAVE_CERTIFICATE_THRESHOLD = 3;

        public LeaveRequestService(
            ILeaveRequestRepository leaveRepo,
            ILeaveBalanceRepository balanceRepo,
            IEmployeeRepository employeeRepo,
            ICurrentUserService currentUser)
        {
            _leaveRepo = leaveRepo;
            _balanceRepo = balanceRepo;
            _employeeRepo = employeeRepo;
            _currentUser = currentUser;
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
                throw new InvalidOperationException("Only active employees can request leave");
        }

        // ================== CREATE ==================

        public async Task<LeaveRequestDto> CreateAsync(CreateLeaveRequestDto dto)
        {
            var employee = await GetValidEmployeeAsync(dto.EmployeeId);
            EnsureEmployeeActive(employee);

            if (dto.StartDate < DateOnly.FromDateTime(DateTime.Today))
                throw new InvalidOperationException("Start date cannot be in the past");

            if (dto.EndDate < dto.StartDate)
                throw new InvalidOperationException("End date must be after start date");

            var totalDays = (dto.EndDate.DayNumber - dto.StartDate.DayNumber) + 1;

            if (dto.LeaveType == LeaveType.Annual)
            {
                var daysUntilStart = dto.StartDate.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;
                if (daysUntilStart < MIN_NOTICE_DAYS)
                    throw new InvalidOperationException($"Annual leave requires at least {MIN_NOTICE_DAYS} days notice");
            }

            var currentYear = dto.StartDate.Year;
            decimal currentBalance = 0;

            if (dto.LeaveType != LeaveType.Unpaid)
            {
                var balance = await _balanceRepo.GetByEmployeeYearAndTypeAsync(
                    dto.EmployeeId, currentYear, dto.LeaveType, _currentUser.CompanyId);

                if (balance == null)
                {
                    balance = new LeaveBalance
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = _currentUser.CompanyId,
                        EmployeeId = dto.EmployeeId,
                        Year = currentYear,
                        LeaveType = dto.LeaveType,
                        TotalEntitlement = GetDefaultEntitlement(dto.LeaveType),
                        Used = 0,
                        Pending = 0
                    };

                    await _balanceRepo.AddAsync(balance);
                }

                currentBalance = balance.Available;

                if (totalDays > currentBalance)
                    throw new InvalidOperationException(
                        $"Insufficient leave balance. Available: {currentBalance} days, Requested: {totalDays} days");
            }

            if (await _leaveRepo.HasOverlappingLeaveAsync(dto.EmployeeId, dto.StartDate, dto.EndDate, _currentUser.CompanyId))
                throw new InvalidOperationException("Leave request overlaps with existing approved leave");

            if (AreAllDaysWeekend(dto.StartDate, dto.EndDate))
                throw new InvalidOperationException("Cannot create leave request for only weekend days");

            var leaveRequest = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                CompanyId = _currentUser.CompanyId,
                EmployeeId = dto.EmployeeId,
                LeaveType = dto.LeaveType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TotalDays = totalDays,
                Status = LeaveRequestStatus.Pending,
                RequestDate = DateTime.UtcNow,
                Reason = dto.Reason,
                CurrentBalance = currentBalance,
                BalanceAfter = currentBalance - totalDays
            };

            await _leaveRepo.AddAsync(leaveRequest);

            if (dto.LeaveType != LeaveType.Unpaid)
            {
                var balance = await _balanceRepo.GetByEmployeeYearAndTypeAsync(
                    dto.EmployeeId, currentYear, dto.LeaveType, _currentUser.CompanyId);

                balance!.Pending += totalDays;
                await _balanceRepo.UpdateAsync(balance);
            }

            return MapToDto(leaveRequest, employee);
        }

        // ================== UPDATE ==================

        public async Task<LeaveRequestDto> UpdateAsync(Guid id, UpdateLeaveRequestDto dto)
        {
            var leaveRequest = await _leaveRepo.GetByIdWithDetailsAsync(id, _currentUser.CompanyId);
            if (leaveRequest == null)
                throw new InvalidOperationException("Leave request not found");

            // extra guard: employee must belong to same company
            var employee = await GetValidEmployeeAsync(leaveRequest.EmployeeId);

            if (leaveRequest.Status != LeaveRequestStatus.Pending)
                throw new InvalidOperationException("Only pending leave requests can be updated");

            var oldTotalDays = leaveRequest.TotalDays;
            var needsBalanceUpdate = false;

            if (dto.StartDate.HasValue || dto.EndDate.HasValue)
            {
                var newStartDate = dto.StartDate ?? leaveRequest.StartDate;
                var newEndDate = dto.EndDate ?? leaveRequest.EndDate;

                if (newStartDate < DateOnly.FromDateTime(DateTime.Today))
                    throw new InvalidOperationException("Start date cannot be in the past");

                if (newEndDate < newStartDate)
                    throw new InvalidOperationException("End date must be after start date");

                if (await _leaveRepo.HasOverlappingLeaveAsync(
                    leaveRequest.EmployeeId, newStartDate, newEndDate, _currentUser.CompanyId, excludeId: id))
                    throw new InvalidOperationException("Leave request overlaps with existing approved leave");

                var newTotalDays = (newEndDate.DayNumber - newStartDate.DayNumber) + 1;

                if (newTotalDays != oldTotalDays && leaveRequest.LeaveType != LeaveType.Unpaid)
                {
                    var currentYear = newStartDate.Year;

                    var balance = await _balanceRepo.GetByEmployeeYearAndTypeAsync(
                        leaveRequest.EmployeeId, currentYear, leaveRequest.LeaveType, _currentUser.CompanyId);

                    if (balance != null)
                    {
                        var additionalDays = newTotalDays - oldTotalDays;
                        if (additionalDays > 0 && additionalDays > balance.Available)
                            throw new InvalidOperationException("Insufficient leave balance for updated dates");

                        needsBalanceUpdate = true;
                    }
                }

                leaveRequest.StartDate = newStartDate;
                leaveRequest.EndDate = newEndDate;
                leaveRequest.TotalDays = newTotalDays;
            }

            if (!string.IsNullOrWhiteSpace(dto.Reason))
                leaveRequest.Reason = dto.Reason;

            await _leaveRepo.UpdateAsync(leaveRequest);

            if (needsBalanceUpdate && leaveRequest.LeaveType != LeaveType.Unpaid)
            {
                var balance = await _balanceRepo.GetByEmployeeYearAndTypeAsync(
                    leaveRequest.EmployeeId, leaveRequest.StartDate.Year, leaveRequest.LeaveType, _currentUser.CompanyId);

                if (balance != null)
                {
                    balance.Pending = balance.Pending - oldTotalDays + leaveRequest.TotalDays;
                    await _balanceRepo.UpdateAsync(balance);
                }
            }

            // use employee we validated (avoid trusting included nav if data inconsistent)
            return MapToDto(leaveRequest, employee);
        }

        // ================== APPROVE ==================

        public async Task ApproveAsync(Guid id, ApproveLeaveDto dto, string approvedBy)
        {
            var leaveRequest = await _leaveRepo.GetByIdWithDetailsAsync(id, _currentUser.CompanyId);
            if (leaveRequest == null)
                throw new InvalidOperationException("Leave request not found");

            await GetValidEmployeeAsync(leaveRequest.EmployeeId);

            if (leaveRequest.Status != LeaveRequestStatus.Pending)
                throw new InvalidOperationException("Only pending leave requests can be approved");

            leaveRequest.Status = LeaveRequestStatus.Approved;
            leaveRequest.ApprovedBy = approvedBy;
            leaveRequest.ApprovedDate = DateTime.UtcNow;

            await _leaveRepo.UpdateAsync(leaveRequest);

            if (leaveRequest.LeaveType != LeaveType.Unpaid)
            {
                var balance = await _balanceRepo.GetByEmployeeYearAndTypeAsync(
                    leaveRequest.EmployeeId, leaveRequest.StartDate.Year, leaveRequest.LeaveType, _currentUser.CompanyId);

                if (balance != null)
                {
                    balance.Pending -= leaveRequest.TotalDays;
                    balance.Used += leaveRequest.TotalDays;
                    await _balanceRepo.UpdateAsync(balance);
                }
            }
        }

        // ================== REJECT ==================

        public async Task RejectAsync(Guid id, RejectLeaveDto dto, string rejectedBy)
        {
            var leaveRequest = await _leaveRepo.GetByIdAsync(id, _currentUser.CompanyId);
            if (leaveRequest == null)
                throw new InvalidOperationException("Leave request not found");

            await GetValidEmployeeAsync(leaveRequest.EmployeeId);

            if (leaveRequest.Status != LeaveRequestStatus.Pending)
                throw new InvalidOperationException("Only pending leave requests can be rejected");

            if (string.IsNullOrWhiteSpace(dto.Reason))
                throw new InvalidOperationException("Rejection reason is required");

            leaveRequest.Status = LeaveRequestStatus.Rejected;
            leaveRequest.RejectedBy = rejectedBy;
            leaveRequest.RejectedDate = DateTime.UtcNow;

            await _leaveRepo.UpdateAsync(leaveRequest);

            if (leaveRequest.LeaveType != LeaveType.Unpaid)
            {
                var balance = await _balanceRepo.GetByEmployeeYearAndTypeAsync(
                    leaveRequest.EmployeeId, leaveRequest.StartDate.Year, leaveRequest.LeaveType, _currentUser.CompanyId);

                if (balance != null)
                {
                    balance.Pending -= leaveRequest.TotalDays;
                    await _balanceRepo.UpdateAsync(balance);
                }
            }
        }

        // ================== CANCEL ==================

        public async Task CancelAsync(Guid id, string reason, string cancelledBy)
        {
            var leaveRequest = await _leaveRepo.GetByIdAsync(id, _currentUser.CompanyId);
            if (leaveRequest == null)
                throw new InvalidOperationException("Leave request not found");

            await GetValidEmployeeAsync(leaveRequest.EmployeeId);

            if (leaveRequest.Status != LeaveRequestStatus.Approved)
                throw new InvalidOperationException("Can only cancel approved leave requests");

            if (leaveRequest.StartDate <= DateOnly.FromDateTime(DateTime.Today))
                throw new InvalidOperationException("Cannot cancel leave that has already started");

            if (string.IsNullOrWhiteSpace(reason))
                throw new InvalidOperationException("Cancellation reason is required");

            leaveRequest.Status = LeaveRequestStatus.Cancelled;
            leaveRequest.CancelledBy = cancelledBy;
            leaveRequest.CancelledDate = DateTime.UtcNow;

            await _leaveRepo.UpdateAsync(leaveRequest);

            if (leaveRequest.LeaveType != LeaveType.Unpaid)
            {
                var balance = await _balanceRepo.GetByEmployeeYearAndTypeAsync(
                    leaveRequest.EmployeeId, leaveRequest.StartDate.Year, leaveRequest.LeaveType, _currentUser.CompanyId);

                if (balance != null)
                {
                    balance.Used -= leaveRequest.TotalDays;
                    await _balanceRepo.UpdateAsync(balance);
                }
            }
        }

        // ================== READ ==================

        public async Task<LeaveRequestDetailDto?> GetByIdAsync(Guid id)
        {
            var leaveRequest = await _leaveRepo.GetByIdWithDetailsAsync(id, _currentUser.CompanyId);
            return leaveRequest != null ? MapToDetailDto(leaveRequest) : null;
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetByEmployeeIdAsync(Guid employeeId)
        {
            var employee = await GetValidEmployeeAsync(employeeId);

            var leaveRequests = await _leaveRepo.GetByEmployeeIdAsync(employeeId, _currentUser.CompanyId);
            return leaveRequests.Select(lr => MapToDto(lr, employee));
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetPendingAsync()
        {
            var leaveRequests = await _leaveRepo.GetPendingAsync(_currentUser.CompanyId);
            return leaveRequests.Select(lr => MapToDto(lr, lr.Employee));
        }

        public async Task<LeaveBalanceDto> GetBalanceAsync(Guid employeeId, int year)
        {
            var employee = await GetValidEmployeeAsync(employeeId);

            var balances = await _balanceRepo.GetByEmployeeAndYearAsync(employeeId, year, _currentUser.CompanyId);

            var allLeaveTypes = Enum.GetValues<LeaveType>().Where(lt => lt != LeaveType.Unpaid);
            foreach (var leaveType in allLeaveTypes)
            {
                if (!balances.Any(b => b.LeaveType == leaveType))
                {
                    var newBalance = new LeaveBalance
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = _currentUser.CompanyId,
                        EmployeeId = employeeId,
                        Year = year,
                        LeaveType = leaveType,
                        TotalEntitlement = GetDefaultEntitlement(leaveType),
                        Used = 0,
                        Pending = 0
                    };
                    await _balanceRepo.AddAsync(newBalance);
                    balances = balances.Append(newBalance);
                }
            }

            return new LeaveBalanceDto
            {
                EmployeeId = employeeId,
                EmployeeName = employee.FullName,
                Year = year,
                Balances = balances.Select(b => new LeaveTypeBalance
                {
                    LeaveType = b.LeaveType.ToString(),
                    TotalEntitlement = b.TotalEntitlement,
                    Used = b.Used,
                    Pending = b.Pending,
                    Available = b.Available
                }).ToList()
            };
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetHistoryAsync(
            Guid employeeId,
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            LeaveType? leaveType = null)
        {
            var employee = await GetValidEmployeeAsync(employeeId);

            var allLeaves = await _leaveRepo.GetByEmployeeIdAsync(employeeId, _currentUser.CompanyId);

            var filteredLeaves = allLeaves.AsEnumerable();

            if (startDate.HasValue)
                filteredLeaves = filteredLeaves.Where(lr => lr.StartDate >= startDate.Value);

            if (endDate.HasValue)
                filteredLeaves = filteredLeaves.Where(lr => lr.EndDate <= endDate.Value);

            if (leaveType.HasValue)
                filteredLeaves = filteredLeaves.Where(lr => lr.LeaveType == leaveType.Value);

            return filteredLeaves.Select(lr => MapToDto(lr, employee));
        }

        // ================== DELETE ==================

        public async Task DeleteAsync(Guid id)
        {
            var leaveRequest = await _leaveRepo.GetByIdAsync(id, _currentUser.CompanyId);
            if (leaveRequest == null)
                throw new InvalidOperationException("Leave request not found");

            await GetValidEmployeeAsync(leaveRequest.EmployeeId);

            if (leaveRequest.Status != LeaveRequestStatus.Pending)
                throw new InvalidOperationException("Only pending leave requests can be deleted");

            if (leaveRequest.LeaveType != LeaveType.Unpaid)
            {
                var balance = await _balanceRepo.GetByEmployeeYearAndTypeAsync(
                    leaveRequest.EmployeeId, leaveRequest.StartDate.Year, leaveRequest.LeaveType, _currentUser.CompanyId);

                if (balance != null)
                {
                    balance.Pending -= leaveRequest.TotalDays;
                    await _balanceRepo.UpdateAsync(balance);
                }
            }

            await _leaveRepo.DeleteAsync(id, _currentUser.CompanyId);
        }

        // ================== HELPERS ==================

        private decimal GetDefaultEntitlement(LeaveType type)
        {
            return type switch
            {
                LeaveType.Annual => 21,
                LeaveType.Sick => 14,
                LeaveType.Emergency => 3,
                LeaveType.Maternity => 90,
                LeaveType.Paternity => 3,
                LeaveType.Study => 7,
                _ => 0
            };
        }

        private bool AreAllDaysWeekend(DateOnly startDate, DateOnly endDate)
        {
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dayOfWeek = date.DayOfWeek;
                if (dayOfWeek != DayOfWeek.Friday && dayOfWeek != DayOfWeek.Saturday)
                    return false;
            }
            return true;
        }

        private LeaveRequestDto MapToDto(LeaveRequest lr, Employee e)
        {
            return new LeaveRequestDto
            {
                Id = lr.Id,
                EmployeeId = lr.EmployeeId,
                EmployeeCode = e.EmployeeCode,
                EmployeeName = e.FullName,
                LeaveType = lr.LeaveType.ToString(),
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                TotalDays = lr.TotalDays,
                Status = lr.Status.ToString(),
                RequestDate = lr.RequestDate,
                Reason = lr.Reason
            };
        }

        private LeaveRequestDetailDto MapToDetailDto(LeaveRequest lr)
        {
            return new LeaveRequestDetailDto
            {
                Id = lr.Id,
                EmployeeId = lr.EmployeeId,
                EmployeeCode = lr.Employee.EmployeeCode,
                EmployeeName = lr.Employee.FullName,
                LeaveType = lr.LeaveType.ToString(),
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                TotalDays = lr.TotalDays,
                Status = lr.Status.ToString(),
                RequestDate = lr.RequestDate,
                Reason = lr.Reason,
                Employee = new EmployeeListDto
                {
                    Id = lr.Employee.Id,
                    EmployeeCode = lr.Employee.EmployeeCode,
                    FullName = lr.Employee.FullName,
                    Email = lr.Employee.Email,
                    PhoneNumber = lr.Employee.PhoneNumber,
                    DepartmentName = lr.Employee.Department?.Name,
                    PositionTitle = lr.Employee.Position?.Title,
                    Status = lr.Employee.Status.ToString(),
                    HireDate = lr.Employee.HireDate,
                    ProfileImageUrl = lr.Employee.ProfileImageUrl
                },
                ApprovedBy = lr.ApprovedBy,
                ApprovedDate = lr.ApprovedDate,
                RejectedBy = lr.RejectedBy,
                RejectedDate = lr.RejectedDate,
                CancelledBy = lr.CancelledBy,
                CancelledDate = lr.CancelledDate,
                CurrentBalance = lr.CurrentBalance,
                BalanceAfter = lr.BalanceAfter,
                Attachments = lr.Attachments.Select(a => new DocumentDto
                {
                    Id = a.Id,
                    DocumentName = a.FileName,
                    FilePath = a.FilePath,
                    UploadedAt = a.UploadedAt
                }).ToList()
            };
        }
    }
}
