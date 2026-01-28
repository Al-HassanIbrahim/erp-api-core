using ERPSystem.Application.DTOs.HR.Employee;
using ERPSystem.Application.DTOs.HR.Payroll;
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
    public class PayrollService:IPayrollService
    {
        private readonly IPayrollRepository _payrollRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IAttendanceRepository _attendanceRepo;
        private readonly ILeaveRequestRepository _leaveRepo;

        public PayrollService(
            IPayrollRepository payrollRepo,
            IEmployeeRepository employeeRepo,
            IAttendanceRepository attendanceRepo,
            ILeaveRequestRepository leaveRepo)
        {
            _payrollRepo = payrollRepo;
            _employeeRepo = employeeRepo;
            _attendanceRepo = attendanceRepo;
            _leaveRepo = leaveRepo;
        }

        public async Task<PayrollBatchDto> GeneratePayrollAsync(
            GeneratePayrollDto dto, string generatedBy)
        {
            // Validate month and year
            if (dto.Month < 1 || dto.Month > 12)
                throw new InvalidOperationException("Invalid month. Must be between 1 and 12");

            if (dto.Year < 2000 || dto.Year > DateTime.Now.Year + 1)
                throw new InvalidOperationException("Invalid year");

            var startDate = new DateOnly(dto.Year, dto.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Get employees to process
            IEnumerable<Employee> employees;

            if (dto.EmployeeIds != null && dto.EmployeeIds.Any())
            {
                // Specific employees
                var empList = new List<Employee>();
                foreach (var id in dto.EmployeeIds)
                {
                    var emp = await _employeeRepo.GetByIdAsync(id);
                    if (emp != null)
                        empList.Add(emp);
                }
                employees = empList;
            }
            else if (dto.DepartmentIds != null && dto.DepartmentIds.Any())
            {
                // By departments
                var empList = new List<Employee>();
                foreach (var deptId in dto.DepartmentIds)
                {
                    var deptEmps = await _employeeRepo.GetByDepartmentIdAsync(deptId);
                    empList.AddRange(deptEmps);
                }
                employees = empList;
            }
            else
            {
                // All employees
                employees = await _employeeRepo.GetAllAsync();
            }

            // Filter by status
            if (!dto.IncludeInactive)
            {
                employees = employees.Where(e => e.Status == EmployeeStatus.Active);
            }

            var result = new PayrollBatchDto
            {
                Month = dto.Month,
                Year = dto.Year,
                TotalEmployees = employees.Count()
            };

            foreach (var employee in employees)
            {
                try
                {
                    // Check if payroll already exists
                    if (await _payrollRepo.ExistsForEmployeeAndPeriodAsync(
                        employee.Id, dto.Month, dto.Year))
                    {
                        result.Errors.Add($"{employee.EmployeeCode}: Payroll already exists for this period");
                        result.FailedCount++;
                        continue;
                    }

                    var payroll = await GenerateSinglePayrollAsync(
                        employee, dto.Month, dto.Year, startDate, endDate, generatedBy);

                    await _payrollRepo.AddAsync(payroll);

                    result.GeneratedCount++;
                    result.TotalGrossPay += payroll.BasicSalary + payroll.TotalAllowances;
                    result.TotalDeductions += payroll.TotalDeductions;
                    result.TotalNetPay += payroll.NetSalary;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"{employee.EmployeeCode}: {ex.Message}");
                    result.FailedCount++;
                }
            }

            return result;
        }

        public async Task<PayrollDetailDto> GenerateEmployeePayrollAsync(
            Guid employeeId, int month, int year, string generatedBy)
        {
            var employee = await _employeeRepo.GetByIdAsync(employeeId);
            if (employee == null)
                throw new InvalidOperationException("Employee not found");

            // Check if already exists
            if (await _payrollRepo.ExistsForEmployeeAndPeriodAsync(employeeId, month, year))
                throw new InvalidOperationException("Payroll already exists for this period");

            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var payroll = await GenerateSinglePayrollAsync(
                employee, month, year, startDate, endDate, generatedBy);

            await _payrollRepo.AddAsync(payroll);

            return MapToDetailDto(payroll);
        }

        private async Task<Payroll> GenerateSinglePayrollAsync(
            Employee employee, int month, int year,
            DateOnly startDate, DateOnly endDate, string generatedBy)
        {
            // Get attendance records
            var attendances = await _attendanceRepo.GetByEmployeeAndPeriodAsync(
                employee.Id, startDate, endDate);

            // Validation: Must have at least one attendance record
            if (!attendances.Any())
                throw new InvalidOperationException("No attendance records found for this period");

            // Calculate working days (excluding weekends)
            var workingDays = CalculateWorkingDays(startDate, endDate);

            // Calculate present days
            var presentDays = attendances.Count(a =>
                a.Status == AttendanceStatus.Present ||
                a.Status == AttendanceStatus.Late);

            // Calculate absent days
            var absentDays = attendances.Count(a => a.Status == AttendanceStatus.Absent);

            // Get approved leaves for this period
            var approvedLeaves = await _leaveRepo.GetApprovedByEmployeeAndPeriodAsync(
                employee.Id, startDate, endDate);

            // Calculate unpaid leave days
            var unpaidLeaveDays = approvedLeaves
                .Where(lr => lr.LeaveType == LeaveType.Unpaid)
                .Sum(lr => CalculateWorkingDaysInRange(lr.StartDate, lr.EndDate));

            // Calculate leave days
            var leaveDays = attendances.Count(a => a.Status == AttendanceStatus.OnLeave);

            // Calculate overtime hours
            var overtimeHours = attendances.Sum(a => a.OvertimeHours);

            // Calculate salary components
            var dailyRate = employee.Salary / workingDays;

            // Base salary = worked days × daily rate
            var baseSalary = presentDays * dailyRate;

            // Create payroll
            var payroll = new Payroll
            {
                Id = Guid.NewGuid(),
                EmployeeId = employee.Id,
                Month = month,
                Year = year,
                PayPeriodStart = startDate,
                PayPeriodEnd = endDate,
                BasicSalary = baseSalary,
                WorkingDays = workingDays,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                UnpaidLeaveDays = unpaidLeaveDays,
                OvertimeHours = overtimeHours,
                Status = PayrollStatus.Draft,
                GeneratedBy = generatedBy,
                BankAccountNumber = employee.BankAccountNumber,
                CreatedAt = DateTime.UtcNow
            };

            // Add allowances
            AddAllowances(payroll, employee);

            // Add overtime pay
            if (overtimeHours > 0)
            {
                var hourlyRate = employee.Salary / (workingDays * 8); // 8 hours per day
                var overtimePay = overtimeHours * hourlyRate * 1.5m; // 1.5x for overtime

                payroll.LineItems.Add(new PayrollLineItem
                {
                    Id = Guid.NewGuid(),
                    Description = $"Overtime Pay ({overtimeHours:F2} hours × {hourlyRate:F2} × 1.5)",
                    Amount = overtimePay,
                    Type = PayrollLineItemType.Allowance
                });
            }

            // Calculate total allowances
            payroll.TotalAllowances = payroll.LineItems
                .Where(i => i.Type == PayrollLineItemType.Allowance)
                .Sum(i => i.Amount);

            // Add deductions
            AddDeductions(payroll, employee, dailyRate, absentDays, unpaidLeaveDays);

            // Calculate total deductions
            payroll.TotalDeductions = payroll.LineItems
                .Where(i => i.Type == PayrollLineItemType.Deduction)
                .Sum(i => i.Amount);

            // Calculate net salary
            payroll.NetSalary = baseSalary + payroll.TotalAllowances - payroll.TotalDeductions;

            // Ensure net salary is not negative
            if (payroll.NetSalary < 0)
                payroll.NetSalary = 0;

            return payroll;
        }

        private void AddAllowances(Payroll payroll, Employee employee)
        {
            // Transport allowance (example: 10% of basic salary)
            var transportAllowance = employee.Salary * 0.10m;
            payroll.LineItems.Add(new PayrollLineItem
            {
                Id = Guid.NewGuid(),
                Description = "Transport Allowance",
                Amount = transportAllowance,
                Type = PayrollLineItemType.Allowance
            });

            // Housing allowance (example: 20% of basic salary)
            var housingAllowance = employee.Salary * 0.20m;
            payroll.LineItems.Add(new PayrollLineItem
            {
                Id = Guid.NewGuid(),
                Description = "Housing Allowance",
                Amount = housingAllowance,
                Type = PayrollLineItemType.Allowance
            });

            // Add more allowances based on position, level, etc.
        }

        private void AddDeductions(Payroll payroll, Employee employee,
            decimal dailyRate, int absentDays, int unpaidLeaveDays)
        {
            // Absence deduction
            if (absentDays > 0)
            {
                var absenceDeduction = absentDays * dailyRate;
                payroll.LineItems.Add(new PayrollLineItem
                {
                    Id = Guid.NewGuid(),
                    Description = $"Absence Deduction ({absentDays} days × {dailyRate:F2})",
                    Amount = absenceDeduction,
                    Type = PayrollLineItemType.Deduction
                });
            }

            // Unpaid leave deduction
            if (unpaidLeaveDays > 0)
            {
                var unpaidLeaveDeduction = unpaidLeaveDays * dailyRate;
                payroll.LineItems.Add(new PayrollLineItem
                {
                    Id = Guid.NewGuid(),
                    Description = $"Unpaid Leave ({unpaidLeaveDays} days × {dailyRate:F2})",
                    Amount = unpaidLeaveDeduction,
                    Type = PayrollLineItemType.Deduction
                });
            }

            // Social security (example: 11% of basic salary)
            var socialSecurity = employee.Salary * 0.11m;
            payroll.LineItems.Add(new PayrollLineItem
            {
                Id = Guid.NewGuid(),
                Description = "Social Security (11%)",
                Amount = socialSecurity,
                Type = PayrollLineItemType.Deduction
            });

            // Income tax (progressive tax example)
            var grossSalary = payroll.BasicSalary + payroll.LineItems
                .Where(i => i.Type == PayrollLineItemType.Allowance)
                .Sum(i => i.Amount);

            var taxDeduction = CalculateTax(grossSalary);
            payroll.LineItems.Add(new PayrollLineItem
            {
                Id = Guid.NewGuid(),
                Description = $"Income Tax",
                Amount = taxDeduction,
                Type = PayrollLineItemType.Deduction
            });
        }

        private decimal CalculateTax(decimal grossSalary)
        {
            // Progressive tax example (Egypt-like system)
            // 0-15000: 0%
            // 15001-30000: 10%
            // 30001-45000: 15%
            // 45001+: 20%

            decimal tax = 0;

            if (grossSalary <= 15000)
            {
                tax = 0;
            }
            else if (grossSalary <= 30000)
            {
                tax = (grossSalary - 15000) * 0.10m;
            }
            else if (grossSalary <= 45000)
            {
                tax = (15000 * 0.10m) + ((grossSalary - 30000) * 0.15m);
            }
            else
            {
                tax = (15000 * 0.10m) + (15000 * 0.15m) + ((grossSalary - 45000) * 0.20m);
            }

            return tax;
        }

        public async Task<PayrollDetailDto?> GetByIdAsync(Guid id)
        {
            var payroll = await _payrollRepo.GetByIdWithDetailsAsync(id);
            return payroll != null ? MapToDetailDto(payroll) : null;
        }

        public async Task<IEnumerable<PayrollDto>> GetByEmployeeIdAsync(Guid employeeId)
        {
            var payrolls = await _payrollRepo.GetByEmployeeIdAsync(employeeId);
            return payrolls.Select(MapToDto);
        }

        public async Task<IEnumerable<PayrollDto>> GetByPeriodAsync(int month, int year)
        {
            var payrolls = await _payrollRepo.GetByMonthAndYearAsync(month, year);
            return payrolls.Select(MapToDto);
        }

        public async Task<PayrollDetailDto> UpdateAsync(
            Guid id, UpdatePayrollDto dto, string modifiedBy)
        {
            var payroll = await _payrollRepo.GetByIdWithDetailsAsync(id);
            if (payroll == null)
                throw new InvalidOperationException("Payroll not found");

            // Validation: Only draft payroll can be updated
            if (payroll.Status != PayrollStatus.Draft)
                throw new InvalidOperationException("Only draft payroll can be updated");

            // Update allowances
            if (dto.Allowances != null)
            {
                // Remove existing allowances
                var existingAllowances = payroll.LineItems
                    .Where(i => i.Type == PayrollLineItemType.Allowance)
                    .ToList();

                foreach (var item in existingAllowances)
                {
                    payroll.LineItems.Remove(item);
                }

                // Add new allowances
                foreach (var allowance in dto.Allowances)
                {
                    payroll.LineItems.Add(new PayrollLineItem
                    {
                        Id = Guid.NewGuid(),
                        Description = allowance.Description,
                        Amount = allowance.Amount,
                        Type = PayrollLineItemType.Allowance
                    });
                }
            }

            // Update deductions
            if (dto.Deductions != null)
            {
                // Remove existing deductions
                var existingDeductions = payroll.LineItems
                    .Where(i => i.Type == PayrollLineItemType.Deduction)
                    .ToList();

                foreach (var item in existingDeductions)
                {
                    payroll.LineItems.Remove(item);
                }

                // Add new deductions
                foreach (var deduction in dto.Deductions)
                {
                    payroll.LineItems.Add(new PayrollLineItem
                    {
                        Id = Guid.NewGuid(),
                        Description = deduction.Description,
                        Amount = deduction.Amount,
                        Type = PayrollLineItemType.Deduction
                    });
                }
            }

            

            // Recalculate totals
            payroll.TotalAllowances = payroll.LineItems
                .Where(i => i.Type == PayrollLineItemType.Allowance)
                .Sum(i => i.Amount);

            payroll.TotalDeductions = payroll.LineItems
                .Where(i => i.Type == PayrollLineItemType.Deduction)
                .Sum(i => i.Amount);

            payroll.NetSalary = payroll.BasicSalary + payroll.TotalAllowances - payroll.TotalDeductions;

            if (payroll.NetSalary < 0)
                payroll.NetSalary = 0;

            await _payrollRepo.UpdateAsync(payroll);

            return MapToDetailDto(payroll);
        }

        public async Task ProcessPayrollAsync(Guid id, string processedBy)
        {
            var payroll = await _payrollRepo.GetByIdAsync(id);
            if (payroll == null)
                throw new InvalidOperationException("Payroll not found");

            // Validation: Can only process draft payroll
            if (payroll.Status != PayrollStatus.Draft)
                throw new InvalidOperationException("Can only process draft payroll");

            // Validation: Net salary must be greater than 0
            if (payroll.NetSalary <= 0)
                throw new InvalidOperationException("Cannot process payroll with zero or negative net salary");

            payroll.Status = PayrollStatus.Processed;
            payroll.ProcessedDate = DateTime.UtcNow;
            payroll.ProcessedBy = processedBy;

            await _payrollRepo.UpdateAsync(payroll);
        }

        public async Task MarkAsPaidAsync(Guid id, MarkPaidDto dto, string paidBy)
        {
            var payroll = await _payrollRepo.GetByIdAsync(id);
            if (payroll == null)
                throw new InvalidOperationException("Payroll not found");

            // Validation: Can only mark processed payroll as paid
            if (payroll.Status != PayrollStatus.Processed)
                throw new InvalidOperationException("Can only mark processed payroll as paid");

            payroll.Status = PayrollStatus.Paid;
            payroll.PaidDate = dto.PaidDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today;
            payroll.PaymentMethod = dto.PaymentMethod;
            payroll.TransactionReference = dto.TransactionReference;
            payroll.PaidBy = paidBy;

            

            await _payrollRepo.UpdateAsync(payroll);
        }

        public async Task RevertToDraftAsync(Guid id, string modifiedBy)
        {
            var payroll = await _payrollRepo.GetByIdAsync(id);
            if (payroll == null)
                throw new InvalidOperationException("Payroll not found");

            // Validation: Can only revert processed payroll
            if (payroll.Status != PayrollStatus.Processed)
                throw new InvalidOperationException("Can only revert processed payroll to draft");

            payroll.Status = PayrollStatus.Draft;
            payroll.ProcessedDate = null;
            payroll.ProcessedBy = null;

            await _payrollRepo.UpdateAsync(payroll);
        }

        public async Task DeleteAsync(Guid id)
        {
            var payroll = await _payrollRepo.GetByIdAsync(id);
            if (payroll == null)
                throw new InvalidOperationException("Payroll not found");

            // Validation: Only draft payroll can be deleted
            if (payroll.Status != PayrollStatus.Draft)
                throw new InvalidOperationException("Only draft payroll can be deleted");

            await _payrollRepo.DeleteAsync(id);
        }

        public async Task<PayrollSummaryDto> GetPeriodSummaryAsync(int month, int year)
        {
            var payrolls = await _payrollRepo.GetByMonthAndYearAsync(month, year);

            var summary = new PayrollSummaryDto
            {
                Month = month,
                Year = year,
                TotalEmployees = payrolls.Count(),
                TotalGrossPay = payrolls.Sum(p => p.BasicSalary + p.TotalAllowances),
                TotalAllowances = payrolls.Sum(p => p.TotalAllowances),
                TotalDeductions = payrolls.Sum(p => p.TotalDeductions),
                TotalNetPay = payrolls.Sum(p => p.NetSalary),
                DraftCount = payrolls.Count(p => p.Status == PayrollStatus.Draft),
                ProcessedCount = payrolls.Count(p => p.Status == PayrollStatus.Processed),
                PaidCount = payrolls.Count(p => p.Status == PayrollStatus.Paid)
            };

            return summary;
        }

        public async Task<PayrollDetailDto> RecalculateAsync(Guid id, string modifiedBy)
        {
            var payroll = await _payrollRepo.GetByIdWithDetailsAsync(id);
            if (payroll == null)
                throw new InvalidOperationException("Payroll not found");

            // Validation: Can only recalculate draft payroll
            if (payroll.Status != PayrollStatus.Draft)
                throw new InvalidOperationException("Can only recalculate draft payroll");

            var employee = await _employeeRepo.GetByIdAsync(payroll.EmployeeId);
            if (employee == null)
                throw new InvalidOperationException("Employee not found");

            // Delete old payroll
            await _payrollRepo.DeleteAsync(id);

            // Generate new one
            var newPayroll = await GenerateSinglePayrollAsync(
                employee,
                payroll.Month,
                payroll.Year,
                payroll.PayPeriodStart,
                payroll.PayPeriodEnd,
                modifiedBy);

            await _payrollRepo.AddAsync(newPayroll);

            return MapToDetailDto(newPayroll);
        }

        // Helper Methods
        private int CalculateWorkingDays(DateOnly startDate, DateOnly endDate)
        {
            int workingDays = 0;
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dayOfWeek = date.DayOfWeek;
                // Exclude Friday and Saturday (weekend in Egypt/Middle East)
                if (dayOfWeek != DayOfWeek.Friday && dayOfWeek != DayOfWeek.Saturday)
                    workingDays++;
            }
            return workingDays;
        }

        private int CalculateWorkingDaysInRange(DateOnly startDate, DateOnly endDate)
        {
            return CalculateWorkingDays(startDate, endDate);
        }

        private PayrollDto MapToDto(Payroll p)
        {
            return new PayrollDto
            {
                Id = p.Id,
                EmployeeId = p.EmployeeId,
                EmployeeCode = p.Employee?.EmployeeCode ?? "Unknown",
                EmployeeName = p.Employee?.FullName ?? "Unknown",
                Month = p.Month,
                Year = p.Year,
                PayPeriodStart = p.PayPeriodStart,
                PayPeriodEnd = p.PayPeriodEnd,
                BasicSalary = p.BasicSalary,
                TotalAllowances = p.TotalAllowances,
                TotalDeductions = p.TotalDeductions,
                NetSalary = p.NetSalary,
                Status = p.Status.ToString(),
                ProcessedDate = p.ProcessedDate,
                PaidDate = p.PaidDate
            };
        }

        private PayrollDetailDto MapToDetailDto(Payroll p)
        {
            return new PayrollDetailDto
            {
                Id = p.Id,
                EmployeeId = p.EmployeeId,
                EmployeeCode = p.Employee?.EmployeeCode ?? "Unknown",
                EmployeeName = p.Employee?.FullName ?? "Unknown",
                Month = p.Month,
                Year = p.Year,
                PayPeriodStart = p.PayPeriodStart,
                PayPeriodEnd = p.PayPeriodEnd,
                BasicSalary = p.BasicSalary,
                TotalAllowances = p.TotalAllowances,
                TotalDeductions = p.TotalDeductions,
                NetSalary = p.NetSalary,
                Status = p.Status.ToString(),
                ProcessedDate = p.ProcessedDate,
                PaidDate = p.PaidDate,
                Employee = p.Employee != null ? new EmployeeListDto
                {
                    Id = p.Employee.Id,
                    EmployeeCode = p.Employee.EmployeeCode,
                    FullName = p.Employee.FullName,
                    Email = p.Employee.Email,
                    PhoneNumber = p.Employee.PhoneNumber,
                    DepartmentName = p.Employee.Department?.Name,
                    PositionTitle = p.Employee.Position?.Title,
                    Status = p.Employee.Status.ToString(),
                    HireDate = p.Employee.HireDate
                } : null,
                WorkingDays = p.WorkingDays,
                PresentDays = p.PresentDays,
                AbsentDays = p.AbsentDays,
                UnpaidLeaveDays = p.UnpaidLeaveDays,
                OvertimeHours = p.OvertimeHours,
                Allowances = p.LineItems
                    .Where(i => i.Type == PayrollLineItemType.Allowance)
                    .Select(i => new PayrollItemDto
                    {
                        Description = i.Description,
                        Amount = i.Amount,
                        Type = i.Type.ToString()
                    }).ToList(),
                Deductions = p.LineItems
                    .Where(i => i.Type == PayrollLineItemType.Deduction)
                    .Select(i => new PayrollItemDto
                    {
                        Description = i.Description,
                        Amount = i.Amount,
                        Type = i.Type.ToString()
                    }).ToList(),
                PaymentMethod = p.PaymentMethod?.ToString(),
                BankAccountNumber = p.BankAccountNumber,
                TransactionReference = p.TransactionReference,
                GeneratedBy = p.GeneratedBy,
                ProcessedBy = p.ProcessedBy,
                PaidBy = p.PaidBy
            };
        }
    }


}
