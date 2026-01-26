using ERPSystem.Application.DTOs.HR.Leave;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<ActionResult<LeaveRequestDto>> Create([FromBody] CreateLeaveRequestDto dto)
        {
            try
            {
                var leave = await _leaveService.CreateAsync(dto);
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
        public async Task<ActionResult<LeaveRequestDetailDto>> GetById(Guid id)
        {
            var leave = await _leaveService.GetByIdAsync(id);
            if (leave == null)
                return NotFound(new { error = "Leave request not found" });

            return Ok(leave);
        }

        /// <summary>
        /// Get employee leave requests
        /// </summary>
        [HttpGet("employee/{employeeId}")]
        [ProducesResponseType(typeof(IEnumerable<LeaveRequestDto>), 200)]
        public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetByEmployee(Guid employeeId)
        {
            try
            {
                var leaves = await _leaveService.GetByEmployeeIdAsync(employeeId);
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
        public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetPending()
        {
            var leaves = await _leaveService.GetPendingAsync();
            return Ok(leaves);
        }

        /// <summary>
        /// Approve leave request
        /// </summary>
        [HttpPost("{id}/approve")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Approve(Guid id, [FromBody] ApproveLeaveDto dto)
        {
            try
            {
                var approvedBy = User.Identity?.Name ?? "System";
                await _leaveService.ApproveAsync(id, dto, approvedBy);
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
        public async Task<ActionResult> Reject(Guid id, [FromBody] RejectLeaveDto dto)
        {
            try
            {
                var rejectedBy = User.Identity?.Name ?? "System";
                await _leaveService.RejectAsync(id, dto, rejectedBy);
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
        public async Task<ActionResult> Cancel(Guid id, [FromBody] CancelLeaveDto dto)
        {
            try
            {
                var cancelledBy = User.Identity?.Name ?? "System";
                await _leaveService.CancelAsync(id, dto.Reason, cancelledBy);
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
        public async Task<ActionResult<LeaveBalanceDto>> GetBalance(Guid employeeId, int year)
        {
            try
            {
                var balance = await _leaveService.GetBalanceAsync(employeeId, year);
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
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _leaveService.DeleteAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
