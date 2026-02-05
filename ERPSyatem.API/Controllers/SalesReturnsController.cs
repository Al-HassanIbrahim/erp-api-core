using ERPSystem.Application.Authorization;
using ERPSystem.Application.DTOs.Sales;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    /// <summary>
    /// Manages sales returns including creation, posting,
    /// and cancellation of returned goods.
    /// </summary>
    [ApiController]
    [Route("api/sales/returns")]
    [Authorize]
    public class SalesReturnsController : ControllerBase
    {
        private readonly ISalesReturnService _service;

        public SalesReturnsController(ISalesReturnService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves sales returns for the current company,
        /// filtered by customer, status, or date range.
        /// </summary>
        [HttpGet]
        [Authorize(Policy = Permissions.Sales.Returns.Read)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? customerId,
            [FromQuery] SalesReturnStatus? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetAllAsync(customerId, status, fromDate, toDate, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific sales return document by its identifier.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.Sales.Returns.Read)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Creates a new sales return document.
        /// The return can be posted to affect inventory if enabled.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = Permissions.Sales.Returns.Access)]
       // [Authorize(Policy = Permissions.Sales.Returns.Create)]
        public async Task<IActionResult> Create([FromBody] CreateSalesReturnRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Only If Inventory module is enabled, Posts (confirms) a sales return.
        /// returned quantitiesare added back to stock (StockIn).
        /// </summary>
        [HttpPost("{id}/post")]
        [Authorize(Policy = Permissions.Sales.Returns.Manage)]
      //  [Authorize(Policy = Permissions.Sales.Returns.Post)]
        public async Task<IActionResult> Post(int id, CancellationToken cancellationToken)
        {
            var result = await _service.PostAsync(id, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Cancels a sales return document.
        /// </summary>
        /// <remarks>
        /// Cancellation is allowed only while the sales return is in <c>Draft</c> status.
        /// A posted return cannot be cancelled because it may have already affected inventory.
        /// To reverse a posted return, a separate corrective document must be created.
        /// </remarks>
        [HttpPost("{id}/cancel")]
        [Authorize(Policy = Permissions.Sales.Returns.Access)]
      //  [Authorize(Policy = Permissions.Sales.Returns.Cancel)]
        public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
        {
            var result = await _service.CancelAsync(id, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Soft-deletes a sales return document.
        /// </summary>
        /// <remarks>
        /// Deletion is allowed only for sales returns in <c>Draft</c> status.
        /// The document is not physically removed from the database,
        /// but is marked as deleted and excluded from future queries.
        /// Deletion should be used only when the return was created by mistake
        /// and has not caused any business or inventory impact.
        /// </remarks>
        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.Sales.Returns.Manage)]
      //  [Authorize(Policy = Permissions.Sales.Returns.Delete)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}