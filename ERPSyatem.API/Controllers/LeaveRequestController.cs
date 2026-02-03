using ERPSystem.Application.DTOs.HR.Leave;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeaveRequestController : ControllerBase
    {
        private readonly ILeaveRequestService _leaveService;

        public LeaveRequestController(ILeaveRequestService leaveService)
        {
            _leaveService = leaveService;
        }

        /// <summary>
        /// Create leave request
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(LeaveRequestDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<LeaveRequestDto>> Create([FromBody] CreateLeaveRequestDto dto,CancellationToken ct)
        {
            try
            {
                var leave = await _leaveService.CreateAsync(dto,ct);
                return Ok(leave);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get leave request by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LeaveRequestDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<LeaveRequestDetailDto>> GetById(Guid id,CancellationToken ct)
        {
            var leave = await _leaveService.GetByIdAsync(id,ct);
            if (leave == null)
                return NotFound(new { error = "Leave request not found" });

            return Ok(leave);
        }

        /// <summary>
        /// Get employee leave requests
        /// </summary>
        [HttpGet("employee/{employeeId}")]
        [ProducesResponseType(typeof(IEnumerable<LeaveRequestDto>), 200)]
        public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetByEmployee(Guid employeeId,CancellationToken ct)
        {
            try
            {
                var leaves = await _leaveService.GetByEmployeeIdAsync(employeeId,ct);
                return Ok(leaves);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get pending leave requests
        /// </summary>
        [HttpGet("pending")]
        [ProducesResponseType(typeof(IEnumerable<LeaveRequestDto>), 200)]
        public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetPending(CancellationToken ct)
        {
            var leaves = await _leaveService.GetPendingAsync(ct);
            return Ok(leaves);
        }

        /// <summary>
        /// Approve leave request
        /// </summary>
        [HttpPost("{id}/approve")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Approve(Guid id, [FromBody] ApproveLeaveDto dto, CancellationToken ct)
        {
            try
            {
                var approvedBy = User.Identity?.Name ?? "System";
                await _leaveService.ApproveAsync(id, dto, approvedBy,ct);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Reject leave request
        /// </summary>
        [HttpPost("{id}/reject")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Reject(Guid id, [FromBody] RejectLeaveDto dto, CancellationToken ct)
        {
            try
            {
                var rejectedBy = User.Identity?.Name ?? "System";
                await _leaveService.RejectAsync(id, dto, rejectedBy, ct);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cancel leave request
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Cancel(Guid id, [FromBody] CancelLeaveDto dto, CancellationToken ct)
        {
            try
            {
                var cancelledBy = User.Identity?.Name ?? "System";
                await _leaveService.CancelAsync(id, dto.Reason, cancelledBy, ct);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get employee leave balance
        /// </summary>
        [HttpGet("balance/{employeeId}/{year}")]
        [ProducesResponseType(typeof(LeaveBalanceDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<LeaveBalanceDto>> GetBalance(Guid employeeId, int year, CancellationToken ct)
        {
            try
            {
                var balance = await _leaveService.GetBalanceAsync(employeeId, year,ct);
                return Ok(balance);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete leave request
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                await _leaveService.DeleteAsync(id,ct);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
