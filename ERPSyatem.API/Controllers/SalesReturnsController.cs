using ERPSystem.Application.DTOs.Sales;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
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

        [HttpGet]
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSalesReturnRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPost("{id}/post")]
        public async Task<IActionResult> Post(int id, CancellationToken cancellationToken)
        {
            var result = await _service.PostAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
        {
            var result = await _service.CancelAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}