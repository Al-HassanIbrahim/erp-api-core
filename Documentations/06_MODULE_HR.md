# 06_MODULE_HR

> **Load with:** `00_PROJECT_OVERVIEW.md` for full context.
> **Purpose:** AI-consumption context file — contracts, structure, and business rules only. No implementation code.

---

## 1. Module Overview

The HR module manages the full employee lifecycle within a company: onboarding, organizational structure (departments and positions), daily attendance tracking with check-in/check-out, leave request management with balance tracking, and payroll generation with salary breakdown and tax calculation. All data is strictly **company-scoped** — every repository enforces `CompanyId` isolation, and cross-company access throws `UnauthorizedAccessException`. The module integrates with `IModuleAccessService` to verify that the requesting company has the HR module enabled before any operation proceeds.

---

## 2. Domain Entities

> All entities implement `ICompanyEntity` (carries `int CompanyId`). All `Id` fields are `Guid`.

### 2.1 Employee

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `Guid` | ✅ | PK |
| `CompanyId` | `int` | ✅ | FK → Company (tenant isolation) |
| `EmployeeCode` | `string (50)` | ✅ | Unique per company |
| `FirstName` | `string (50)` | ✅ | — |
| `LastName` | `string (50)` | ✅ | — |
| `FullName` | `string` | computed | `[NotMapped]` — `FirstName + LastName` |
| `Email` | `string (100)` | ✅ | Unique per company; validated as email |
| `PhoneNumber` | `string? (20)` | ❌ | — |
| `DateOfBirth` | `DateTime` | ✅ | Must be ≥ 18 years old at hire date |
| `Gender` | `Gender` (enum) | ✅ | — |
| `Nationality` | `string (100)` | ✅ | — |
| `NationalId` | `string (50)` | ✅ | Unique per company |
| `MaritalStatus` | `MaritalStatus` (enum) | ❌ | — |
| `HireDate` | `DateTime` | ✅ | Cannot be in the future |
| `ProbationEndDate` | `DateTime?` | ❌ | Auto-set: `HireDate + ProbationPeriodMonths` |
| `TerminationDate` | `DateTime?` | ❌ | Set on termination; must be ≥ HireDate |
| `Status` | `EmployeeStatus` (enum) | ✅ | Default: `Active` |
| `DepartmentId` | `Guid` | ✅ | FK → Department (must be active) |
| `Department` | `Department` | nav | — |
| `PositionId` | `Guid` | ✅ | FK → JobPosition (must be active) |
| `Position` | `JobPosition` | nav | — |
| `ReportsToId` | `Guid?` | ❌ | FK → Employee (self-ref; circular check enforced) |
| `Manager` | `Employee?` | nav | — |
| `DirectReports` | `ICollection<Employee>` | nav | — |
| `CurrentAddressLine` | `string? (200)` | ❌ | — |
| `CurrentCity` | `string? (100)` | ❌ | — |
| `CurrentCountry` | `string? (100)` | ❌ | — |
| `CurrentPostalCode` | `string? (20)` | ❌ | — |
| `Salary` | `decimal (18,2)` | ✅ | Must be within `Position.MinSalary`–`Position.MaxSalary` |
| `Currency` | `string (3)` | ✅ | Default: `"EGP"` |
| `BankAccountNumber` | `string? (50)` | ❌ | — |
| `BankName` | `string? (100)` | ❌ | — |
| `BankBranch` | `string? (100)` | ❌ | — |
| `ProfileImageUrl` | `string? (500)` | ❌ | — |
| `Documents` | `ICollection<EmployeeDocument>` | nav | — |
| `Attendances` | `ICollection<Attendance>` | nav | — |
| `LeaveRequests` | `ICollection<LeaveRequest>` | nav | — |
| `Payrolls` | `ICollection<Payroll>` | nav | — |
| `CreatedAt` | `DateTime` | ✅ | Auto UTC |
| `ModifiedAt` | `DateTime?` | ❌ | — |
| `CreatedBy` | `string? (100)` | ❌ | — |
| `ModifiedBy` | `string? (100)` | ❌ | — |

---

### 2.2 Department

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `Guid` | ✅ | PK |
| `CompanyId` | `int` | ✅ | FK → Company |
| `Code` | `string (20)` | ✅ | Unique per company |
| `Name` | `string (100)` | ✅ | Unique per company (case-insensitive) |
| `Description` | `string? (500)` | ❌ | — |
| `ManagerId` | `Guid?` | ❌ | FK → Employee (must be Active) |
| `Manager` | `Employee?` | nav | — |
| `IsActive` | `bool` | ✅ | Default: `true` |
| `Employees` | `ICollection<Employee>` | nav | — |
| `Positions` | `ICollection<JobPosition>` | nav | — |
| `CreatedAt` | `DateTime` | ✅ | Auto UTC |
| `ModifiedAt` | `DateTime?` | ❌ | — |

---

### 2.3 JobPosition

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `Guid` | ✅ | PK |
| `CompanyId` | `int` | ✅ | FK → Company |
| `Code` | `string` | ✅ | Unique per company |
| `Title` | `string` | ✅ | Position title (e.g., "Senior Developer") |
| `Description` | `string?` | ❌ | — |
| `Level` | `PositionLevel` (enum) | ✅ | — |
| `MinSalary` | `decimal (18,2)` | ✅ | Salary range lower bound |
| `MaxSalary` | `decimal (18,2)` | ✅ | Salary range upper bound; must be > MinSalary |
| `IsActive` | `bool` | ✅ | Default: `true` |
| `DepartmentId` | `Guid` | ✅ | FK → Department |
| `Department` | `Department` | nav | — |
| `Employees` | `ICollection<Employee>` | nav | — |
| `CreatedAt` | `DateTime` | ✅ | Auto UTC |
| `ModifiedAt` | `DateTime?` | ❌ | — |

---

### 2.4 Attendance

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `Guid` | ✅ | PK |
| `CompanyId` | `int` | ✅ | FK → Company |
| `EmployeeId` | `Guid` | ✅ | FK → Employee |
| `Employee` | `Employee` | nav | — |
| `Date` | `DateOnly` | ✅ | Cannot be a future date |
| `CheckInTime` | `TimeOnly?` | ❌ | — |
| `CheckOutTime` | `TimeOnly?` | ❌ | Must be after CheckInTime |
| `Status` | `AttendanceStatus` (enum) | ✅ | `Present`, `Absent`, `Late`, `OnLeave` |
| `WorkedHours` | `decimal (5,2)` | ❌ | Auto-calculated: `(CheckOut - CheckIn - 30min) / 60` |
| `OvertimeHours` | `decimal (5,2)` | ❌ | `Max(0, WorkedHours - 8)` |
| `Notes` | `string? (500)` | ❌ | — |
| `IsManualEntry` | `bool` | ✅ | Default: `false` |
| `ManualEntryReason` | `string? (500)` | ❌ | Required for manual entries |
| `CreatedAt` | `DateTime` | ✅ | Auto UTC |
| `ModifiedAt` | `DateTime?` | ❌ | — |
| `CreatedBy` | `string? (100)` | ❌ | — |
| `ModifiedBy` | `string? (100)` | ❌ | — |

