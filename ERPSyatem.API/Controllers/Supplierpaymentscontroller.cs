using ERPSystem.Application.DTOs.Purchase;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static ERPSystem.Application.Authorization.Permissions;

namespace ERPSyatem.API.Controllers
{
    /// <summary>
    /// Manages supplier payments and their allocation against purchase invoices.
    /// </summary>
    /// <remarks>
    /// State machine: Draft → Posted (via POST /{id}/post) → Cancelled (via POST /{id}/cancel).
    /// Posting applies payment allocations atomically, incrementing each invoice's PaidAmount
    /// and recalculating its PaymentStatus (Unpaid / PartiallyPaid / Paid).
    /// Cancellation fully reverses all allocation effects.
    /// </remarks>
    [ApiController]
    [Route("api/v1/purchase/payments")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public sealed class SupplierPaymentsController : ControllerBase
    {
        private readonly ISupplierPaymentService _service;
        private readonly ILogger<SupplierPaymentsController> _logger;

        public SupplierPaymentsController(
            ISupplierPaymentService service,
            ILogger<SupplierPaymentsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ──────────────────────────────────────────────────────────
        //  GET /api/v1/purchase/payments
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all supplier payments for the caller's company.
        /// </summary>
        /// <response code="200">List of payment summaries (may be empty).</response>
        [HttpGet]
        [Authorize(Policy = Purchasing.Read)]
        [ProducesResponseType(typeof(IReadOnlyList<SupplierPaymentListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<SupplierPaymentListDto>>> GetAll(
            CancellationToken ct)
        {
            var result = await _service.GetAllAsync(ct);
            return Ok(result);
        }

        // ──────────────────────────────────────────────────────────
        //  GET /api/v1/purchase/payments/{id}
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a supplier payment with its full allocation detail.
        /// </summary>
        /// <param name="id">Payment primary key.</param>
        /// <response code="200">Payment found.</response>
        /// <response code="404">Payment not found in this company.</response>
        [Authorize(Policy = Purchasing.Read)]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(SupplierPaymentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SupplierPaymentDto>> GetById(
            int id,
            CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return Ok(result);
        }

        // ──────────────────────────────────────────────────────────
        //  POST /api/v1/purchase/payments
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a new supplier payment in Draft status.
        /// </summary>
        /// <remarks>
        /// Allocation rules enforced at creation:
        /// <list type="bullet">
        ///   <item>Each allocation must reference an invoice belonging to the same supplier.</item>
        ///   <item>Total allocated amount must not exceed the payment <c>Amount</c>.</item>
        /// </list>
        /// The payment number is generated automatically from the document sequence.
        /// </remarks>
        /// <param name="request">Payment header and invoice allocations.</param>
        /// <response code="201">Payment created. Location header points to the new resource.</response>
        /// <response code="400">Validation failed (missing required fields, invalid amounts).</response>
        /// <response code="404">Referenced supplier or invoice not found.</response>
        /// <response code="422">Business rule violation (e.g. allocation exceeds payment amount, invoice supplier mismatch).</response>
        [HttpPost]
        [Authorize(Policy = Purchasing.Write)]
        [ProducesResponseType(typeof(SupplierPaymentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<SupplierPaymentDto>> Create(
            [FromBody] CreateSupplierPaymentDto request,
            CancellationToken ct)
        {
            var result = await _service.CreateAsync(request, ct);
            _logger.LogInformation(
                "Supplier payment {Id} ({Number}) created.",
                result.Id, result.PaymentNumber);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // ──────────────────────────────────────────────────────────
        //  POST /api/v1/purchase/payments/{id}/post
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Posts a Draft payment, applying all invoice allocations atomically.
        /// </summary>
        /// <remarks>
        /// For each allocation the service:
        /// <list type="bullet">
        ///   <item>Verifies the invoice is in <b>Posted</b> status.</item>
        ///   <item>Verifies the invoice belongs to the payment's supplier.</item>
        ///   <item>Verifies the allocated amount does not exceed the invoice's BalanceDue.</item>
        ///   <item>Increments invoice.PaidAmount and recalculates invoice.PaymentStatus.</item>
        /// </list>
        /// All mutations commit or roll back together.
        /// </remarks>
        /// <param name="id">Payment primary key.</param>
        /// <response code="204">Payment successfully posted.</response>
        /// <response code="404">Payment not found.</response>
        /// <response code="422">Payment is not in Draft status, has no allocations, or a business rule was violated.</response>
        [HttpPost("{id:int}/post")]
        [Authorize(Policy = Purchasing.Post)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Post(
            int id,
            CancellationToken ct)
        {
            await _service.PostAsync(id, ct);
            _logger.LogInformation("Supplier payment {Id} posted.", id);
            return NoContent();
        }

        // ──────────────────────────────────────────────────────────
        //  POST /api/v1/purchase/payments/{id}/cancel
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Cancels a Posted payment, fully reversing all invoice allocation effects.
        /// </summary>
        /// <remarks>
        /// For each allocation the service:
        /// <list type="bullet">
        ///   <item>Decrements invoice.PaidAmount by the allocated amount (floor: 0).</item>
        ///   <item>Recalculates invoice.PaymentStatus from the actual remaining balance.</item>
        /// </list>
        /// All mutations commit or roll back together.
        /// </remarks>
        /// <param name="id">Payment primary key.</param>
        /// <response code="204">Payment successfully cancelled and all allocations reversed.</response>
        /// <response code="404">Payment not found.</response>
        /// <response code="422">Payment is not in Posted status.</response>
        [HttpPost("{id:int}/cancel")]
        [Authorize(Policy = Purchasing.Write)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Cancel(
            int id,
            CancellationToken ct)
        {
            await _service.CancelAsync(id, ct);
            _logger.LogInformation("Supplier payment {Id} cancelled.", id);
            return NoContent();
        }
    }
}
