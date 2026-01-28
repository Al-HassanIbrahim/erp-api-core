using ERPSystem.Application.DTOs.HR;
using ERPSystem.Application.DTOs.HR.Department;
using ERPSystem.Application.DTOs.HR.Employee;
using ERPSystem.Application.DTOs.HR.JobPosition;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.Services.Hr
{
    public class EmployeeService:IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IDepartmentRepository _departmentRepo;
        private readonly IPositionRepository _positionRepo;

        public EmployeeService(
            IEmployeeRepository employeeRepo,
            IDepartmentRepository departmentRepo,
            IPositionRepository positionRepo)
        {
            _employeeRepo = employeeRepo;
            _departmentRepo = departmentRepo;
            _positionRepo = positionRepo;
        }

        public async Task<EmployeeDetailDto> CreateAsync(CreateEmployeeDto dto, string createdBy)
        {
            // Validation: Employee code must be unique
            if (await _employeeRepo.ExistsByEmployeeCodeAsync(dto.EmployeeCode))
                throw new InvalidOperationException($"Employee code '{dto.EmployeeCode}' already exists");

            // Validation: Email must be unique
            if (await _employeeRepo.ExistsByEmailAsync(dto.Email))
                throw new InvalidOperationException($"Email '{dto.Email}' already exists");

            // Validation: National ID must be unique
            if (await _employeeRepo.ExistsByNationalIdAsync(dto.NationalId))
                throw new InvalidOperationException($"National ID '{dto.NationalId}' already exists");

            // Validation: Age must be at least 18 years at hire date
            var ageAtHire = dto.HireDate.Year - dto.DateOfBirth.Year;
            if (dto.DateOfBirth > dto.HireDate.AddYears(-ageAtHire))
                ageAtHire--;

            if (ageAtHire < 18)
                throw new InvalidOperationException("Employee must be at least 18 years old at hire date");

            // Validation: Hire date cannot be in the future
            if (dto.HireDate > DateTime.Today)
                throw new InvalidOperationException("Hire date cannot be in the future");

            // Validation: Department must exist
            var department = await _departmentRepo.GetByIdAsync(dto.DepartmentId);
            if (department == null)
                throw new InvalidOperationException("Department not found");

            // Validation: Position must exist
            var position = await _positionRepo.GetByIdAsync(dto.PositionId);
            if (position == null)
                throw new InvalidOperationException("Position not found");

            // Validation: Salary must be within position's min-max range
            if (dto.Salary < position.MinSalary || dto.Salary > position.MaxSalary)
                throw new InvalidOperationException(
                    $"Salary must be between {position.MinSalary} and {position.MaxSalary} for this position");

            // Validation: If reportsTo is set, manager must be active employee
            if (dto.ReportsToId.HasValue)
            {
                var manager = await _employeeRepo.GetByIdAsync(dto.ReportsToId.Value);
                if (manager == null)
                    throw new InvalidOperationException("Manager not found");

                if (manager.Status != EmployeeStatus.Active)
                    throw new InvalidOperationException("Manager must be an active employee");
            }

            // Handle address copying if SameAsCurrent is true
            

            // Create employee entity
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
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
                DepartmentId = dto.DepartmentId,
                PositionId = dto.PositionId,
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

            // Return full details
            var created = await _employeeRepo.GetByIdWithDetailsAsync(employee.Id);
            return MapToDetailDto(created!);
        }

        public async Task<EmployeeDetailDto> UpdateAsync(Guid id, UpdateEmployeeDto dto, string modifiedBy)
        {
            var employee = await _employeeRepo.GetByIdAsync(id);
            if (employee == null)
                throw new InvalidOperationException("Employee not found");

            // Check email uniqueness if changed
            if (dto.Email != null && dto.Email != employee.Email)
            {
                if (await _employeeRepo.ExistsByEmailAsync(dto.Email))
                    throw new InvalidOperationException($"Email '{dto.Email}' already exists");
                employee.Email = dto.Email;
            }

            // Validate department if changed
            if (dto.DepartmentId.HasValue && dto.DepartmentId != employee.DepartmentId)
            {
                var department = await _departmentRepo.GetByIdAsync(dto.DepartmentId.Value);
                if (department == null)
                    throw new InvalidOperationException("Department not found");
                employee.DepartmentId = dto.DepartmentId.Value;
            }

            // Validate position and salary if changed
            if (dto.PositionId.HasValue && dto.PositionId != employee.PositionId)
            {
                var position = await _positionRepo.GetByIdAsync(dto.PositionId.Value);
                if (position == null)
                    throw new InvalidOperationException("Position not found");
                employee.PositionId = dto.PositionId.Value;

                // Check salary range
                if (dto.Salary.HasValue)
                {
                    if (dto.Salary.Value < position.MinSalary || dto.Salary.Value > position.MaxSalary)
                        throw new InvalidOperationException(
                            $"Salary must be between {position.MinSalary} and {position.MaxSalary}");
                }
            }

            // Validate manager if changed
            if (dto.ReportsToId.HasValue && dto.ReportsToId != employee.ReportsToId)
            {
                // Manager cannot report to themselves
                if (dto.ReportsToId == id)
                    throw new InvalidOperationException("Employee cannot report to themselves");

                var manager = await _employeeRepo.GetByIdAsync(dto.ReportsToId.Value);
                if (manager == null)
                    throw new InvalidOperationException("Manager not found");

                if (manager.Status != EmployeeStatus.Active)
                    throw new InvalidOperationException("Manager must be active");

                // Check for circular reporting
                if (await _employeeRepo.HasCircularReportingAsync(id, dto.ReportsToId.Value))
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

            var updated = await _employeeRepo.GetByIdWithDetailsAsync(id);
            return MapToDetailDto(updated!);
        }

        public async Task UpdateStatusAsync(Guid id, UpdateEmployeeDto dto, string modifiedBy)
        {
            var employee = await _employeeRepo.GetByIdAsync(id);
            if (employee == null)
                throw new InvalidOperationException("Employee not found");

            // Validation: Only active employees can be marked inactive
            if (employee.Status != EmployeeStatus.Active && dto.Status == EmployeeStatus.Inactive)
                throw new InvalidOperationException("Only active employees can be marked inactive");

            // Validation: Termination requires reason
            if (dto.Status == EmployeeStatus.Terminated && string.IsNullOrWhiteSpace(dto.Reason))
                throw new InvalidOperationException("Termination reason is required");

            // Validation: Termination date cannot be before hire date
            if (dto.Status == EmployeeStatus.Terminated && dto.EffectiveDate < employee.HireDate)
                throw new InvalidOperationException("Termination date cannot be before hire date");

            employee.Status = dto.Status;

            if (dto.Status == EmployeeStatus.Terminated)
            {
                employee.TerminationDate = dto.EffectiveDate;
            }

            employee.ModifiedAt = DateTime.UtcNow;
            employee.ModifiedBy = modifiedBy;

            await _employeeRepo.UpdateAsync(employee);
        }

        public async Task<EmployeeDetailDto?> GetByIdAsync(Guid id)
        {
            var employee = await _employeeRepo.GetByIdWithDetailsAsync(id);
            return employee != null ? MapToDetailDto(employee) : null;
        }

        public async Task<IEnumerable<EmployeeListDto>> GetAllAsync()
        {
            var employees = await _employeeRepo.GetAllAsync();
            return employees.Select(MapToListDto);
        }

        public async Task<IEnumerable<EmployeeListDto>> GetByDepartmentAsync(Guid departmentId)
        {
            var employees = await _employeeRepo.GetByDepartmentIdAsync(departmentId);
            return employees.Select(MapToListDto);
        }

        public async Task<IEnumerable<EmployeeListDto>> GetByStatusAsync(EmployeeStatus status)
        {
            var employees = await _employeeRepo.GetByStatusAsync(status);
            return employees.Select(MapToListDto);
        }

        public async Task DeleteAsync(Guid id)
        {
            var employee = await _employeeRepo.GetByIdWithDetailsAsync(id);
            if (employee == null)
                throw new InvalidOperationException("Employee not found");

            // Check if employee has subordinates
            if (employee.DirectReports.Any())
                throw new InvalidOperationException(
                    "Cannot delete employee with subordinates. Please reassign them first.");

            await _employeeRepo.DeleteAsync(id);
        }

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