---

### 2.5 LeaveRequest

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `Guid` | ✅ | PK |
| `CompanyId` | `int` | ✅ | FK → Company |
| `EmployeeId` | `Guid` | ✅ | FK → Employee (must be Active) |
| `Employee` | `Employee` | nav | — |
| `LeaveType` | `LeaveType` (enum) | ✅ | `Annual`, `Sick`, `Emergency`, `Maternity`, `Paternity`, `Study`, `Unpaid` |
| `StartDate` | `DateOnly` | ✅ | Cannot be in the past |
| `EndDate` | `DateOnly` | ✅ | Must be ≥ StartDate |
| `TotalDays` | `int` | ✅ | `(EndDate - StartDate) + 1` |
| `Status` | `LeaveRequestStatus` (enum) | ✅ | Default: `Pending` |
| `RequestDate` | `DateTime` | ✅ | Auto UTC |
| `Reason` | `string (500)` | ✅ | — |
| `ApprovedBy` | `string? (100)` | ❌ | Set on approval |
| `ApprovedDate` | `DateTime?` | ❌ | — |
| `RejectedBy` | `string? (100)` | ❌ | Set on rejection |
| `RejectedDate` | `DateTime?` | ❌ | — |
| `CancelledBy` | `string? (100)` | ❌ | Set on cancellation |
| `CancelledDate` | `DateTime?` | ❌ | — |
| `CurrentBalance` | `decimal (5,2)` | ❌ | Balance at time of request |
| `BalanceAfter` | `decimal (5,2)` | ❌ | `CurrentBalance - TotalDays` |
| `Attachments` | `ICollection<LeaveAttachment>` | nav | — |

---

### 2.6 LeaveBalance

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `Guid` | ✅ | PK |
| `CompanyId` | `int` | ✅ | FK → Company |
| `EmployeeId` | `Guid` | ✅ | FK → Employee |
| `Employee` | `Employee` | nav | — |
| `Year` | `int` | ✅ | Calendar year |
| `LeaveType` | `LeaveType` (enum) | ✅ | — |
| `TotalEntitlement` | `decimal (5,2)` | ✅ | Total days allocated |
| `Used` | `decimal (5,2)` | ❌ | Days consumed (approved) |
| `Pending` | `decimal (5,2)` | ❌ | Days in pending requests |
| `Available` | `decimal` | computed | `[NotMapped]` — `TotalEntitlement - Used - Pending` |

---

### 2.7 Payroll

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `Guid` | ✅ | PK |
| `CompanyId` | `int` | ✅ | FK → Company |
| `EmployeeId` | `Guid` | ✅ | FK → Employee |
| `Employee` | `Employee` | nav | — |
| `Month` | `int` | ✅ | 1–12 |
| `Year` | `int` | ✅ | 2000 to current year + 1 |
| `PayPeriodStart` | `DateOnly` | ✅ | First day of month |
| `PayPeriodEnd` | `DateOnly` | ✅ | Last day of month |
| `BasicSalary` | `decimal (18,2)` | ✅ | `PresentDays × DailyRate` |
| `TotalAllowances` | `decimal (18,2)` | ❌ | Sum of Allowance line items |
| `TotalDeductions` | `decimal (18,2)` | ❌ | Sum of Deduction line items |
| `NetSalary` | `decimal (18,2)` | ❌ | `BasicSalary + Allowances - Deductions` |
| `WorkingDays` | `int` | ❌ | Total working days in period (excl. Fri/Sat) |
| `PresentDays` | `int` | ❌ | Present + Late attendance days |
| `AbsentDays` | `int` | ❌ | Absent attendance days |
| `UnpaidLeaveDays` | `int` | ❌ | Days from approved Unpaid leaves |
| `OvertimeHours` | `decimal (5,2)` | ❌ | Sum from attendance records |
| `Status` | `PayrollStatus` (enum) | ✅ | Default: `Draft` |
| `ProcessedDate` | `DateTime?` | ❌ | Set when `Draft → Processed` |
| `PaidDate` | `DateTime?` | ❌ | Set when `Processed → Paid` |
| `PaymentMethod` | `PaymentMethod?` (enum) | ❌ | — |
| `BankAccountNumber` | `string? (100)` | ❌ | Copied from Employee |
| `TransactionReference` | `string? (100)` | ❌ | Set on MarkPaid |
| `LineItems` | `ICollection<PayrollLineItem>` | nav | — |
| `GeneratedBy` | `string? (100)` | ❌ | — |
| `ProcessedBy` | `string? (100)` | ❌ | — |
| `PaidBy` | `string? (100)` | ❌ | — |
| `CreatedAt` | `DateTime` | ✅ | Auto UTC |

---

### 2.8 PayrollLineItem

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `Guid` | ✅ | PK |
| `CompanyId` | `int` | ✅ | FK → Company |
| `PayrollId` | `Guid` | ✅ | FK → Payroll |
| `Payroll` | `Payroll` | nav | — |
| `Description` | `string (200)` | ✅ | e.g., "Transport Allowance", "Social Security (11%)" |
| `Amount` | `decimal (18,2)` | ✅ | — |
| `Type` | `PayrollLineItemType` (enum) | ✅ | `Allowance` or `Deduction` |

---

### 2.9 EmployeeDocument

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `Guid` | ✅ | PK |
| `CompanyId` | `int` | ✅ | FK → Company |
| `EmployeeId` | `Guid` | ✅ | FK → Employee |
| `Employee` | `Employee` | nav | — |
| `DocumentName` | `string (200)` | ✅ | — |
| `DocumentType` | `string (50)` | ✅ | — |
| `FilePath` | `string (500)` | ✅ | — |
| `FileSizeBytes` | `long` | ✅ | — |
| `FileExtension` | `string (50)` | ✅ | — |
| `UploadedAt` | `DateTime` | ✅ | Auto UTC |
| `UploadedBy` | `string? (100)` | ❌ | — |

---

### 2.10 LeaveAttachment

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `Guid` | ✅ | PK |
| `CompanyId` | `int` | ✅ | FK → Company |
| `LeaveRequestId` | `Guid` | ✅ | FK → LeaveRequest |
| `LeaveRequest` | `LeaveRequest` | nav | — |
| `FileName` | `string (200)` | ✅ | — |
| `FilePath` | `string (500)` | ✅ | — |
| `UploadedAt` | `DateTime` | ✅ | Auto UTC |

---

## 3. Repository Interfaces

