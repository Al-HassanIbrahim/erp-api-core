using ERPSystem.Application.DTOs.Sales;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    /// <summary>
    /// Handles sales deliveries which represent the physical
    /// dispatch of goods to customers.
    /// </summary>
    [ApiController]
    [Route("api/sales/deliveries")]
    [Authorize]
    public class SalesDeliveriesController : ControllerBase
    {
        private readonly ISalesDeliveryService _service;

        public SalesDeliveriesController(ISalesDeliveryService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves sales delivery documents for the current company.
        /// Supports filtering by invoice, status, and date range.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? invoiceId,
            [FromQuery] SalesDeliveryStatus? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetAllAsync(invoiceId, status, fromDate, toDate, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific sales delivery document by its identifier,
        /// including delivery lines.
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
        /// Creates a new sales delivery document.
        /// This represents a planned delivery of goods.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSalesDeliveryRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Posts (confirms) a sales delivery.
        /// If Inventory module is enabled, stock quantities
        /// are reduced from the specified warehouse.
        /// </summary>
        [HttpPost("{id}/post")]
        public async Task<IActionResult> Post(int id, CancellationToken cancellationToken)
        {
            var result = await _service.PostAsync(id, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Cancels a sales delivery document.
        /// </summary>
        /// <remarks>
        /// Cancellation is allowed only while the delivery is in <c>Draft</c> status.
        /// Posted deliveries cannot be cancelled because they may have created inventory impact.
        /// To reverse a posted delivery, create a sales return instead.
        /// </remarks>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
        {
            var result = await _service.CancelAsync(id, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Soft-deletes a sales delivery document.
        /// </summary>
        /// <remarks>
        /// Deletion is allowed only for <c>Draft</c> deliveries that have no inventory impact.
        /// The record is marked as deleted and excluded from future queries.
        /// </remarks>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}