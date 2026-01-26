using ERPSystem.Application.DTOs.Sales;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    /// <summary>
    /// Handles customer receipts including creation, posting,
    /// cancellation, and allocation to sales invoices.
    /// </summary>
    [ApiController]
    [Route("api/sales/receipts")]
    [Authorize]
    public class SalesReceiptsController : ControllerBase
    {
        private readonly ISalesReceiptService _service;

        public SalesReceiptsController(ISalesReceiptService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves sales receipts for the current company.
        /// Supports filtering by customer, receipt status, and date range.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? customerId,
            [FromQuery] SalesReceiptStatus? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetAllAsync(customerId, status, fromDate, toDate, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific sales receipt by its identifier,
        /// including allocation details if applicable.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Creates a new sales receipt for a customer.
        /// The receipt represents a payment received.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSalesReceiptRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Posts (confirms) a sales receipt.
        /// Posting finalizes the receipt and applies allocations to invoices.
        /// </summary>
        [HttpPost("{id}/post")]
        public async Task<IActionResult> Post(int id, CancellationToken cancellationToken)
        {
            var result = await _service.PostAsync(id, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Cancels a sales receipt.
        /// </summary>
        /// <remarks>
        /// Business rule:
        /// - Draft receipts have no financial impact and can be cancelled directly.
        /// - Posted receipts have already affected invoices;
        ///   cancelling a posted receipt will reverse all allocations
        ///   and restore invoice balances and payment statuses.
        /// - Posted receipts are not deleted, only reversed.
        /// </remarks>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
        {
            var result = await _service.CancelAsync(id, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a sales receipt.
        /// </summary>
        /// <remarks>
        /// Business rule:
        /// - Only Draft receipts can be deleted.
        /// - Posted or Cancelled receipts must never be deleted
        ///   to preserve accounting and audit integrity.
        /// </remarks>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}