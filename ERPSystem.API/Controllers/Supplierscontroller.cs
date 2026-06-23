using ERPSystem.Application.DTOs.Purchase;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static ERPSystem.Application.Authorization.Permissions;

namespace ERPSystem.API.Controllers
{
    /// <summary>
    /// Manages supplier master data for the purchasing module.
    /// </summary>
    [ApiController]
    [Route("api/v1/purchase/suppliers")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public sealed class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _service;
        private readonly ILogger<SuppliersController> _logger;

        public SuppliersController(
            ISupplierService service,
            ILogger<SuppliersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ──────────────────────────────────────────────────────────
        //  GET /api/v1/purchase/suppliers
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all suppliers that belong to the caller's company.
        /// </summary>
        /// <response code="200">List of suppliers (may be empty).</response>
        [HttpGet]
        [Authorize(Policy = Purchasing.suppliers.Read)]
        [ProducesResponseType(typeof(IReadOnlyList<SupplierListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<SupplierListDto>>> GetAll(
            CancellationToken ct)
        {
            var result = await _service.GetAllAsync(ct);
            return Ok(result);
        }

        // ──────────────────────────────────────────────────────────
        //  GET /api/v1/purchase/suppliers/{id}
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a single supplier by its identifier.
        /// </summary>
        /// <param name="id">Supplier primary key.</param>
        /// <response code="200">Supplier found.</response>
        /// <response code="404">Supplier not found in this company.</response>
        [HttpGet("{id:int}")]
        [Authorize(Policy = Purchasing.suppliers.Read)]
        [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SupplierDto>> GetById(
            int id,
            CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return Ok(result);
        }

        // ──────────────────────────────────────────────────────────
        //  POST /api/v1/purchase/suppliers
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a new supplier.
        /// </summary>
        /// <param name="request">Supplier creation payload.</param>
        /// <response code="201">Supplier created. Location header points to the new resource.</response>
        /// <response code="400">Validation failed (missing required fields, out-of-range values).</response>
        /// <response code="409">A supplier with the same code already exists.</response>
        [HttpPost]
        [Authorize(Policy = Purchasing.suppliers.Write)]
        [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<SupplierDto>> Create(
            [FromBody] CreateSupplierDto request,
            CancellationToken ct)
        {
            var result = await _service.CreateAsync(request, ct);
            _logger.LogInformation("Supplier {Id} ({Code}) created.", result.Id, result.Code);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // ──────────────────────────────────────────────────────────
        //  PUT /api/v1/purchase/suppliers/{id}
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Fully replaces a supplier's editable fields.
        /// </summary>
        /// <param name="id">Supplier primary key.</param>
        /// <param name="request">Updated supplier data.</param>
        /// <response code="200">Supplier updated, returns the refreshed resource.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="404">Supplier not found.</response>
        /// <response code="409">Duplicate supplier code conflict.</response>
        [HttpPut("{id:int}")]
        [Authorize(Policy = Purchasing.suppliers.Write)]
        [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<SupplierDto>> Update(
            int id,
            [FromBody] UpdateSupplierDto request,
            CancellationToken ct)
        {
            var result = await _service.UpdateAsync(id, request, ct);
            return Ok(result);
        }

        // ──────────────────────────────────────────────────────────
        //  DELETE /api/v1/purchase/suppliers/{id}
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Soft-deletes a supplier.
        /// Will be rejected if the supplier has any posted purchasing documents.
        /// </summary>
        /// <param name="id">Supplier primary key.</param>
        /// <response code="204">Supplier deleted (soft).</response>
        /// <response code="404">Supplier not found.</response>
        /// <response code="422">Supplier has active purchasing documents and cannot be deleted.</response>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = Purchasing.suppliers.Write)]
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