> All repositories extend `BaseRepository<T>` which enforces `CompanyId` scoping. `EnsureCompany()` guard is called on every method.

### 3.1 IAttendanceRepository

| Method | Parameters | Return Type |
|---|---|---|
| `GetByIdAsync` | `Guid id, int companyId, CancellationToken ct` | `Task<Attendance?>` |
| `GetByEmployeeAndDateAsync` | `Guid employeeId, DateOnly date, int companyId, CancellationToken ct` | `Task<Attendance?>` |
| `GetByEmployeeAndPeriodAsync` | `Guid employeeId, DateOnly start, DateOnly end, int companyId, CancellationToken ct` | `Task<IReadOnlyList<Attendance>>` |
| `HasCheckedInTodayAsync` | `Guid employeeId, DateOnly date, int companyId, CancellationToken ct` | `Task<bool>` |
| `IsPayrollProcessedForPeriodAsync` | `Guid employeeId, DateOnly date, int companyId, CancellationToken ct` | `Task<bool>` |
| `AddAsync` | `Attendance attendance` | `Task` |
| `UpdateAsync` | `Attendance attendance` | `Task` |
| `DeleteAsync` | `Guid id, int companyId, CancellationToken ct` | `Task` |

---

### 3.2 IDepartmentRepository

| Method | Parameters | Return Type |
|---|---|---|
| `GetByIdAsync` | `Guid id, int companyId, CancellationToken ct` | `Task<Department?>` |
| `GetByIdWithDetailsAsync` | `Guid id, int companyId, CancellationToken ct` | `Task<Department?>` (includes Manager, Employees, Positions) |
| `GetAllAsync` | `int companyId, CancellationToken ct` | `Task<IEnumerable<Department>>` (includes Manager, Employees) |
| `ExistsByCodeAsync` | `string code, int companyId, CancellationToken ct` | `Task<bool>` |
| `ExistsByNameAsync` | `string name, int companyId, CancellationToken ct` | `Task<bool>` (case-insensitive) |
| `GetEmployeeCountAsync` | `Guid departmentId, int companyId, CancellationToken ct` | `Task<int>` |
| `AddAsync` | `Department department, CancellationToken ct` | `Task` |
| `UpdateAsync` | `Department department, CancellationToken ct` | `Task` |
| `DeleteAsync` | `Guid id, int companyId, CancellationToken ct` | `Task` (cascades to Positions; blocks if Employees exist) |

---

### 3.3 IEmployeeRepository

| Method | Parameters | Return Type |
|---|---|---|
| `GetByIdAsync` | `Guid id, int companyId, CancellationToken ct` | `Task<Employee?>` |
| `GetByIdWithDetailsAsync` | `Guid id, int companyId, CancellationToken ct` | `Task<Employee?>` (includes Dept, Position, Manager, DirectReports, Documents) |
| `GetAllAsync` | `int companyId, CancellationToken ct` | `Task<IEnumerable<Employee>>` (includes Dept, Position) |
| `ExistsByEmailAsync` | `string email, int companyId, CancellationToken ct` | `Task<bool>` (case-insensitive) |
| `ExistsByEmployeeCodeAsync` | `string code, int companyId, CancellationToken ct` | `Task<bool>` |
| `ExistsByNationalIdAsync` | `string nationalId, int companyId, CancellationToken ct` | `Task<bool>` |
| `GetByDepartmentIdAsync` | `Guid departmentId, int companyId, CancellationToken ct` | `Task<IEnumerable<Employee>>` |
| `GetByStatusAsync` | `EmployeeStatus status, int companyId, CancellationToken ct` | `Task<IEnumerable<Employee>>` |
| `HasCircularReportingAsync` | `Guid employeeId, Guid managerId, int companyId, CancellationToken ct` | `Task<bool>` (traverses manager chain) |
| `AddAsync` | `Employee employee` | `Task` |
| `UpdateAsync` | `Employee employee` | `Task` |
| `DeleteAsync` | `Guid id, int companyId, CancellationToken ct` | `Task` (cascades: Attendances, LeaveRequests, Payrolls, Documents) |

---

### 3.4 ILeaveBalanceRepository

| Method | Parameters | Return Type |
|---|---|---|
| `GetByEmployeeYearAndTypeAsync` | `Guid employeeId, int year, LeaveType type, int companyId, CancellationToken ct` | `Task<LeaveBalance?>` |
| `GetByEmployeeAndYearAsync` | `Guid employeeId, int year, int companyId, CancellationToken ct` | `Task<IEnumerable<LeaveBalance>>` |
| `AddAsync` | `LeaveBalance balance` | `Task` |
| `UpdateAsync` | `LeaveBalance balance` | `Task` |

---

### 3.5 ILeaveRequestRepository

| Method | Parameters | Return Type |
|---|---|---|
| `GetByIdAsync` | `Guid id, int companyId, CancellationToken ct` | `Task<LeaveRequest?>` |
| `GetByIdWithDetailsAsync` | `Guid id, int companyId, CancellationToken ct` | `Task<LeaveRequest?>` (includes Employee, Attachments) |
| `GetByEmployeeIdAsync` | `Guid employeeId, int companyId, CancellationToken ct` | `Task<IEnumerable<LeaveRequest>>` (desc by RequestDate) |
| `GetPendingAsync` | `int companyId, CancellationToken ct` | `Task<IEnumerable<LeaveRequest>>` (includes Employee, ordered by RequestDate asc) |
| `HasOverlappingLeaveAsync` | `Guid employeeId, DateOnly start, DateOnly end, int companyId, Guid? excludeId, CancellationToken ct` | `Task<bool>` |
| `GetApprovedByEmployeeAndPeriodAsync` | `Guid employeeId, DateOnly startDate, DateOnly endDate, int companyId, CancellationToken ct` | `Task<IEnumerable<LeaveRequest>>` |
| `AnyByEmployeeAsync` | `Guid employeeId, int companyId, CancellationToken ct` | `Task<bool>` |
| `AddAsync` | `LeaveRequest leaveRequest` | `Task` |
| `UpdateAsync` | `LeaveRequest leaveRequest` | `Task` |
| `DeleteAsync` | `Guid id, int companyId, CancellationToken ct` | `Task` |

---

### 3.6 IPayrollRepository

