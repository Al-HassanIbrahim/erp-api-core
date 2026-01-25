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
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.Services.Hr
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly ILeaveRequestRepository _leaveRepo;
        private readonly ILeaveBalanceRepository _balanceRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private const int MIN_NOTICE_DAYS = 2;
        private const int SICK_LEAVE_CERTIFICATE_THRESHOLD = 3;

        public LeaveRequestService(
            ILeaveRequestRepository leaveRepo,
            ILeaveBalanceRepository balanceRepo,
            IEmployeeRepository employeeRepo)
        {
            _leaveRepo = leaveRepo;
            _balanceRepo = balanceRepo;
            _employeeRepo = employeeRepo;
        }

        public async Task<LeaveRequestDto> CreateAsync(CreateLeaveRequestDto dto)
        {
            var employee = await _employeeRepo.GetByIdAsync(dto.EmployeeId);
            if (employee == null)
                throw new InvalidOperationException("Employee not found");

            if (employee.Status != EmployeeStatus.Active)
                throw new InvalidOperationException("Only active employees can request leave");

            // Validation: Start date cannot be in the past
            if (dto.StartDate < DateOnly.FromDateTime(DateTime.Today))
                throw new InvalidOperationException("Start date cannot be in the past");

            // Validation: End date must be after start date
            if (dto.EndDate < dto.StartDate)
                throw new InvalidOperationException("End date must be after start date");

            // Calculate total days
            var totalDays = (dto.EndDate.DayNumber - dto.StartDate.DayNumber) + 1;

            // Validation: Annual leave requires minimum 2 days notice
            if (dto.LeaveType == LeaveType.Annual)
            {
                var daysUntilStart = dto.StartDate.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;
                if (daysUntilStart < MIN_NOTICE_DAYS)
                    throw new InvalidOperationException($"Annual leave requires at least {MIN_NOTICE_DAYS} days notice");
            }

            // Validation: Sick leave > 3 days requires medical certificate
            if (dto.LeaveType == LeaveType.Sick && totalDays > SICK_LEAVE_CERTIFICATE_THRESHOLD)
            {
                // Note: File attachment validation should be done in controller/UI
                // For now, we just document this requirement
            }

            // Check leave balance (except unpaid leave)
            var currentYear = dto.StartDate.Year;
            decimal currentBalance = 0;

            if (dto.LeaveType != LeaveType.Unpaid)
            {
                var balance = await _balanceRepo.GetByEmployeeYearAndTypeAsync(
                    dto.EmployeeId, currentYear, dto.LeaveType);

                if (balance == null)
                {
                    // Initialize balance if not exists
                    balance = new LeaveBalance
                    {
                        Id = Guid.NewGuid(),
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

                // Validation: Cannot exceed available balance
                if (totalDays > currentBalance)
                    throw new InvalidOperationException(
                        $"Insufficient leave balance. Available: {currentBalance} days, Requested: {totalDays} days");
            }

            // Validation: Cannot overlap with existing approved leave
            if (await _leaveRepo.HasOverlappingLeaveAsync(dto.EmployeeId, dto.StartDate, dto.EndDate))
                throw new InvalidOperationException("Leave request overlaps with existing approved leave");

            // Validation: Cannot create leave for non-working days (weekends)
            // Check if all days are weekends
            if (AreAllDaysWeekend(dto.StartDate, dto.EndDate))
                throw new InvalidOperationException("Cannot create leave request for only weekend days");

            // Create leave request
            var leaveRequest = new LeaveRequest
            {
                Id = Guid.NewGuid(),
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

            // Update pending balance
            if (dto.LeaveType != LeaveType.Unpaid)
            {
                var balance = await _balanceRepo.GetByEmployeeYearAndTypeAsync(
                    dto.EmployeeId, currentYear, dto.LeaveType);
                balance!.Pending += totalDays;
                await _balanceRepo.UpdateAsync(balance);
            }

            return MapToDto(leaveRequest, employee);
        }

        public async Task<LeaveRequestDto> UpdateAsync(Guid id, UpdateLeaveRequestDto dto)
        {
            var leaveRequest = await _leaveRepo.GetByIdWithDetailsAsync(id);
            if (leaveRequest == null)
                throw new InvalidOperationException("Leave request not found");

            // Validation: Only pending requests can be updated
            if (leaveRequest.Status != LeaveRequestStatus.Pending)
                throw new InvalidOperationException("Only pending leave requests can be updated");

            var oldTotalDays = leaveRequest.TotalDays;
            var needsBalanceUpdate = false;

            // Update dates if provided
            if (dto.StartDate.HasValue || dto.EndDate.HasValue)
            {
                var newStartDate = dto.StartDate ?? leaveRequest.StartDate;
                var newEndDate = dto.EndDate ?? leaveRequest.EndDate;

                // Validation
                if (newStartDate < DateOnly.FromDateTime(DateTime.Today))
                    throw new InvalidOperationException("Start date cannot be in the past");

                if (newEndDate < newStartDate)
                    throw new InvalidOperationException("End date must be after start date");

                // Check for overlaps (excluding this request)
                if (await _leaveRepo.HasOverlappingLeaveAsync(
                    leaveRequest.EmployeeId, newStartDate, newEndDate, id))
                    throw new InvalidOperationException("Leave request overlaps with existing approved leave");

                var newTotalDays = (newEndDate.DayNumber - newStartDate.DayNumber) + 1;

                // Update balance if days changed
                if (newTotalDays != oldTotalDays)
                {
                    var currentYear = newStartDate.Year;
                    var balance = await _balanceRepo.GetByEmployeeYearAndTypeAsync(
                        leaveRequest.EmployeeId, currentYear, leaveRequest.LeaveType);

                    if (balance != null)
                    {
                        // Check if new total exceeds available
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

            // Update reason if provided
            if (!string.IsNullOrWhiteSpace(dto.Reason))
            {
                leaveRequest.Reason = dto.Reason;
            }

            await _leaveRepo.UpdateAsync(leaveRequest);

            // Update pending balance if needed
            if (needsBalanceUpdate && leaveRequest.LeaveType != LeaveType.Unpaid)
            {
                var balance = await _balanceRepo.GetByEmployeeYearAndTypeAsync(
                    leaveRequest.EmployeeId, leaveRequest.StartDate.Year, leaveRequest.LeaveType);

                if (balance != null)
                {
                    balance.Pending = balance.Pending - oldTotalDays + leaveRequest.TotalDays;
                    await _balanceRepo.UpdateAsync(balance);
                }
            }

            return MapToDto(leaveRequest, leaveRequest.Employee);
        }

        public async Task ApproveAsync(Guid id, ApproveLeaveDto dto, string approvedBy)
        {
            var leaveRequest = await _leaveRepo.GetByIdWithDetailsAsync(id);
            if (leaveRequest == null)
                throw new InvalidOperationException("Leave request not found");

            // Validation: Only pending requests can be approved
            if (leaveRequest.Status != LeaveRequestStatus.Pending)
                throw new InvalidOperationException("Only pending leave requests can be approved");

            // Validation: Cannot approve own leave (if approvedBy is employee)
            // This check would need user context in real implementation
            // if (leaveRequest.EmployeeId.ToString() == approvedBy)
            //     throw new InvalidOperationException("Cannot approve your own leave request");

            leaveRequest.Status = LeaveRequestStatus.Approved;
            leaveRequest.ApprovedBy = approvedBy;
            leaveRequest.ApprovedDate = DateTime.UtcNow;

            await _leaveRepo.UpdateAsync(leaveRequest);

            // Deduct from balance and update used
            if (leaveRequest.LeaveType != LeaveType.Unpaid)
            {
                var balance = await _balanceRepo.GetByEmployeeYearAndTypeAsync(
                    leaveRequest.EmployeeId, leaveRequest.StartDate.Year, leaveRequest.LeaveType);

                if (balance != null)
                {
                    balance.Pending -= leaveRequest.TotalDays;
                    balance.Used += leaveRequest.TotalDays;
                    await _balanceRepo.UpdateAsync(balance);
                }
            }

            // Optional: Mark employee as OnLeave for those dates
            // This could be handled by a background job or during attendance checks
        }

        public async Task RejectAsync(Guid id, RejectLeaveDto dto, string rejectedBy)
        {
            var leaveRequest = await _leaveRepo.GetByIdAsync(id);
            if (leaveRequest == null)
                throw new InvalidOperationException("Leave request not found");

            // Validation: Only pending requests can be rejected
            if (leaveRequest.Status != LeaveRequestStatus.Pending)
                throw new InvalidOperationException("Only pending leave requests can be rejected");

            // Validation: Rejection reason is required
            if (string.IsNullOrWhiteSpace(dto.Reason))
                throw new InvalidOperationException("Rejection reason is required");

            leaveRequest.Status = LeaveRequestStatus.Rejected;
            leaveRequest.RejectedBy = rejectedBy;
            leaveRequest.RejectedDate = DateTime.UtcNow;

            await _leaveRepo.UpdateAsync(leaveRequest);

            // Return pending balance
            if (leaveRequest.LeaveType != LeaveType.Unpaid)
            {
                var balance = await _balanceRepo.GetByEmployeeYearAndTypeAsync(
                    leaveRequest.EmployeeId, leaveRequest.StartDate.Year, leaveRequest.LeaveType);

                if (balance != null)
                {
                    balance.Pending -= leaveRequest.TotalDays;
                    await _balanceRepo.UpdateAsync(balance);
                }
            }
        }

        public async Task CancelAsync(Guid id, string reason, string cancelledBy)
        {
            var leaveRequest = await _leaveRepo.GetByIdAsync(id);
            if (leaveRequest == null)
                throw new InvalidOperationException("Leave request not found");

            // Validation: Can only cancel approved leaves before start date
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

            // Return balance to employee
            if (leaveRequest.LeaveType != LeaveType.Unpaid)
            {
                var balance = await _balanceRepo.GetByEmployeeYearAndTypeAsync(
                    leaveRequest.EmployeeId, leaveRequest.StartDate.Year, leaveRequest.LeaveType);

                if (balance != null)
                {
                    balance.Used -= leaveRequest.TotalDays;
                    await _balanceRepo.UpdateAsync(balance);
                }
            }
        }

        public async Task<LeaveRequestDetailDto?> GetByIdAsync(Guid id)
        {
            var leaveRequest = await _leaveRepo.GetByIdWithDetailsAsync(id);
            return leaveRequest != null ? MapToDetailDto(leaveRequest) : null;
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetByEmployeeIdAsync(Guid employeeId)
        {
            var employee = await _employeeRepo.GetByIdAsync(employeeId);
            if (employee == null)
                throw new InvalidOperationException("Employee not found");

            var leaveRequests = await _leaveRepo.GetByEmployeeIdAsync(employeeId);
            return leaveRequests.Select(lr => MapToDto(lr, employee));
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetPendingAsync()
        {
            var leaveRequests = await _leaveRepo.GetPendingAsync();
            return leaveRequests.Select(lr => MapToDto(lr, lr.Employee));
        }

        

        public async Task<LeaveBalanceDto> GetBalanceAsync(Guid employeeId, int year)
        {
            var employee = await _employeeRepo.GetByIdAsync(employeeId);
            if (employee == null)
                throw new InvalidOperationException("Employee not found");

            var balances = await _balanceRepo.GetByEmployeeAndYearAsync(employeeId, year);

            // Initialize missing balances for all leave types
            var allLeaveTypes = Enum.GetValues<LeaveType>().Where(lt => lt != LeaveType.Unpaid);
            foreach (var leaveType in allLeaveTypes)
            {
                if (!balances.Any(b => b.LeaveType == leaveType))
                {
                    var newBalance = new LeaveBalance
                    {
                        Id = Guid.NewGuid(),
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
            var employee = await _employeeRepo.GetByIdAsync(employeeId);
            if (employee == null)
                throw new InvalidOperationException("Employee not found");

            var allLeaves = await _leaveRepo.GetByEmployeeIdAsync(employeeId);

            // Apply filters
            var filteredLeaves = allLeaves.AsEnumerable();

            if (startDate.HasValue)
                filteredLeaves = filteredLeaves.Where(lr => lr.StartDate >= startDate.Value);

            if (endDate.HasValue)
                filteredLeaves = filteredLeaves.Where(lr => lr.EndDate <= endDate.Value);

            if (leaveType.HasValue)
                filteredLeaves = filteredLeaves.Where(lr => lr.LeaveType == leaveType.Value);

            return filteredLeaves.Select(lr => MapToDto(lr, employee));
        }

        public async Task DeleteAsync(Guid id)
        {
            var leaveRequest = await _leaveRepo.GetByIdAsync(id);
            if (leaveRequest == null)
                throw new InvalidOperationException("Leave request not found");

            // Validation: Only pending requests can be deleted
            if (leaveRequest.Status != LeaveRequestStatus.Pending)
                throw new InvalidOperationException("Only pending leave requests can be deleted");

            // Return pending balance
            if (leaveRequest.LeaveType != LeaveType.Unpaid)
            {
                var balance = await _balanceRepo.GetByEmployeeYearAndTypeAsync(
                    leaveRequest.EmployeeId, leaveRequest.StartDate.Year, leaveRequest.LeaveType);

                if (balance != null)
                {
                    balance.Pending -= leaveRequest.TotalDays;
                    await _balanceRepo.UpdateAsync(balance);
                }
            }

            await _leaveRepo.DeleteAsync(id);
        }

        // Helper Methods
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
