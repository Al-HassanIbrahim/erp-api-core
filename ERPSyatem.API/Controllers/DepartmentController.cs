using ERPSystem.Application.DTOs.HR.Department;
using ERPSystem.Application.DTOs.HR.Employee;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.HR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentRepository _departmentRepo;
        private readonly IEmployeeRepository _employeeRepo;

        public DepartmentController(
            IDepartmentRepository departmentRepo,
            IEmployeeRepository employeeRepo)
        {
            _departmentRepo = departmentRepo;
            _employeeRepo = employeeRepo;
        }

        /// <summary>
        /// Get all departments
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), 200)]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAll()
        {
            var departments = await _departmentRepo.GetAllAsync();
            return Ok(departments.Select(MapToDto));
        }

        /// <summary>
        /// Get department by ID with details
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DepartmentDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DepartmentDetailDto>> GetById(Guid id)
        {
            var department = await _departmentRepo.GetByIdWithDetailsAsync(id);
            if (department == null)
                return NotFound(new { error = "Department not found" });

            return Ok(MapToDetailDto(department));
        }

        /// <summary>
        /// Create new department
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(DepartmentDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<DepartmentDto>> Create([FromBody] CreateDepartmentDto dto)
        {
            try
            {
                // Validation: Code must be unique
                if (await _departmentRepo.ExistsByCodeAsync(dto.Code))
                    return BadRequest(new { error = $"Department code '{dto.Code}' already exists" });

                // Validation: Name must be unique
                if (await _departmentRepo.ExistsByNameAsync(dto.Name))
                    return BadRequest(new { error = $"Department name '{dto.Name}' already exists" });

                // Validation: Manager must exist if provided
                if (dto.ManagerId.HasValue)
                {
                    var manager = await _employeeRepo.GetByIdAsync(dto.ManagerId.Value);
                    if (manager == null)
                        return BadRequest(new { error = "Manager not found" });
                }

                var department = new Department
                {
                    Id = Guid.NewGuid(),
                    Code = dto.Code,
                    Name = dto.Name,
                    Description = dto.Description,
                    ManagerId = dto.ManagerId,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                await _departmentRepo.AddAsync(department);
                return CreatedAtAction(nameof(GetById), new { id = department.Id }, MapToDto(department));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update department
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DepartmentDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DepartmentDto>> Update(Guid id, [FromBody] UpdateDepartmentDto dto)
        {
            try
            {
                var department = await _departmentRepo.GetByIdAsync(id);
                if (department == null)
                    return NotFound(new { error = "Department not found" });

                // Validation: Name must be unique (excluding current)
                var existingByName = await _departmentRepo.GetAllAsync();
                if (existingByName.Any(d => d.Name.ToLower() == dto.Name.ToLower() && d.Id != id))
                    return BadRequest(new { error = $"Department name '{dto.Name}' already exists" });

                // Validation: Manager must exist if provided
                if (dto.ManagerId.HasValue)
                {
                    var manager = await _employeeRepo.GetByIdAsync(dto.ManagerId.Value);
                    if (manager == null)
                        return BadRequest(new { error = "Manager not found" });
                }

                department.Name = dto.Name;
                department.Description = dto.Description;
                department.ManagerId = dto.ManagerId;
                department.IsActive = dto.IsActive;
                department.ModifiedAt = DateTime.UtcNow;

                await _departmentRepo.UpdateAsync(department);
                return Ok(MapToDto(department));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete department
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var department = await _departmentRepo.GetByIdAsync(id);
                if (department == null)
                    return NotFound(new { error = "Department not found" });

                // Validation: Cannot delete department with active employees
                var employeeCount = await _departmentRepo.GetEmployeeCountAsync(id);
                if (employeeCount > 0)
                    return BadRequest(new { error = $"Cannot delete department with {employeeCount} employees. Please reassign them first." });

                await _departmentRepo.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

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