| Method | Parameters | Return Type |
|---|---|---|
| `GetByIdAsync` | `Guid id, int companyId, CancellationToken ct` | `Task<Payroll?>` |
| `GetByIdWithDetailsAsync` | `Guid id, int companyId, CancellationToken ct` | `Task<Payroll?>` (includes Employee, LineItems) |
| `GetByEmployeeIdAsync` | `Guid employeeId, int companyId, CancellationToken ct` | `Task<IEnumerable<Payroll>>` (desc by Year, Month) |
| `GetByEmployeeAndPeriodAsync` | `Guid employeeId, int month, int year, int companyId, CancellationToken ct` | `Task<Payroll?>` (includes LineItems) |
| `GetByMonthAndYearAsync` | `int month, int year, int companyId, CancellationToken ct` | `Task<IEnumerable<Payroll>>` (includes Employee) |
| `ExistsForEmployeeAndPeriodAsync` | `Guid employeeId, int month, int year, int companyId, CancellationToken ct` | `Task<bool>` |
| `AddAsync` | `Payroll payroll` | `Task` |
| `UpdateAsync` | `Payroll payroll` | `Task` |
| `DeleteAsync` | `Guid id, int companyId, CancellationToken ct` | `Task` |

---

### 3.7 IPositionRepository

| Method | Parameters | Return Type |
|---|---|---|
| `GetByIdAsync` | `Guid id, int companyId, CancellationToken ct` | `Task<JobPosition?>` (includes Department) |
| `GetAllAsync` | `int companyId, CancellationToken ct` | `Task<IEnumerable<JobPosition>>` (includes Department) |
| `ExistsByCodeAsync` | `string code, int companyId, CancellationToken ct` | `Task<bool>` |
| `AddAsync` | `JobPosition position` | `Task` |
| `UpdateAsync` | `JobPosition position` | `Task` |
| `DeleteAsync` | `Guid id, int companyId, CancellationToken ct` | `Task` |

---

## 4. Service Interfaces

> All services call `_moduleAccess.EnsureHrAccessAsync()` first. All use `_currentUser.CompanyId` for tenant scoping.

### 4.1 IEmployeeService

| Method | Parameters | Return Type | Notes |
|---|---|---|---|
| `CreateAsync` | `CreateEmployeeDto dto, string createdBy, CancellationToken ct` | `Task<EmployeeDetailDto>` | Full uniqueness + age + salary validation |
| `UpdateAsync` | `Guid id, UpdateEmployeeDto dto, string modifiedBy, CancellationToken ct` | `Task<EmployeeDetailDto>` | Partial update; validates changed fields only |
| `UpdateStatusAsync` | `Guid id, UpdateEmployeeDto dto, string modifiedBy, CancellationToken ct` | `Task` | Status transitions with business rule enforcement |
| `GetByIdAsync` | `Guid id, CancellationToken ct` | `Task<EmployeeDetailDto?>` | Returns null if not found |
| `GetAllAsync` | `CancellationToken ct` | `Task<IEnumerable<EmployeeListDto>>` | — |
| `GetByDepartmentAsync` | `Guid departmentId, CancellationToken ct` | `Task<IEnumerable<EmployeeListDto>>` | Validates dept exists in company |
| `GetByStatusAsync` | `EmployeeStatus status, CancellationToken ct` | `Task<IEnumerable<EmployeeListDto>>` | — |
| `DeleteAsync` | `Guid id, CancellationToken ct` | `Task` | Blocks if employee has direct reports |

---

### 4.2 IAttendanceService

| Method | Parameters | Return Type | Notes |
|---|---|---|---|
| `CheckInAsync` | `CheckInDto dto, string createdBy, CancellationToken ct` | `Task<AttendanceDto>` | Validates not future, no duplicate, employee active |
| `CheckOutAsync` | `CheckOutDto dto, string modifiedBy, CancellationToken ct` | `Task<AttendanceDto>` | Requires existing check-in; calculates hours |
| `CreateManualEntryAsync` | `ManualAttendanceDto dto, string createdBy, CancellationToken ct` | `Task<AttendanceDto>` | Blocks if payroll already processed |
| `UpdateAsync` | `Guid id, UpdateAttendanceDto dto, string modifiedBy, CancellationToken ct` | `Task<AttendanceDto>` | Blocks if payroll already processed |
| `GetSummaryAsync` | `Guid employeeId, int month, int year, CancellationToken ct` | `Task<AttendanceSummaryDto>` | Aggregates attendance stats for the month |

---

### 4.3 ILeaveRequestService

| Method | Parameters | Return Type | Notes |
|---|---|---|---|
| `CreateAsync` | `CreateLeaveRequestDto dto, CancellationToken ct` | `Task<LeaveRequestDto>` | Validates balance, overlap, and weekend-only requests |
| `UpdateAsync` | `Guid id, UpdateLeaveRequestDto dto, CancellationToken ct` | `Task<LeaveRequestDto>` | Only `Pending` requests can be updated |
| `ApproveAsync` | `Guid id, string approvedBy, CancellationToken ct` | `Task<LeaveRequestDto>` | Moves `Pending → Approved`; deducts balance |
| `RejectAsync` | `Guid id, string rejectedBy, string reason, CancellationToken ct` | `Task<LeaveRequestDto>` | Moves `Pending → Rejected`; restores pending balance |
| `CancelAsync` | `Guid id, string cancelledBy, CancellationToken ct` | `Task<LeaveRequestDto>` | Only `Pending` or `Approved` can be cancelled |
| `GetByIdAsync` | `Guid id, CancellationToken ct` | `Task<LeaveRequestDetailDto?>` | Returns detail with attachments |
| `GetPendingAsync` | `CancellationToken ct` | `Task<IEnumerable<LeaveRequestDto>>` | Company-wide pending list |
| `GetBalanceAsync` | `Guid employeeId, int year, CancellationToken ct` | `Task<LeaveBalanceDto>` | Auto-initializes missing balances with defaults |
| `GetHistoryAsync` | `Guid employeeId, DateOnly? start, DateOnly? end, LeaveType? type, CancellationToken ct` | `Task<IEnumerable<LeaveRequestDto>>` | Filterable history |
| `DeleteAsync` | `Guid id, CancellationToken ct` | `Task` | Only `Pending` requests; restores pending balance |

---

### 4.4 IPayrollService

| Method | Parameters | Return Type | Notes |
|---|---|---|---|
| `GeneratePayrollAsync` | `GeneratePayrollDto dto, string generatedBy, CancellationToken ct` | `Task<PayrollBatchDto>` | Batch generation; skips existing; tolerates per-employee failures |
| `GenerateEmployeePayrollAsync` | `Guid employeeId, int month, int year, string generatedBy, CancellationToken ct` | `Task<PayrollDetailDto>` | Single employee; throws if already exists |
| `GetByIdAsync` | `Guid id, CancellationToken ct` | `Task<PayrollDetailDto?>` | — |
| `GetByEmployeeIdAsync` | `Guid employeeId, CancellationToken ct` | `Task<IEnumerable<PayrollDto>>` | — |
| `GetByPeriodAsync` | `int month, int year, CancellationToken ct` | `Task<IEnumerable<PayrollDto>>` | — |
| `UpdateAsync` | `Guid id, UpdatePayrollDto dto, string modifiedBy, CancellationToken ct` | `Task<PayrollDetailDto>` | Only `Draft` payrolls |
| `ProcessPayrollAsync` | `Guid id, string processedBy, CancellationToken ct` | `Task` | `Draft → Processed` |
| `MarkAsPaidAsync` | `Guid id, MarkPaidDto dto, string paidBy, CancellationToken ct` | `Task` | `Processed → Paid` |
| `RevertToDraftAsync` | `Guid id, string modifiedBy, CancellationToken ct` | `Task` | `Processed → Draft` only |
| `DeleteAsync` | `Guid id, CancellationToken ct` | `Task` | Only `Draft` payrolls |
| `GetPeriodSummaryAsync` | `int month, int year, CancellationToken ct` | `Task<PayrollSummaryDto>` | Aggregate totals for the period |
| `RecalculateAsync` | `Guid id, string modifiedBy, CancellationToken ct` | `Task<PayrollDetailDto>` | Deletes & regenerates; only `Draft` payrolls |

