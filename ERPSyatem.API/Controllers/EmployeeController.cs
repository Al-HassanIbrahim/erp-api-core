using ERPSystem.Application.DTOs.HR.Employee;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        /// <summary>
        /// Get all employees
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EmployeeListDto>), 200)]
        public async Task<ActionResult<IEnumerable<EmployeeListDto>>> GetAll()
        {
            var employees = await _employeeService.GetAllAsync();
            return Ok(employees);
        }

        /// <summary>
        /// Get employee by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EmployeeDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<EmployeeDetailDto>> GetById(Guid id)
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null)
                return NotFound(new { error = "Employee not found" });

            return Ok(employee);
        }

        /// <summary>
        /// Get employees by department
        /// </summary>
        [HttpGet("department/{departmentId}")]
        [ProducesResponseType(typeof(IEnumerable<EmployeeListDto>), 200)]
        public async Task<ActionResult<IEnumerable<EmployeeListDto>>> GetByDepartment(Guid departmentId)
        {
            var employees = await _employeeService.GetByDepartmentAsync(departmentId);
            return Ok(employees);
        }

        /// <summary>
        /// Get employees by status (1=Active, 2=Inactive, 3=OnLeave, 4=Terminated)
        /// </summary>
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(IEnumerable<EmployeeListDto>), 200)]
        public async Task<ActionResult<IEnumerable<EmployeeListDto>>> GetByStatus(EmployeeStatus status)
        {
            var employees = await _employeeService.GetByStatusAsync(status);
            return Ok(employees);
        }

        /// <summary>
        /// Create new employee
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(EmployeeDetailDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<EmployeeDetailDto>> Create([FromBody] CreateEmployeeDto dto)
        {
            try
            {
                var createdBy = User.Identity?.Name ?? "System";
                var employee = await _employeeService.CreateAsync(dto, createdBy);
                return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update employee
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EmployeeDetailDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<EmployeeDetailDto>> Update(
            Guid id, [FromBody] UpdateEmployeeDto dto)
        {
            try
            {
                var modifiedBy = User.Identity?.Name ?? "System";
                var employee = await _employeeService.UpdateAsync(id, dto, modifiedBy);
                return Ok(employee);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update employee status
        /// </summary>
        [HttpPut("{id}/status")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> UpdateStatus(
            Guid id, [FromBody] UpdateEmployeeDto dto)
        {
            try
            {
                var modifiedBy = User.Identity?.Name ?? "System";
                await _employeeService.UpdateStatusAsync(id, dto, modifiedBy);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete employee
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _employeeService.DeleteAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
