using ERPSystem.Application.DTOs.HR.JobPosition;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.HR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobPositionController : ControllerBase
    {
        private readonly IPositionRepository _positionRepo;
        private readonly IDepartmentRepository _departmentRepo;
        private readonly ICurrentUserService _currentUser;

        public JobPositionController(
            IPositionRepository positionRepo,
            IDepartmentRepository departmentRepo,
            ICurrentUserService currentUser)
        {
            _positionRepo = positionRepo;
            _departmentRepo = departmentRepo;
            _currentUser = currentUser;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PositionDto>), 200)]
        public async Task<ActionResult<IEnumerable<PositionDto>>> GetAll(CancellationToken ct)
        {
            var positions = await _positionRepo.GetAllAsync(_currentUser.CompanyId, ct);
            return Ok(positions.Select(MapToDto));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PositionDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PositionDto>> GetById(Guid id, CancellationToken ct)
        {
            var position = await _positionRepo.GetByIdAsync(id, _currentUser.CompanyId, ct);
            if (position == null)
                return NotFound(new { error = "Position not found" });

            return Ok(MapToDto(position));
        }

        [HttpPost]
        [ProducesResponseType(typeof(PositionDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PositionDto>> Create([FromBody] CreatePositionDto dto, CancellationToken ct)
        {
            try
            {
                if (await _positionRepo.ExistsByCodeAsync(dto.Code, _currentUser.CompanyId, ct))
                    return BadRequest(new { error = $"Position code '{dto.Code}' already exists" });

                if (dto.MaxSalary <= dto.MinSalary)
                    return BadRequest(new { error = "Maximum salary must be greater than minimum salary" });

                if (dto.DepartmentId.HasValue)
                {
                    var department = await _departmentRepo.GetByIdAsync(dto.DepartmentId.Value, _currentUser.CompanyId, ct);
                    if (department == null)
                        return BadRequest(new { error = "Department not found or does not belong to your company." });
                }

                var position = new JobPosition
                {
                    Id = Guid.NewGuid(),
                    CompanyId = _currentUser.CompanyId,
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

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PositionDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PositionDto>> Update(Guid id, [FromBody] UpdatePositionDto dto, CancellationToken ct)
        {
            try
            {
                var position = await _positionRepo.GetByIdAsync(id, _currentUser.CompanyId, ct);
                if (position == null)
                    return NotFound(new { error = "Position not found" });

                if (dto.MaxSalary <= dto.MinSalary)
                    return BadRequest(new { error = "Maximum salary must be greater than minimum salary" });

                if (dto.DepartmentId.HasValue)
                {
                    var department = await _departmentRepo.GetByIdAsync(dto.DepartmentId.Value, _currentUser.CompanyId, ct);
                    if (department == null)
                        return BadRequest(new { error = "Department not found or does not belong to your company." });
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

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                // اختيارياً: تتأكد إنه موجود في نفس الشركة قبل الحذف
                var position = await _positionRepo.GetByIdAsync(id, _currentUser.CompanyId, ct);
                if (position == null)
                    return NotFound(new { error = "Position not found" });

                await _positionRepo.DeleteAsync(id, _currentUser.CompanyId, ct);
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