---

## 5. DTOs & View Models

### 5.1 Request DTOs

#### Employee

| DTO | Fields |
|---|---|
| `CreateEmployeeDto` | `EmployeeCode(string)`, `FirstName(string)`, `LastName(string)`, `Email(string)`, `PhoneNumber(string?)`, `DateOfBirth(DateTime)`, `Gender(Gender)`, `Nationality(string)`, `NationalId(string)`, `MaritalStatus(MaritalStatus)`, `HireDate(DateTime)`, `ProbationPeriodMonths(int)`, `DepartmentId(Guid)`, `PositionId(Guid)`, `ReportsToId(Guid?)`, `CurrentAddress(AddressDto)`, `BankAccountNumber(string?)`, `BankName(string?)`, `BankBranch(string?)`, `Salary(decimal)`, `Currency(string)` |
| `UpdateEmployeeDto` | `Email(string?)`, `PhoneNumber(string?)`, `DepartmentId(Guid?)`, `PositionId(Guid?)`, `ReportsToId(Guid?)`, `CurrentAddress(AddressDto?)`, `BankAccountNumber(string?)`, `BankName(string?)`, `BankBranch(string?)`, `Salary(decimal?)`, `Currency(string?)`, `Status(EmployeeStatus)`, `EffectiveDate(DateTime?)`, `Reason(string?)` |

#### Department

| DTO | Fields |
|---|---|
| `CreateDepartmentDto` | `Code(string)`, `Name(string)`, `Description(string?)`, `ManagerId(Guid?)`, `IsActive(bool)` |
| `UpdateDepartmentDto` | `Name(string)`, `Description(string?)`, `ManagerId(Guid?)`, `IsActive(bool)` |

#### Position

| DTO | Fields |
|---|---|
| `CreatePositionDto` | `Code(string)`, `Title(string)`, `Description(string?)`, `Level(PositionLevel)`, `MinSalary(decimal)`, `MaxSalary(decimal)`, `DepartmentId(Guid)` |
| `UpdatePositionDto` | `Title(string?)`, `Description(string?)`, `Level(PositionLevel?)`, `MinSalary(decimal?)`, `MaxSalary(decimal?)`, `DepartmentId(Guid?)`, `IsActive(bool?)` |

#### Attendance

| DTO | Fields |
|---|---|
| `CheckInDto` | `EmployeeId(Guid)`, `CheckInTime(DateTime?)`, `Notes(string?)` |
| `CheckOutDto` | `EmployeeId(Guid)`, `CheckOutTime(DateTime?)` |
| `ManualAttendanceDto` | `EmployeeId(Guid)`, `Date(DateOnly)`, `CheckInTime(TimeOnly?)`, `CheckOutTime(TimeOnly?)`, `Status(AttendanceStatus)`, `Notes(string?)` |
| `UpdateAttendanceDto` | `CheckInTime(TimeOnly?)`, `CheckOutTime(TimeOnly?)`, `Status(AttendanceStatus?)`, `Notes(string?)` |

#### Leave

| DTO | Fields |
|---|---|
| `CreateLeaveRequestDto` | `EmployeeId(Guid)`, `LeaveType(LeaveType)`, `StartDate(DateOnly)`, `EndDate(DateOnly)`, `Reason(string)` |
| `UpdateLeaveRequestDto` | `StartDate(DateOnly?)`, `EndDate(DateOnly?)`, `Reason(string?)` |

#### Payroll

| DTO | Fields |
|---|---|
| `GeneratePayrollDto` | `Month(int)`, `Year(int)`, `EmployeeIds(List<Guid>?)`, `DepartmentIds(List<Guid>?)`, `IncludeInactive(bool)` |
| `UpdatePayrollDto` | `PaymentMethod(PaymentMethod?)`, `BankAccountNumber(string?)`, `Notes(string?)` |
| `MarkPaidDto` | `PaymentMethod(PaymentMethod)`, `TransactionReference(string?)`, `PaidDate(DateTime?)` |

---

### 5.2 Response DTOs

#### Employee

| DTO | Fields |
|---|---|
| `EmployeeDetailDto` | `Id`, `EmployeeCode`, `FirstName`, `LastName`, `FullName`, `Email`, `PhoneNumber`, `DateOfBirth`, `Gender(string)`, `Nationality`, `NationalId`, `MaritalStatus(string)`, `HireDate`, `ProbationEndDate`, `TerminationDate`, `Status(string)`, `Department(DepartmentDto?)`, `Position(PositionDto?)`, `ReportsTo(EmployeeListDto?)`, `DirectReports(List<EmployeeListDto>)`, `CurrentAddress(AddressDto)`, `BankAccountNumber`, `BankName`, `BankBranch`, `Salary`, `Currency`, `ProfileImageUrl`, `Documents(List<DocumentDto>)`, `CreatedAt`, `ModifiedAt` |
| `DepartmentDto` | `Id`, `Code`, `Name`, `Description`, `ManagerId`, `ManagerName`, `EmployeeCount`, `IsActive`, `CreatedAt` |
| `DepartmentDetailDto` | Same as `DepartmentDto` + `Manager(EmployeeListDto?)`, `Employees(List<EmployeeListDto>)` |
| `PositionDto` | `Id`, `Code`, `Title`, `Description`, `Level(string)`, `MinSalary`, `MaxSalary`, `IsActive` |
| `AddressDto` | `City(string?)`, `Country(string?)`, `PostalCode(string?)` |
| `DocumentDto` | `Id`, `DocumentName`, `DocumentType`, `FilePath`, `FileSizeBytes`, `FileExtension`, `UploadedAt`, `UploadedBy` |

#### Attendance

| DTO | Fields |
|---|---|
| `AttendanceDto` | `Id`, `EmployeeId`, `EmployeeCode`, `EmployeeName`, `Date`, `CheckInTime`, `CheckOutTime`, `Status(string)`, `WorkedHours`, `OvertimeHours`, `Notes`, `IsManualEntry` |

