using ERPSystem.Application.Authorization;
using ERPSystem.Application.DTOs.Sales;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    /// <summary>
    /// Manages sales invoices lifecycle including creation, modification,
    /// posting (confirmation), cancellation, and deletion.
    /// </summary>
    [ApiController]
    [Route("api/sales/invoices")]
    [Authorize]
    public class SalesInvoicesController : ControllerBase
    {
        private readonly ISalesInvoiceService _service;

        public SalesInvoicesController(ISalesInvoiceService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves a list of sales invoices for the current company.
        /// Supports filtering by customer, invoice status, and date range.
        /// </summary>
        [HttpGet]
        [Authorize(Policy = Permissions.Sales.Invoices.Read)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? customerId,
            [FromQuery] SalesInvoiceStatus? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetAllAsync(customerId, status, fromDate, toDate, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific sales invoice by its identifier,
        /// including header and invoice lines.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.Sales.Invoices.Read)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Creates a new sales invoice in Draft status.
        /// The invoice can be modified until it is posted.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = Permissions.Sales.Invoices.Create)]
        public async Task<IActionResult> Create([FromBody] CreateSalesInvoiceRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Updates an existing sales invoice.
        /// Only invoices in Draft status can be modified.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.Sales.Invoices.Update)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSalesInvoiceRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.UpdateAsync(id, request, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Posts (confirms) a sales invoice.
        /// Once posted, the invoice becomes immutable and financially effective.
        /// </summary>
        [HttpPost("{id}/post")]
        [Authorize(Policy = Permissions.Sales.Invoices.Post)]
        public async Task<IActionResult> Post(int id, CancellationToken cancellationToken)
        {
            var result = await _service.PostAsync(id, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Cancels a sales invoice.
        /// </summary>
        /// <remarks>
        /// Cancellation represents a business decision, not a data removal.
        /// The invoice remains in the system for audit and reporting purposes,
        /// but its status is changed to <c>Cancelled</c>.
        ///
        /// Rules:
        /// - Only invoices that belong to the current company can be cancelled.
        /// - An invoice cannot be cancelled if it has related deliveries or financial transactions.
        /// - Cancellation is irreversible.
        /// </remarks>

        [HttpPost("{id}/cancel")]
        [Authorize(Policy = "sales.invoices.cancel")]
        public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
        {
            var result = await _service.CancelAsync(id, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Soft-deletes a sales invoice.
        /// </summary>
        /// <remarks>
        /// Deletion represents a technical or user input mistake.
        /// The invoice is not physically removed from the database,
        /// but is marked as deleted and excluded from future queries.
        ///
        /// Rules:
        /// - Only draft invoices can be deleted.
        /// - Deleted invoices must not have any business impact.
        /// - Deletion should be used only when the invoice was created by mistake.
        /// </remarks>
        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.Sales.Invoices.Delete)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}