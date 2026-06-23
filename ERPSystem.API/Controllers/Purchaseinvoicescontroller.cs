using ERPSystem.Application.Authorization;
using ERPSystem.Application.DTOs.Purchase;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static ERPSystem.Application.Authorization.Permissions;

namespace ERPSystem.API.Controllers
{
    /// <summary>
    /// Manages purchase invoices: draft creation, editing, posting, and deletion.
    /// </summary>
    /// <remarks>
    /// State machine: Draft → Posted (via POST /{id}/post).
    /// Only Draft invoices can be edited or deleted.
    /// Posting triggers an atomic stock-in movement via the Inventory module.
    /// </remarks>
    [ApiController]
    [Route("api/v1/purchase/invoices")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public sealed class PurchaseInvoicesController : ControllerBase
    {
        private readonly IPurchaseInvoiceService _service;
        private readonly ILogger<PurchaseInvoicesController> _logger;

        public PurchaseInvoicesController(
            IPurchaseInvoiceService service,
            ILogger<PurchaseInvoicesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ──────────────────────────────────────────────────────────
        //  GET /api/v1/purchase/invoices
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all purchase invoices for the caller's company.
        /// </summary>
        /// <response code="200">List of invoice summaries (may be empty).</response>
        [HttpGet]
        [Authorize(Policy = Purchasing.Purchases.Read)]
        [ProducesResponseType(typeof(IReadOnlyList<PurchaseInvoiceListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<PurchaseInvoiceListDto>>> GetAll(
            CancellationToken ct)
        {
            var result = await _service.GetAllAsync(ct);
            return Ok(result);
        }

        // ──────────────────────────────────────────────────────────
        //  GET /api/v1/purchase/invoices/{id}
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a purchase invoice with its full line detail.
        /// </summary>
        /// <param name="id">Invoice primary key.</param>
        /// <response code="200">Invoice found.</response>
        /// <response code="404">Invoice not found in this company.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PurchaseInvoiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [Authorize(Policy = Purchasing.Purchases.Read)]
        public async Task<ActionResult<PurchaseInvoiceDto>> GetById(
            int id,
            CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return Ok(result);
        }

        // ──────────────────────────────────────────────────────────
        //  POST /api/v1/purchase/invoices
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a new purchase invoice in Draft status.
        /// The invoice number is generated automatically from the document sequence.
        /// </summary>
        /// <param name="request">Invoice header and line items.</param>
        /// <response code="201">Invoice created. Location header points to the new resource.</response>
        /// <response code="400">Validation failed (missing required fields, invalid line values).</response>
        /// <response code="404">Referenced supplier or warehouse not found.</response>
        /// <response code="422">Business rule violation (e.g. supplier inactive).</response>
        [HttpPost]
        [Authorize(Policy = Purchasing.Purchases.Write)]
        [ProducesResponseType(typeof(PurchaseInvoiceDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<PurchaseInvoiceDto>> Create(
            [FromBody] CreatePurchaseInvoiceDto request,
            CancellationToken ct)
        {
            var result = await _service.CreateAsync(request, ct);
            _logger.LogInformation(
                "Purchase invoice {Id} ({Number}) created.",
                result.Id, result.InvoiceNumber);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // ──────────────────────────────────────────────────────────
        //  PUT /api/v1/purchase/invoices/{id}
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Fully replaces the header and lines of a Draft invoice.
        /// Lines are replaced (not merged) — send the complete desired line set.
        /// </summary>
        /// <param name="id">Invoice primary key.</param>
        /// <param name="request">Replacement header and line items.</param>
        /// <response code="200">Invoice updated, returns the refreshed resource.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="404">Invoice, supplier, or warehouse not found.</response>
        /// <response code="422">Invoice is not in Draft status.</response>
        [HttpPut("{id:int}")]
        [Authorize(Policy = Purchasing.Purchases.Write)]
        [ProducesResponseType(typeof(PurchaseInvoiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<PurchaseInvoiceDto>> Update(
            int id,
            [FromBody] UpdatePurchaseInvoiceDto request,
            CancellationToken ct)
        {
            var result = await _service.UpdateAsync(id, request, ct);
            return Ok(result);
        }

        // ──────────────────────────────────────────────────────────
        //  POST /api/v1/purchase/invoices/{id}/post
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Posts a Draft invoice, moving it to Posted status.
        /// </summary>
        /// <remarks>
        /// Posting is an atomic, transactional operation:
        /// <list type="bullet">
        ///   <item>Validates supplier and warehouse are still active.</item>
        ///   <item>Validates all stored line values (qty &gt; 0, conversion rate &gt; 0).</item>
        ///   <item>Calls InventoryService.StockInAsync to record stock movement.</item>
        ///   <item>Updates invoice status and stores the resulting InventoryDocumentId.</item>
        ///   <item>All mutations commit or roll back together.</item>
        /// </list>
        /// </remarks>
        /// <param name="id">Invoice primary key.</param>
        /// <response code="204">Invoice successfully posted.</response>
        /// <response code="404">Invoice not found.</response>
        /// <response code="422">Invoice is not in Draft status, has no lines, or a business rule was violated.</response>
        [HttpPost("{id:int}/post")]
        [Authorize(Policy = Purchasing.Purchases.Post)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Post(
            int id,
            CancellationToken ct)
        {
            await _service.PostAsync(id, ct);
            _logger.LogInformation("Purchase invoice {Id} posted.", id);
            return NoContent();
        }

        // ──────────────────────────────────────────────────────────
        //  DELETE /api/v1/purchase/invoices/{id}
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Soft-deletes a Draft invoice.
        /// Posted invoices cannot be deleted.
        /// </summary>
        /// <param name="id">Invoice primary key.</param>
        /// <response code="204">Invoice deleted.</response>
        /// <response code="404">Invoice not found.</response>
        /// <response code="422">Invoice is not in Draft status.</response>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = Purchasing.Purchases.Write)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}