#### Leave

| DTO | Fields |
|---|---|
| `LeaveRequestDto` | `Id`, `EmployeeId`, `EmployeeCode`, `EmployeeName`, `LeaveType(string)`, `StartDate`, `EndDate`, `TotalDays`, `Status(string)`, `RequestDate`, `Reason` |
| `LeaveRequestDetailDto` | Same as `LeaveRequestDto` + `Employee(EmployeeListDto)`, `ApprovedBy`, `ApprovedDate`, `RejectedBy`, `RejectedDate`, `CancelledBy`, `CancelledDate`, `CurrentBalance`, `BalanceAfter`, `Attachments(List<DocumentDto>)` |

#### Payroll

| DTO | Fields |
|---|---|
| `PayrollDto` | `Id`, `EmployeeId`, `EmployeeCode`, `EmployeeName`, `Month`, `Year`, `PayPeriodStart`, `PayPeriodEnd`, `BasicSalary`, `TotalAllowances`, `TotalDeductions`, `NetSalary`, `Status(string)`, `ProcessedDate`, `PaidDate` |
| `PayrollDetailDto` | Same as `PayrollDto` + `Employee(EmployeeListDto?)`, `WorkingDays`, `PresentDays`, `AbsentDays`, `UnpaidLeaveDays`, `OvertimeHours`, `Allowances(List<PayrollItemDto>)`, `Deductions(List<PayrollItemDto>)`, `PaymentMethod`, `BankAccountNumber`, `TransactionReference`, `GeneratedBy`, `ProcessedBy`, `PaidBy` |
| `PayrollItemDto` | `Description(string)`, `Amount(decimal)`, `Type(string)` |

---

### 5.3 List / Summary DTOs

| DTO | Fields |
|---|---|
| `EmployeeListDto` | `Id`, `EmployeeCode`, `FullName`, `Email`, `PhoneNumber`, `DepartmentName`, `PositionTitle`, `Status(string)`, `HireDate`, `ProfileImageUrl` |
| `AttendanceSummaryDto` | `EmployeeId`, `EmployeeName`, `Month`, `Year`, `TotalWorkingDays`, `PresentDays`, `AbsentDays`, `LateDays`, `LeaveDays`, `TotalWorkedHours`, `TotalOvertimeHours`, `AttendanceRate(decimal)` |
| `LeaveBalanceDto` | `EmployeeId`, `EmployeeName`, `Year`, `Balances(List<LeaveTypeBalance>)` |
| `LeaveTypeBalance` | `LeaveType(string)`, `TotalEntitlement`, `Used`, `Pending`, `Available` |
| `PayrollBatchDto` | `Month`, `Year`, `TotalEmployees`, `GeneratedCount`, `FailedCount`, `TotalGrossPay`, `TotalDeductions`, `TotalNetPay`, `Errors(List<string>)` |
| `PayrollSummaryDto` | Aggregate totals for all payrolls in a given month/year |

---

## 6. API Endpoints

### 6.1 EmployeeController — `api/employee`

| HTTP Verb | Route | Auth | Request | Response | Description |
|---|---|---|---|---|---|
| `GET` | `/api/employee` | ✅ JWT | — | `IEnumerable<EmployeeListDto>` | Get all employees (company-scoped) |
| `GET` | `/api/employee/{id}` | ✅ JWT | path: `Guid id` | `EmployeeDetailDto` / 404 | Get employee with full details |
| `GET` | `/api/employee/department/{departmentId}` | ✅ JWT | path: `Guid departmentId` | `IEnumerable<EmployeeListDto>` | Get employees by department |
| `GET` | `/api/employee/status/{status}` | ✅ JWT | path: `EmployeeStatus status` (1=Active, 2=Inactive, 3=OnLeave, 4=Terminated) | `IEnumerable<EmployeeListDto>` | Get employees by status |
| `POST` | `/api/employee` | ✅ JWT | body: `CreateEmployeeDto` | `EmployeeDetailDto` 201 / 400 | Create new employee |
| `PUT` | `/api/employee/{id}` | ✅ JWT | path: `Guid id`, body: `UpdateEmployeeDto` | `EmployeeDetailDto` / 400 / 404 | Update employee details |
| `PUT` | `/api/employee/{id}/status` | ✅ JWT | path: `Guid id`, body: `UpdateEmployeeDto` | 204 / 400 / 404 | Update employee status |
| `DELETE` | `/api/employee/{id}` | ✅ JWT | path: `Guid id` | 204 / 400 / 404 | Delete employee (blocks if has direct reports) |

---

### 6.2 DepartmentController — `api/department`

| HTTP Verb | Route | Auth | Request | Response | Description |
|---|---|---|---|---|---|
| `GET` | `/api/department` | ✅ JWT | — | `IEnumerable<DepartmentDto>` | Get all departments |
| `GET` | `/api/department/{id}` | ✅ JWT | path: `Guid id` | `DepartmentDetailDto` / 404 | Get department with manager and employees |
| `POST` | `/api/department` | ✅ JWT | body: `CreateDepartmentDto` | `DepartmentDto` 201 / 400 | Create department |
| `PUT` | `/api/department/{id}` | ✅ JWT | path: `Guid id`, body: `UpdateDepartmentDto` | `DepartmentDto` / 400 / 404 | Update department |
| `DELETE` | `/api/department/{id}` | ✅ JWT | path: `Guid id` | 204 / 400 / 404 | Delete department (blocks if has employees or positions) |

---

### 6.3 JobPositionController — `api/jobposition`

| HTTP Verb | Route | Auth | Request | Response | Description |
|---|---|---|---|---|---|
| `GET` | `/api/jobposition` | ✅ JWT | — | `IEnumerable<PositionDto>` | Get all positions |
| `GET` | `/api/jobposition/{id}` | ✅ JWT | path: `Guid id` | `PositionDto` / 404 | Get position by ID |
| `POST` | `/api/jobposition` | ✅ JWT | body: `CreatePositionDto` | `PositionDto` 201 / 400 | Create position |
| `PUT` | `/api/jobposition/{id}` | ✅ JWT | path: `Guid id`, body: `UpdatePositionDto` | `PositionDto` / 400 / 404 | Update position |
| `DELETE` | `/api/jobposition/{id}` | ✅ JWT | path: `Guid id` | 204 / 400 / 404 | Delete position |

---

### 6.4 AttendanceController — `api/attendance` *(inferred from service)*

