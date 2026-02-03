using ERPSystem.Application.DTOs.HR;
using ERPSystem.Application.DTOs.HR.Department;
using ERPSystem.Application.DTOs.HR.Employee;
using ERPSystem.Application.DTOs.HR.JobPosition;
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
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IDepartmentRepository _departmentRepo;
        private readonly IPositionRepository _positionRepo;
        private readonly ICurrentUserService _cuurentUser;
        private readonly IModuleAccessService _moduleAccess;
        public EmployeeService(
            IEmployeeRepository employeeRepo,
            IDepartmentRepository departmentRepo,
            IPositionRepository positionRepo,
            ICurrentUserService currentUser,
            IModuleAccessService moduleAccess)
        {
            _employeeRepo = employeeRepo;
            _departmentRepo = departmentRepo;
            _positionRepo = positionRepo;
            _cuurentUser = currentUser;
            _moduleAccess = moduleAccess;
        }

        // ================== GUARDS ==================

        private async Task<Employee> GetValidEmployeeAsync(Guid id, CancellationToken ct = default)
        {
            var emp = await _employeeRepo.GetByIdAsync(id, _cuurentUser.CompanyId, ct);
            if (emp == null)
                throw new InvalidOperationException("Employee not found or does not belong to your company.");
            return emp;
        }

        private async Task<Employee> GetValidEmployeeWithDetailsAsync(Guid id, CancellationToken ct = default)
        {
            var emp = await _employeeRepo.GetByIdWithDetailsAsync(id, _cuurentUser.CompanyId, ct);
            if (emp == null)
                throw new InvalidOperationException("Employee not found or does not belong to your company.");
            return emp;
        }

        private async Task<Department> GetValidDepartmentAsync(Guid departmentId, CancellationToken ct = default)
        {
            var dept = await _departmentRepo.GetByIdAsync(departmentId, _cuurentUser.CompanyId, ct);
            if (dept == null)
                throw new InvalidOperationException("Department not found or does not belong to your company.");

            if (!dept.IsActive)
                throw new InvalidOperationException("Department is not active.");

            return dept;
        }

        private async Task<JobPosition> GetValidPositionAsync(Guid positionId, CancellationToken ct = default)
        {
            var pos = await _positionRepo.GetByIdAsync(positionId, _cuurentUser.CompanyId, ct);
            if (pos == null)
                throw new InvalidOperationException("Position not found or does not belong to your company.");

            if (!pos.IsActive)
                throw new InvalidOperationException("Position is not active.");

            return pos;
        }

        // ================== CREATE ==================

        public async Task<EmployeeDetailDto> CreateAsync(CreateEmployeeDto dto, string createdBy, CancellationToken ct = default)
        {
            await _moduleAccess.EnsureHrAccessAsync(ct);
            if (await _employeeRepo.ExistsByEmployeeCodeAsync(dto.EmployeeCode, _cuurentUser.CompanyId))
                throw new InvalidOperationException($"Employee code '{dto.EmployeeCode}' already exists");

            if (await _employeeRepo.ExistsByEmailAsync(dto.Email, _cuurentUser.CompanyId))
                throw new InvalidOperationException($"Email '{dto.Email}' already exists");

            if (await _employeeRepo.ExistsByNationalIdAsync(dto.NationalId, _cuurentUser.CompanyId))
                throw new InvalidOperationException($"National ID '{dto.NationalId}' already exists");

            // Age >= 18 at hire date
            var ageAtHire = dto.HireDate.Year - dto.DateOfBirth.Year;
            if (dto.DateOfBirth > dto.HireDate.AddYears(-ageAtHire))
                ageAtHire--;

            if (ageAtHire < 18)
                throw new InvalidOperationException("Employee must be at least 18 years old at hire date");

            if (dto.HireDate > DateTime.Today)
                throw new InvalidOperationException("Hire date cannot be in the future");

            // Department/Position (company scoped + active)
            var department = await GetValidDepartmentAsync(dto.DepartmentId);
            var position = await GetValidPositionAsync(dto.PositionId);

            if (dto.Salary < position.MinSalary || dto.Salary > position.MaxSalary)
                throw new InvalidOperationException(
                    $"Salary must be between {position.MinSalary} and {position.MaxSalary} for this position");

            // ReportsTo (company scoped + active)
            if (dto.ReportsToId.HasValue)
            {
                var manager = await _employeeRepo.GetByIdAsync(dto.ReportsToId.Value, _cuurentUser.CompanyId);
                if (manager == null)
                    throw new InvalidOperationException("Manager not found or does not belong to your company.");

                if (manager.Status != EmployeeStatus.Active)
                    throw new InvalidOperationException("Manager must be an active employee");
            }

            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                CompanyId = _cuurentUser.CompanyId, // ok even if BaseRepo enforces
                EmployeeCode = dto.EmployeeCode,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,

                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Nationality = dto.Nationality,
                NationalId = dto.NationalId,

                MaritalStatus = dto.MaritalStatus,
                HireDate = dto.HireDate,
                ProbationEndDate = dto.HireDate.AddMonths(dto.ProbationPeriodMonths),
                Status = EmployeeStatus.Active,

                DepartmentId = department.Id,
                PositionId = position.Id,
                ReportsToId = dto.ReportsToId,

                CurrentCity = dto.CurrentAddress.City,
                CurrentCountry = dto.CurrentAddress.Country,
                CurrentPostalCode = dto.CurrentAddress.PostalCode,

                BankAccountNumber = dto.BankAccountNumber,
                BankName = dto.BankName,
                BankBranch = dto.BankBranch,

                Salary = dto.Salary,
                Currency = dto.Currency,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            await _employeeRepo.AddAsync(employee);

            var created = await _employeeRepo.GetByIdWithDetailsAsync(employee.Id, _cuurentUser.CompanyId);
            return MapToDetailDto(created!);
        }

        // ================== UPDATE ==================

        public async Task<EmployeeDetailDto> UpdateAsync(Guid id, UpdateEmployeeDto dto, string modifiedBy, CancellationToken ct = default)
        {
            await _moduleAccess.EnsureHrAccessAsync(ct);
            var employee = await GetValidEmployeeAsync(id);

            // Email uniqueness if changed (company-scoped)
            if (dto.Email != null && dto.Email != employee.Email)
            {
                if (await _employeeRepo.ExistsByEmailAsync(dto.Email, _cuurentUser.CompanyId))
                    throw new InvalidOperationException($"Email '{dto.Email}' already exists");
                employee.Email = dto.Email;
            }

            // Department if changed (company-scoped + active)
            if (dto.DepartmentId.HasValue && dto.DepartmentId != employee.DepartmentId)
            {
                var department = await GetValidDepartmentAsync(dto.DepartmentId.Value);
                employee.DepartmentId = department.Id;
            }

            // Position if changed (company-scoped + active) + salary range
            if (dto.PositionId.HasValue && dto.PositionId != employee.PositionId)
            {
                var position = await GetValidPositionAsync(dto.PositionId.Value);
                employee.PositionId = position.Id;

                if (dto.Salary.HasValue)
                {
                    if (dto.Salary.Value < position.MinSalary || dto.Salary.Value > position.MaxSalary)
                        throw new InvalidOperationException(
                            $"Salary must be between {position.MinSalary} and {position.MaxSalary}");
                }
            }

            // Manager if changed (company-scoped + active + no circular)
            if (dto.ReportsToId.HasValue && dto.ReportsToId != employee.ReportsToId)
            {
                if (dto.ReportsToId == id)
                    throw new InvalidOperationException("Employee cannot report to themselves");

                var manager = await _employeeRepo.GetByIdAsync(dto.ReportsToId.Value, _cuurentUser.CompanyId);
                if (manager == null)
                    throw new InvalidOperationException("Manager not found or does not belong to your company.");

                if (manager.Status != EmployeeStatus.Active)
                    throw new InvalidOperationException("Manager must be active");

                if (await _employeeRepo.HasCircularReportingAsync(id, dto.ReportsToId.Value, _cuurentUser.CompanyId))
                    throw new InvalidOperationException("Cannot create circular reporting structure");

                employee.ReportsToId = dto.ReportsToId;
            }

            // Update other fields
            if (dto.FirstName != null) employee.FirstName = dto.FirstName;
            if (dto.LastName != null) employee.LastName = dto.LastName;
            if (dto.PhoneNumber != null) employee.PhoneNumber = dto.PhoneNumber;
            if (dto.DateOfBirth.HasValue) employee.DateOfBirth = dto.DateOfBirth.Value;
            if (dto.Gender.HasValue) employee.Gender = dto.Gender.Value;
            if (dto.Nationality != null) employee.Nationality = dto.Nationality;
            if (dto.MaritalStatus.HasValue) employee.MaritalStatus = dto.MaritalStatus.Value;
            if (dto.Salary.HasValue) employee.Salary = dto.Salary.Value;

            if (dto.CurrentAddress != null)
            {
                employee.CurrentCity = dto.CurrentAddress.City;
                employee.CurrentCountry = dto.CurrentAddress.Country;
                employee.CurrentPostalCode = dto.CurrentAddress.PostalCode;
            }

            if (dto.BankAccountNumber != null) employee.BankAccountNumber = dto.BankAccountNumber;
            if (dto.BankName != null) employee.BankName = dto.BankName;
            if (dto.BankBranch != null) employee.BankBranch = dto.BankBranch;

            employee.ModifiedAt = DateTime.UtcNow;
            employee.ModifiedBy = modifiedBy;

            await _employeeRepo.UpdateAsync(employee);

            var updated = await _employeeRepo.GetByIdWithDetailsAsync(id, _cuurentUser.CompanyId);
            return MapToDetailDto(updated!);
        }

        // ================== UPDATE STATUS ==================

        public async Task UpdateStatusAsync(Guid id, UpdateEmployeeDto dto, string modifiedBy, CancellationToken ct = default)
        {
            await _moduleAccess.EnsureHrAccessAsync(ct);
            var employee = await GetValidEmployeeAsync(id);

            if (employee.Status != EmployeeStatus.Active && dto.Status == EmployeeStatus.Inactive)
                throw new InvalidOperationException("Only active employees can be marked inactive");

            if (dto.Status == EmployeeStatus.Terminated && string.IsNullOrWhiteSpace(dto.Reason))
                throw new InvalidOperationException("Termination reason is required");

            if (dto.Status == EmployeeStatus.Terminated && dto.EffectiveDate < employee.HireDate)
                throw new InvalidOperationException("Termination date cannot be before hire date");

            employee.Status = dto.Status;

            if (dto.Status == EmployeeStatus.Terminated)
                employee.TerminationDate = dto.EffectiveDate;

            employee.ModifiedAt = DateTime.UtcNow;
            employee.ModifiedBy = modifiedBy;

            await _employeeRepo.UpdateAsync(employee);
        }

        // ================== READ ==================

        public async Task<EmployeeDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            await _moduleAccess.EnsureHrAccessAsync(ct);
            var employee = await _employeeRepo.GetByIdWithDetailsAsync(id, _cuurentUser.CompanyId);
            return employee != null ? MapToDetailDto(employee) : null;
        }

        public async Task<IEnumerable<EmployeeListDto>> GetAllAsync(CancellationToken ct = default)
        {
            await _moduleAccess.EnsureHrAccessAsync(ct);
            var employees = await _employeeRepo.GetAllAsync(_cuurentUser.CompanyId);
            return employees.Select(MapToListDto);
        }

        public async Task<IEnumerable<EmployeeListDto>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default)
        {
            await _moduleAccess.EnsureHrAccessAsync(ct);
            // optional: validate dept in same company to avoid probing
            await GetValidDepartmentAsync(departmentId);

            var employees = await _employeeRepo.GetByDepartmentIdAsync(departmentId, _cuurentUser.CompanyId);
            return employees.Select(MapToListDto);
        }

        public async Task<IEnumerable<EmployeeListDto>> GetByStatusAsync(EmployeeStatus status,CancellationToken ct = default)
        {
            await _moduleAccess.EnsureHrAccessAsync(ct);
            var employees = await _employeeRepo.GetByStatusAsync(status, _cuurentUser.CompanyId);
            return employees.Select(MapToListDto);
        }

        // ================== DELETE ==================

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _moduleAccess.EnsureHrAccessAsync(ct);
            var employee = await GetValidEmployeeWithDetailsAsync(id);

            if (employee.DirectReports.Any())
                throw new InvalidOperationException(
                    "Cannot delete employee with subordinates. Please reassign them first.");

            await _employeeRepo.DeleteAsync(id, _cuurentUser.CompanyId,ct);
        }

        // ================== MAPPERS ==================

        private EmployeeListDto MapToListDto(Employee e)
        {
            return new EmployeeListDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FullName = e.FullName,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                DepartmentName = e.Department?.Name,
                PositionTitle = e.Position?.Title,
                Status = e.Status.ToString(),
                HireDate = e.HireDate,
                ProfileImageUrl = e.ProfileImageUrl
            };
        }

        private EmployeeDetailDto MapToDetailDto(Employee e)
        {
            return new EmployeeDetailDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FirstName = e.FirstName,
                LastName = e.LastName,
                FullName = e.FullName,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                DateOfBirth = e.DateOfBirth,
                Gender = e.Gender.ToString(),
                Nationality = e.Nationality,
                NationalId = e.NationalId,
                MaritalStatus = e.MaritalStatus.ToString(),
                HireDate = e.HireDate,
                ProbationEndDate = e.ProbationEndDate,
                TerminationDate = e.TerminationDate,
                Status = e.Status.ToString(),

                Department = e.Department != null ? new DepartmentDto
                {
                    Id = e.Department.Id,
                    Code = e.Department.Code,
                    Name = e.Department.Name,
                    Description = e.Department.Description,
                    IsActive = e.Department.IsActive,
                    CreatedAt = e.Department.CreatedAt
                } : null,

                Position = e.Position != null ? new PositionDto
                {
                    Id = e.Position.Id,
                    Code = e.Position.Code,
                    Title = e.Position.Title,
                    Description = e.Position.Description,
                    Level = e.Position.Level.ToString(),
                    MinSalary = e.Position.MinSalary,
                    MaxSalary = e.Position.MaxSalary,
                    IsActive = e.Position.IsActive
                } : null,

                ReportsTo = e.Manager != null ? MapToListDto(e.Manager) : null,
                DirectReports = e.DirectReports.Select(MapToListDto).ToList(),

                CurrentAddress = new AddressDto
                {
                    City = e.CurrentCity,
                    Country = e.CurrentCountry,
                    PostalCode = e.CurrentPostalCode
                },
                BankAccountNumber = e.BankAccountNumber,
                BankName = e.BankName,
                BankBranch = e.BankBranch,
                Salary = e.Salary,
                Currency = e.Currency,
                ProfileImageUrl = e.ProfileImageUrl,

                Documents = e.Documents.Select(d => new DocumentDto
                {
                    Id = d.Id,
                    DocumentName = d.DocumentName,
                    DocumentType = d.DocumentType,
                    FilePath = d.FilePath,
                    FileSizeBytes = d.FileSizeBytes,
                    FileExtension = d.FileExtension,
                    UploadedAt = d.UploadedAt,
                    UploadedBy = d.UploadedBy
                }).ToList(),

                CreatedAt = e.CreatedAt,
                ModifiedAt = e.ModifiedAt
            };
        }
    }
}
