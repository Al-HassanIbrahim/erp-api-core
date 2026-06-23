using ERPSystem.Application.DTOs.Purchase;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static ERPSystem.Application.Authorization.Permissions;

namespace ERPSystem.API.Controllers
{
    /// <summary>
    /// Manages purchase returns: draft creation, posting, and deletion.
    /// </summary>
    /// <remarks>
    /// State machine: Draft → Posted (via POST /{id}/post).
    /// A return may optionally reference a Posted purchase invoice.
    /// When a linked invoice is provided, return quantities are validated
    /// against purchased quantities minus any previously posted returns.
    /// Posting triggers an atomic stock-out via the Inventory module
    /// (MAC cost is resolved automatically by InventoryService — no cost input needed).
    /// </remarks>
    [ApiController]
    [Route("api/v1/purchase/returns")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public sealed class PurchaseReturnsController : ControllerBase
    {
        private readonly IPurchaseReturnService _p_service;
        private readonly ILogger<PurchaseReturnsController> _logger;

        public PurchaseReturnsController(
            IPurchaseReturnService service,
            ILogger<PurchaseReturnsController> logger)
        {
            _p_service = service;
            _logger = logger;
        }

        // ──────────────────────────────────────────────────────────
        //  GET /api/v1/purchase/returns
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all purchase returns for the caller's company.
        /// </summary>
        /// <response code="200">List of return summaries (may be empty).</response>
        [HttpGet]
        [Authorize(Policy = Purchasing.Purchases.Read)]
        [ProducesResponseType(typeof(IReadOnlyList<PurchaseReturnListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<PurchaseReturnListDto>>> GetAll(
            CancellationToken ct)
        {
            var result = await _p_service.GetAllAsync(ct);
            return Ok(result);
        }

        // ──────────────────────────────────────────────────────────
        //  GET /api/v1/purchase/returns/{id}
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a purchase return with its full line detail.
        /// </summary>
        /// <param name="id">Return primary key.</param>
        /// <response code="200">Return found.</response>
        /// <response code="404">Return not found in this company.</response>
        [HttpGet("{id:int}")]
        [Authorize(Policy = Purchasing.Purchases.Read)]
        [ProducesResponseType(typeof(PurchaseReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PurchaseReturnDto>> GetById(
            int id,
            CancellationToken ct)
        {
            var result = await _p_service.GetByIdAsync(id, ct);
            return Ok(result);
        }

        // ──────────────────────────────────────────────────────────
        //  POST /api/v1/purchase/returns
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a new purchase return in Draft status.
        /// </summary>
        /// <remarks>
        /// When <c>PurchaseInvoiceId</c> is supplied:
        /// <list type="bullet">
        ///   <item>The referenced invoice must be in <b>Posted</b> status.</item>
        ///   <item>Return quantities are validated against available (purchased − already returned) quantities.</item>
        /// </list>
        /// The return number is generated automatically from the document sequence.
        /// </remarks>
        /// <param name="request">Return header and line items.</param>
        /// <response code="201">Return created. Location header points to the new resource.</response>
        /// <response code="400">Validation failed (missing required fields, invalid line values).</response>
        /// <response code="404">Referenced supplier, warehouse, or invoice not found.</response>
        /// <response code="422">Business rule violation (e.g. supplier inactive, quantity over-return).</response>
        [HttpPost]
        [Authorize(Policy = Purchasing.Purchases.Write)]
        [ProducesResponseType(typeof(PurchaseReturnDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<PurchaseReturnDto>> Create(
            [FromBody] CreatePurchaseReturnDto request,
            CancellationToken ct)
        {
            var result = await _p_service.CreateAsync(request, ct);
            _logger.LogInformation(
                "Purchase return {Id} ({Number}) created.",
                result.Id, result.ReturnNumber);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // ──────────────────────────────────────────────────────────
        //  POST /api/v1/purchase/returns/{id}/post
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Posts a Draft return, moving it to Posted status.
        /// </summary>
        /// <remarks>
        /// Posting is an atomic, transactional operation:
        /// <list type="bullet">
        ///   <item>Validates supplier and warehouse are still active.</item>
        ///   <item>Re-validates over-return quantities against the linked invoice (if any).</item>
        ///   <item>Calls InventoryService.StockOutAsync — MAC cost is auto-resolved.</item>
        ///   <item>Updates return status and stores the resulting InventoryDocumentId.</item>
        ///   <item>All mutations commit or roll back together.</item>
        /// </list>
        /// </remarks>
        /// <param name="id">Return primary key.</param>
        /// <response code="204">Return successfully posted.</response>
        /// <response code="404">Return not found.</response>
        /// <response code="422">Return is not in Draft status, has no lines, or a business rule was violated.</response>
        [HttpPost("{id:int}/post")]
        [Authorize(Policy = Purchasing.Purchases.Post)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Post(
            int id,
            CancellationToken ct)
        {
            await _p_service.PostAsync(id, ct);
            _logger.LogInformation("Purchase return {Id} posted.", id);
            return NoContent();
        }

        // ──────────────────────────────────────────────────────────
        //  DELETE /api/v1/purchase/returns/{id}
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Soft-deletes a Draft return.
        /// Posted returns cannot be deleted.
        /// </summary>
        /// <param name="id">Return primary key.</param>
        /// <response code="204">Return deleted.</response>
        /// <response code="404">Return not found.</response>
        /// <response code="422">Return is not in Draft status.</response>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = Purchasing.Purchases.Write)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken ct)
        {
            await _p_service.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}