| HTTP Verb | Route | Auth | Request | Response | Description |
|---|---|---|---|---|---|
| `POST` | `/api/attendance/checkin` | ✅ JWT | body: `CheckInDto` | `AttendanceDto` 200 / 400 | Employee check-in |
| `POST` | `/api/attendance/checkout` | ✅ JWT | body: `CheckOutDto` | `AttendanceDto` 200 / 400 | Employee check-out |
| `POST` | `/api/attendance/manual` | ✅ JWT | body: `ManualAttendanceDto` | `AttendanceDto` 201 / 400 | Create manual attendance entry |
| `PUT` | `/api/attendance/{id}` | ✅ JWT | path: `Guid id`, body: `UpdateAttendanceDto` | `AttendanceDto` / 400 / 404 | Update attendance record |
| `GET` | `/api/attendance/summary/{employeeId}/{month}/{year}` | ✅ JWT | path params | `AttendanceSummaryDto` | Get monthly summary |

---

### 6.5 LeaveController — `api/leave` *(inferred from service)*

| HTTP Verb | Route | Auth | Request | Response | Description |
|---|---|---|---|---|---|
| `POST` | `/api/leave` | ✅ JWT | body: `CreateLeaveRequestDto` | `LeaveRequestDto` 201 / 400 | Submit leave request |
| `PUT` | `/api/leave/{id}` | ✅ JWT | path: `Guid id`, body: `UpdateLeaveRequestDto` | `LeaveRequestDto` / 400 | Update pending request |
| `POST` | `/api/leave/{id}/approve` | ✅ JWT | path: `Guid id` | `LeaveRequestDto` / 400 | Approve leave |
| `POST` | `/api/leave/{id}/reject` | ✅ JWT | path: `Guid id`, body: `{ reason }` | `LeaveRequestDto` / 400 | Reject leave |
| `POST` | `/api/leave/{id}/cancel` | ✅ JWT | path: `Guid id` | `LeaveRequestDto` / 400 | Cancel leave |
| `GET` | `/api/leave/{id}` | ✅ JWT | path: `Guid id` | `LeaveRequestDetailDto` / 404 | Get leave details |
| `GET` | `/api/leave/pending` | ✅ JWT | — | `IEnumerable<LeaveRequestDto>` | All pending requests |
| `GET` | `/api/leave/balance/{employeeId}/{year}` | ✅ JWT | path params | `LeaveBalanceDto` | Leave balance by year |
| `GET` | `/api/leave/history/{employeeId}` | ✅ JWT | path + query: `startDate?`, `endDate?`, `leaveType?` | `IEnumerable<LeaveRequestDto>` | Leave history |
| `DELETE` | `/api/leave/{id}` | ✅ JWT | path: `Guid id` | 204 / 400 | Delete pending request |

---

### 6.6 PayrollController — `api/payroll`

| HTTP Verb | Route | Auth | Request | Response | Description |
|---|---|---|---|---|---|
| `POST` | `/api/payroll/generate` | ✅ JWT | body: `GeneratePayrollDto` | `PayrollBatchDto` 200 / 400 | Batch payroll generation |
| `GET` | `/api/payroll/{id}` | ✅ JWT | path: `Guid id` | `PayrollDetailDto` / 404 | Get payroll details |
| `GET` | `/api/payroll/employee/{employeeId}` | ✅ JWT | path: `Guid employeeId` | `IEnumerable<PayrollDto>` | Employee's payroll history |
| `GET` | `/api/payroll/period/{month}/{year}` | ✅ JWT | path: `int month, int year` | `IEnumerable<PayrollDto>` | All payrolls for period |
| `PUT` | `/api/payroll/{id}` | ✅ JWT | path: `Guid id`, body: `UpdatePayrollDto` | `PayrollDetailDto` / 400 / 404 | Update draft payroll |
| `POST` | `/api/payroll/{id}/process` | ✅ JWT | path: `Guid id` | 204 / 400 / 404 | Process payroll (Draft → Processed) |
| `POST` | `/api/payroll/{id}/mark-paid` | ✅ JWT | path: `Guid id`, body: `MarkPaidDto` | 204 / 400 / 404 | Mark as paid (Processed → Paid) |
| `POST` | `/api/payroll/{id}/revert` | ✅ JWT | path: `Guid id` | 204 / 400 / 404 | Revert to draft (Processed → Draft) |
| `DELETE` | `/api/payroll/{id}` | ✅ JWT | path: `Guid id` | 204 / 400 / 404 | Delete draft payroll |
| `GET` | `/api/payroll/summary/{month}/{year}` | ✅ JWT | path: `int month, int year` | `PayrollSummaryDto` | Period aggregate summary |
| `POST` | `/api/payroll/{id}/recalculate` | ✅ JWT | path: `Guid id` | `PayrollDetailDto` / 400 / 404 | Recalculate draft payroll |

---

## 7. Business Rules & Validation

### 7.1 Employee Rules

| Rule | Details |
|---|---|
| **Unique EmployeeCode** | Per company; throws if duplicate |
| **Unique Email** | Per company (case-insensitive) |
| **Unique NationalId** | Per company |
| **Minimum Age** | Must be ≥ 18 years old at hire date |
| **Hire Date** | Cannot be in the future |
| **Salary Range** | Must be within `Position.MinSalary` and `Position.MaxSalary` |
| **Department Active** | Assigned department must have `IsActive = true` |
| **Position Active** | Assigned position must have `IsActive = true` |
| **Manager Active** | `ReportsToId` must be an `Active` employee |
| **Circular Reporting** | System traverses manager chain to prevent circular hierarchies |
| **Status: Inactive** | Only `Active` employees can be set to `Inactive` |
| **Status: Terminated** | Termination reason required; `EffectiveDate` must be ≥ HireDate |
| **Delete Block** | Cannot delete employee who has direct reports; reassign first |
| **Delete Cascade** | Deletion removes: Attendances, LeaveRequests (+ Attachments), Payrolls (+ LineItems), Documents |

---

### 7.2 Department Rules

| Rule | Details |
|---|---|
| **Unique Code** | Per company |
| **Unique Name** | Per company (case-insensitive trim comparison) |
| **Manager Active** | If `ManagerId` set, manager must be `Active` employee in same company |
| **Delete Block (Employees)** | Cannot delete if any employees are assigned; reassign first |
| **Delete Cascade (Positions)** | Cascades delete to all `JobPositions` under the department |

---

### 7.3 JobPosition Rules

| Rule | Details |
|---|---|
| **Unique Code** | Per company |
| **Salary Range** | `MaxSalary` must be > `MinSalary` |

---

### 7.4 Attendance Rules

| Rule | Details |
|---|---|
| **Employee Must Be Active** | `CheckIn` requires `EmployeeStatus.Active` |
| **No Future Dates** | CheckIn and ManualEntry cannot be for future dates |
| **No Duplicate CheckIn** | One check-in per employee per date |
| **No Duplicate CheckOut** | Cannot check out if already checked out today |
| **CheckOut After CheckIn** | `CheckOutTime` must be strictly after `CheckInTime` |
| **30-Minute Break Deduction** | Worked hours = `(CheckOut - CheckIn - 30min) / 60`; minimum 0 |
| **Overtime** | `Max(0, WorkedHours - 8)` |
| **Late Threshold** | If `CheckInTime > 09:00 + 15 minutes` → status = `Late` |
| **Payroll Lock** | Cannot create or modify attendance if payroll has been processed for that period |

