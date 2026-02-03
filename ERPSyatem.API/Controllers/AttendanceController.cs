using ERPSystem.Application.DTOs.HR.Attendance;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static ERPSystem.Application.DTOs.HR.Attendance.Check;

namespace ERPSyatem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        /// <summary>
        /// Check in
        /// </summary>
        [HttpPost("checkin")]
        [ProducesResponseType(typeof(AttendanceDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AttendanceDto>> CheckIn([FromBody] CheckInDto dto, CancellationToken ct)
        {
            try
            {
                var createdBy = User.Identity?.Name ?? "System";
                var attendance = await _attendanceService.CheckInAsync(dto, createdBy,ct);
                return Ok(attendance);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Check out
        /// </summary>
        [HttpPost("checkout")]
        [ProducesResponseType(typeof(AttendanceDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AttendanceDto>> CheckOut([FromBody] CheckOutDto dto, CancellationToken ct)
        {
            try
            {
                var modifiedBy = User.Identity?.Name ?? "System";
                var attendance = await _attendanceService.CheckOutAsync(dto, modifiedBy,ct);
                return Ok(attendance);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create manual attendance entry
        /// </summary>
        [HttpPost("manual")]
        [ProducesResponseType(typeof(AttendanceDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AttendanceDto>> CreateManual([FromBody] ManualAttendanceDto dto, CancellationToken ct)
        {
            try
            {
                var createdBy = User.Identity?.Name ?? "System";
                var attendance = await _attendanceService.CreateManualEntryAsync(dto, createdBy, ct);
                return Ok(attendance);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update attendance
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(AttendanceDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AttendanceDto>> Update(
            Guid id, [FromBody] UpdateAttendanceDto dto, CancellationToken ct)
        {
            try
            {
                var modifiedBy = User.Identity?.Name ?? "System";
                var attendance = await _attendanceService.UpdateAsync(id, dto, modifiedBy,ct);
                return Ok(attendance);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get monthly attendance summary
        /// </summary>
        [HttpGet("summary/{employeeId}/{month}/{year}")]
        [ProducesResponseType(typeof(AttendanceSummaryDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AttendanceSummaryDto>> GetSummary(
            Guid employeeId, int month, int year, CancellationToken ct)
        {
            try
            {
                var summary = await _attendanceService.GetSummaryAsync(employeeId, month, year, ct);
                return Ok(summary);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
