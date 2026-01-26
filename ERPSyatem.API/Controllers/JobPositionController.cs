using ERPSystem.Application.DTOs.HR.JobPosition;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.HR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobPositionController : ControllerBase
    {
        private readonly IPositionRepository _positionRepo;
        private readonly IDepartmentRepository _departmentRepo;

        public JobPositionController(
            IPositionRepository positionRepo,
            IDepartmentRepository departmentRepo)
        {
            _positionRepo = positionRepo;
            _departmentRepo = departmentRepo;
        }

        /// <summary>
        /// Get all positions
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PositionDto>), 200)]
        public async Task<ActionResult<IEnumerable<PositionDto>>> GetAll()
        {
            var positions = await _positionRepo.GetAllAsync();
            return Ok(positions.Select(MapToDto));
        }

        /// <summary>
        /// Get position by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PositionDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PositionDto>> GetById(Guid id)
        {
            var position = await _positionRepo.GetByIdAsync(id);
            if (position == null)
                return NotFound(new { error = "Position not found" });

            return Ok(MapToDto(position));
        }

        /// <summary>
        /// Create new position
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PositionDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PositionDto>> Create([FromBody] CreatePositionDto dto)
        {
            try
            {
                // Validation: Code must be unique
                if (await _positionRepo.ExistsByCodeAsync(dto.Code))
                    return BadRequest(new { error = $"Position code '{dto.Code}' already exists" });

                // Validation: MaxSalary must be greater than MinSalary
                if (dto.MaxSalary <= dto.MinSalary)
                    return BadRequest(new { error = "Maximum salary must be greater than minimum salary" });

                // Validation: Department must exist if provided
                if (dto.DepartmentId.HasValue)
                {
                    var department = await _departmentRepo.GetByIdAsync(dto.DepartmentId.Value);
                    if (department == null)
                        return BadRequest(new { error = "Department not found" });
                }

                var position = new JobPosition
                {
                    Id = Guid.NewGuid(),
                    Code = dto.Code,
                    Title = dto.Title,
                    Description = dto.Description,
                    Level = dto.Level,
                    DepartmentId = dto.DepartmentId,
                    MinSalary = dto.MinSalary,
                    MaxSalary = dto.MaxSalary,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                await _positionRepo.AddAsync(position);
                return CreatedAtAction(nameof(GetById), new { id = position.Id }, MapToDto(position));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update position
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PositionDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PositionDto>> Update(Guid id, [FromBody] UpdatePositionDto dto)
        {
            try
            {
                var position = await _positionRepo.GetByIdAsync(id);
                if (position == null)
                    return NotFound(new { error = "Position not found" });

                // Validation: MaxSalary must be greater than MinSalary
                if (dto.MaxSalary <= dto.MinSalary)
                    return BadRequest(new { error = "Maximum salary must be greater than minimum salary" });

                // Validation: Department must exist if provided
                if (dto.DepartmentId.HasValue)
                {
                    var department = await _departmentRepo.GetByIdAsync(dto.DepartmentId.Value);
                    if (department == null)
                        return BadRequest(new { error = "Department not found" });
                }

                position.Title = dto.Title;
                position.Description = dto.Description;
                position.Level = dto.Level;
                position.DepartmentId = dto.DepartmentId;
                position.MinSalary = dto.MinSalary;
                position.MaxSalary = dto.MaxSalary;
                position.IsActive = dto.IsActive;
                position.ModifiedAt = DateTime.UtcNow;

                await _positionRepo.UpdateAsync(position);
                return Ok(MapToDto(position));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete position
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _positionRepo.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private PositionDto MapToDto(JobPosition p) => new()
        {
            Id = p.Id,
            Code = p.Code,
            Title = p.Title,
            Description = p.Description,
            Level = p.Level.ToString(),
            DepartmentId = p.DepartmentId,
            DepartmentName = p.Department?.Name,
            MinSalary = p.MinSalary,
            MaxSalary = p.MaxSalary,
            IsActive = p.IsActive
        };
    }

}