---

### 7.5 Leave Request Rules

| Rule | Details |
|---|---|
| **Employee Must Be Active** | Only `Active` employees can request leave |
| **No Past Start Date** | `StartDate` cannot be in the past |
| **End ≥ Start** | `EndDate` must be ≥ `StartDate` |
| **Annual Leave Notice** | Annual leave requires ≥ 2 days advance notice |
| **Sufficient Balance** | For non-Unpaid leave: `TotalDays ≤ LeaveBalance.Available` |
| **No Overlap** | Cannot overlap with existing `Approved` or `Pending` leave |
| **No Weekend-Only Leave** | All days in the range cannot be Friday/Saturday only |
| **Auto-Init Balance** | If no balance record exists, one is auto-created with default entitlement |
| **Pending Balance** | On create: `balance.Pending += TotalDays` |
| **Approve Side Effect** | `balance.Pending -= TotalDays`, `balance.Used += TotalDays` |
| **Reject Side Effect** | `balance.Pending -= TotalDays` (restored) |
| **Cancel (Approved)** | `balance.Used -= TotalDays` (restored) |
| **Delete** | Only `Pending` requests; restores `balance.Pending` |
| **Update** | Only `Pending` requests can be updated |
| **Sick Certificate** | Sick leave > 3 days → note: `SICK_LEAVE_CERTIFICATE_THRESHOLD = 3` (enforced externally) |

#### Leave Type Default Entitlements (days/year)

| LeaveType | Default Days |
|---|---|
| `Annual` | 21 |
| `Sick` | 14 |
| `Emergency` | 3 |
| `Maternity` | 90 |
| `Paternity` | 3 |
| `Study` | 7 |
| `Unpaid` | Unlimited (no balance check) |

---

### 7.6 Payroll Rules

| Rule | Details |
|---|---|
| **No Duplicate Payroll** | One payroll per employee per month/year |
| **Valid Month** | 1–12 |
| **Valid Year** | 2000 to `currentYear + 1` |
| **Attendance Required** | Cannot generate payroll with zero attendance records in the period |
| **Inactive Filter** | `IncludeInactive = false` (default) skips non-Active employees |
| **Payroll Lock on Attendance** | Attendance cannot be modified once payroll is not `Draft` |
| **Status Transitions** | `Draft → Processed → Paid` only; `Processed → Draft` (revert); no other transitions |
| **Edit/Delete Restriction** | Only `Draft` payrolls can be edited, deleted, or recalculated |
| **Mark Paid Restriction** | Only `Processed` payrolls can be marked as paid |
| **Revert Restriction** | Only `Processed` payrolls can revert to `Draft` |
| **Batch Tolerance** | Batch generation does not abort on single employee failure; errors collected in `PayrollBatchDto.Errors` |

#### Payroll Calculation Logic

| Component | Formula |
|---|---|
| **Working Days** | Count of Mon–Thu days in month (excludes Fri/Sat) |
| **Daily Rate** | `Employee.Salary / WorkingDays` |
| **Basic Salary** | `PresentDays × DailyRate` (Present + Late count as present) |
| **Transport Allowance** | `Salary × 10%` |
| **Housing Allowance** | `Salary × 20%` |
| **Absence Deduction** | `AbsentDays × DailyRate` |
| **Unpaid Leave Deduction** | `UnpaidLeaveDays × DailyRate` |
| **Social Security** | `Salary × 11%` |
| **Income Tax** | Progressive: 0% (≤15K), 10% (15K–30K), 15% (30K–45K), 20% (>45K) |
| **Net Salary** | `BasicSalary + TotalAllowances - TotalDeductions` |

#### Status Enum Values

| Enum | Values |
|---|---|
| `EmployeeStatus` | `Active`, `Inactive`, `OnLeave`, `Terminated` |
| `AttendanceStatus` | `Present`, `Absent`, `Late`, `OnLeave` |
| `LeaveRequestStatus` | `Pending`, `Approved`, `Rejected`, `Cancelled` |
| `LeaveType` | `Annual`, `Sick`, `Emergency`, `Maternity`, `Paternity`, `Study`, `Unpaid` |
| `PayrollStatus` | `Draft`, `Processed`, `Paid` |
| `PayrollLineItemType` | `Allowance`, `Deduction` |
| `PaymentMethod` | *(enum values not specified in provided code)* |
| `PositionLevel` | *(enum values not specified in provided code)* |
| `Gender` | *(enum values not specified in provided code)* |
| `MaritalStatus` | *(enum values not specified in provided code)* |

---

## 8. Cross-Module Dependencies

| Interface / Service | Source Module | Usage in HR |
|---|---|---|
| `ICurrentUserService` | Core / Infrastructure | Every service and repository uses `.CompanyId` for tenant scoping; controllers use `.Name` for audit `CreatedBy`/`ModifiedBy` |
| `IModuleAccessService` | Core | Every service method calls `EnsureHrAccessAsync()` to verify the company has the HR module enabled before proceeding |
| `ICompanyEntity` | Domain.Abstractions | All HR entities implement this interface to carry `CompanyId` |
| `AppDbContext` | Infrastructure | `DepartmentController` directly injects `AppDbContext` to query `JobPositions` count before delete — **note:** this is a violation of the service layer pattern; direct DbContext access in controllers should be moved to `IDepartmentRepository` |

### ⚠️ Architectural Issues Detected

| Issue | Location | Recommendation |
|---|---|---|
| **Direct DbContext in Controller** | `DepartmentController.Delete()` uses `_context.JobPositions.CountAsync()` directly | Move to `IDepartmentRepository.GetPositionCountAsync()` |
| **Business logic in Controller** | `DepartmentController` validates manager existence and name uniqueness | Move all validation to `IDepartmentService` |
| **No service layer for Department** | `DepartmentController` calls `IDepartmentRepository` directly | Create `IDepartmentService` + `DepartmentService` to follow module pattern |
| **No service layer for JobPosition** | `JobPositionController` calls `IPositionRepository` directly | Create `IPositionService` + `PositionService` |
| **Hardcoded today date** | `LeaveRequestService.CreateAsync()` uses `new DateOnly(2020, 1, 1)` instead of `DateOnly.FromDateTime(DateTime.Today)` | **Bug:** All leave date validations are comparing against 2020-01-01; replace with `DateOnly.FromDateTime(DateTime.Today)` |

---

**Document Version:** 1.0
**Last Updated:** 2024
**Status:** Ready for AI Consumption
