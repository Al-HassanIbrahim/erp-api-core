using ERPSystem.Application.DTOs.HR.Department;
using ERPSystem.Application.DTOs.HR.Employee;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.HR;
using ERPSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentRepository _departmentRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ICurrentUserService _currentUser;

        public DepartmentController(
            IDepartmentRepository departmentRepo,
            IEmployeeRepository employeeRepo,
            ICurrentUserService currentUser)
        {
            _departmentRepo = departmentRepo;
            _employeeRepo = employeeRepo;
            _currentUser = currentUser;
        }

        /// <summary>Get all departments (company-scoped)</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAll(CancellationToken ct)
        {

            var departments = await _departmentRepo.GetAllAsync(_currentUser.CompanyId, ct);
            return Ok(departments.Select(MapToDto));
        }

        /// <summary>Get department by id with details (company-scoped)</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(DepartmentDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DepartmentDetailDto>> GetById(Guid id, CancellationToken ct)
        {
            var department = await _departmentRepo.GetByIdWithDetailsAsync(id, _currentUser.CompanyId, ct);
            if (department == null)
                return NotFound(new { error = "Department not found" });

            return Ok(MapToDetailDto(department));
        }

        /// <summary>Create new department (company-scoped)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(DepartmentDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<DepartmentDto>> Create([FromBody] CreateDepartmentDto dto, CancellationToken ct)
        {
            try
            {
                if (await _departmentRepo.ExistsByCodeAsync(dto.Code, _currentUser.CompanyId, ct))
                    return BadRequest(new { error = $"Department code '{dto.Code}' already exists" });

                if (await _departmentRepo.ExistsByNameAsync(dto.Name, _currentUser.CompanyId, ct))
                    return BadRequest(new { error = $"Department name '{dto.Name}' already exists" });

                if (dto.ManagerId.HasValue)
                {
                    var manager = await _employeeRepo.GetByIdAsync(dto.ManagerId.Value, _currentUser.CompanyId, ct);
                    if (manager == null)
                        return BadRequest(new { error = "Manager not found or does not belong to your company." });

                    if (manager.Status != EmployeeStatus.Active)
                        return BadRequest(new { error = "Manager must be active." });
                }

                var department = new Department
                {
                    Id = Guid.NewGuid(),
                    CompanyId = _currentUser.CompanyId,
                    Code = dto.Code,
                    Name = dto.Name,
                    Description = dto.Description,
                    ManagerId = dto.ManagerId,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                await _departmentRepo.AddAsync(department, ct);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = department.Id },
                    MapToDto(department));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>Update department (company-scoped)</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(DepartmentDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DepartmentDto>> Update(Guid id, [FromBody] UpdateDepartmentDto dto, CancellationToken ct)
        {
            try
            {
                var department = await _departmentRepo.GetByIdAsync(id, _currentUser.CompanyId, ct);
                if (department == null)
                    return NotFound(new { error = "Department not found" });

                // Name uniqueness (exclude current dept) - company scoped
                var all = await _departmentRepo.GetAllAsync(_currentUser.CompanyId, ct);
                if (all.Any(d => d.Id != id && d.Name.Trim().ToLower() == dto.Name.Trim().ToLower()))
                    return BadRequest(new { error = $"Department name '{dto.Name}' already exists" });

                if (dto.ManagerId.HasValue)
                {
                    var manager = await _employeeRepo.GetByIdAsync(dto.ManagerId.Value, _currentUser.CompanyId, ct);
                    if (manager == null)
                        return BadRequest(new { error = "Manager not found or does not belong to your company." });

                    if (manager.Status != EmployeeStatus.Active)
                        return BadRequest(new { error = "Manager must be active." });
                }

                department.Name = dto.Name;
                department.Description = dto.Description;
                department.ManagerId = dto.ManagerId;
                department.IsActive = dto.IsActive;
                department.ModifiedAt = DateTime.UtcNow;

                await _departmentRepo.UpdateAsync(department, ct);

                return Ok(MapToDto(department));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>Delete department (company-scoped)</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                var department = await _departmentRepo.GetByIdAsync(id, _currentUser.CompanyId, ct);
                if (department == null)
                    return NotFound(new { error = "Department not found" });

                var employeeCount = await _departmentRepo.GetEmployeeCountAsync(id, _currentUser.CompanyId, ct);
                if (employeeCount > 0)
                    return BadRequest(new
                    {
                        error = $"Cannot delete department with {employeeCount} employees. Please reassign them first."
                    });

                await _departmentRepo.DeleteAsync(id, _currentUser.CompanyId, ct);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ================== MAPPERS ==================

        private DepartmentDto MapToDto(Department d) => new()
        {
            Id = d.Id,
            Code = d.Code,
            Name = d.Name,
            Description = d.Description,
            ManagerId = d.ManagerId,
            ManagerName = d.Manager?.FullName,
            EmployeeCount = d.Employees?.Count ?? 0,
            IsActive = d.IsActive,
            CreatedAt = d.CreatedAt
        };

        private DepartmentDetailDto MapToDetailDto(Department d) => new()
        {
            Id = d.Id,
            Code = d.Code,
            Name = d.Name,
            Description = d.Description,
            ManagerId = d.ManagerId,
            ManagerName = d.Manager?.FullName,
            EmployeeCount = d.Employees?.Count ?? 0,
            IsActive = d.IsActive,
            CreatedAt = d.CreatedAt,

            Manager = d.Manager != null ? new EmployeeListDto
            {
                Id = d.Manager.Id,
                EmployeeCode = d.Manager.EmployeeCode,
                FullName = d.Manager.FullName,
                Email = d.Manager.Email,
                PhoneNumber = d.Manager.PhoneNumber,
                Status = d.Manager.Status.ToString(),
                HireDate = d.Manager.HireDate
            } : null,

            Employees = d.Employees?.Select(e => new EmployeeListDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FullName = e.FullName,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                PositionTitle = e.Position?.Title,
                Status = e.Status.ToString(),
                HireDate = e.HireDate
            }).ToList() ?? new List<EmployeeListDto>()
        };
    }
}
