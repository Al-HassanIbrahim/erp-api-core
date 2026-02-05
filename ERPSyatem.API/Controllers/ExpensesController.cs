using ERPSystem.Application.Authorization;
using ERPSystem.Application.DTOs.Expenses;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [ApiController]
    [Route("api/expenses")]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _service;

        public ExpensesController(IExpenseService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves a paged list of expenses with optional filters.
        /// </summary>
        [HttpGet]
        [Authorize(Policy =Permissions.Expenses.Items.Read)]
        public async Task<IActionResult> GetAll([FromQuery] ExpenseQuery query, CancellationToken ct)
        {
            var result = await _service.GetAllAsync(query, ct);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific expense by its identifier.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy =Permissions.Expenses.Items.Read)]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Creates a new expense.
        /// </summary>
        [HttpPost]
        [Authorize(Policy =Permissions.Expenses.Items.Create)]
        public async Task<IActionResult> Create([FromBody] CreateExpenseDto dto, CancellationToken ct)
        {
            var result = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Updates an existing expense.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy =Permissions.Expenses.Items.Update)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseDto dto, CancellationToken ct)
        {
            var result = await _service.UpdateAsync(id, dto, ct);
            return Ok(result);
        }

        /// <summary>
        /// Updates only the status of an expense.
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Policy =Permissions.Expenses.Items.Update)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateExpenseStatusDto dto, CancellationToken ct)
        {
            var result = await _service.UpdateStatusAsync(id, dto, ct);
            return Ok(result);
        }

        /// <summary>
        /// Soft-deletes an expense.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy =Permissions.Expenses.Items.Delete)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}