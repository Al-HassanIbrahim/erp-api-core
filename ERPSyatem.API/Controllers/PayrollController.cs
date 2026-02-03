using ERPSystem.Application.DTOs.HR.Payroll;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _payrollService;

        public PayrollController(IPayrollService payrollService)
        {
            _payrollService = payrollService;
        }

        /// <summary>
        /// Generate payroll for specified employees/departments
        /// </summary>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(PayrollBatchDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PayrollBatchDto>> Generate([FromBody] GeneratePayrollDto dto,CancellationToken ct)
        {
            try
            {
                var generatedBy = User.Identity?.Name ?? "System";
                var result = await _payrollService.GeneratePayrollAsync(dto, generatedBy,ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get payroll by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PayrollDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PayrollDetailDto>> GetById(Guid id, CancellationToken ct)
        {
            var payroll = await _payrollService.GetByIdAsync(id, ct);
            if (payroll == null)
                return NotFound(new { error = "Payroll not found" });

            return Ok(payroll);
        }

        /// <summary>
        /// Get employee payrolls
        /// </summary>
        [HttpGet("employee/{employeeId}")]
        [ProducesResponseType(typeof(IEnumerable<PayrollDto>), 200)]
        public async Task<ActionResult<IEnumerable<PayrollDto>>> GetByEmployee(Guid employeeId, CancellationToken ct)
        {
            var payrolls = await _payrollService.GetByEmployeeIdAsync(employeeId, ct);
            return Ok(payrolls);
        }

        /// <summary>
        /// Get payrolls by period
        /// </summary>
        [HttpGet("period/{month}/{year}")]
        [ProducesResponseType(typeof(IEnumerable<PayrollDto>), 200)]
        public async Task<ActionResult<IEnumerable<PayrollDto>>> GetByPeriod(int month, int year,CancellationToken ct)
        {
            var payrolls = await _payrollService.GetByPeriodAsync(month, year, ct);
            return Ok(payrolls);
        }

        /// <summary>
        /// Update payroll (only draft)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PayrollDetailDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PayrollDetailDto>> Update(Guid id, [FromBody] UpdatePayrollDto dto,CancellationToken ct)
        {
            try
            {
                var modifiedBy = User.Identity?.Name ?? "System";
                var payroll = await _payrollService.UpdateAsync(id, dto, modifiedBy,ct);
                return Ok(payroll);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Process payroll (Draft -> Processed)
        /// </summary>
        [HttpPost("{id}/process")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Process(Guid id, CancellationToken ct)
        {
            try
            {
                var processedBy = User.Identity?.Name ?? "System";
                await _payrollService.ProcessPayrollAsync(id, processedBy, ct);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Mark payroll as paid (Processed -> Paid)
        /// </summary>
        [HttpPost("{id}/mark-paid")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> MarkPaid(Guid id, [FromBody] MarkPaidDto dto,CancellationToken ct)
        {
            try
            {
                var paidBy = User.Identity?.Name ?? "System";
                await _payrollService.MarkAsPaidAsync(id, dto, paidBy,ct);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Revert payroll to draft (Processed -> Draft)
        /// </summary>
        [HttpPost("{id}/revert")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> RevertToDraft(Guid id, CancellationToken ct)
        {
            try
            {
                var modifiedBy = User.Identity?.Name ?? "System";
                await _payrollService.RevertToDraftAsync(id, modifiedBy, ct);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete payroll (only draft)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                await _payrollService.DeleteAsync(id,ct);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get payroll summary for a period
        /// </summary>
        [HttpGet("summary/{month}/{year}")]
        [ProducesResponseType(typeof(PayrollSummaryDto), 200)]
        public async Task<ActionResult<PayrollSummaryDto>> GetSummary(int month, int year,CancellationToken ct)
        {
            var summary = await _payrollService.GetPeriodSummaryAsync(month, year,ct);
            return Ok(summary);
        }

        /// <summary>
        /// Recalculate payroll (only draft)
        /// </summary>
        [HttpPost("{id}/recalculate")]
        [ProducesResponseType(typeof(PayrollDetailDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PayrollDetailDto>> Recalculate(Guid id,CancellationToken ct)
        {
            try
            {
                var modifiedBy = User.Identity?.Name ?? "System";
                var payroll = await _payrollService.RecalculateAsync(id, modifiedBy,ct);
                return Ok(payroll);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
