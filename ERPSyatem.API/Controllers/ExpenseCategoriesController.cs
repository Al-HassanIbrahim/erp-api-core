using ERPSystem.Application.Authorization;
using ERPSystem.Application.DTOs.Expenses;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [ApiController]
    [Route("api/expense-categories")]
    [Authorize]
    public class ExpenseCategoriesController : ControllerBase
    {
        private readonly IExpenseCategoryService _service;

        public ExpenseCategoriesController(IExpenseCategoryService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all expense categories for the current company.
        /// </summary>
        [HttpGet]
        [Authorize(Policy =Permissions.Expenses.Items.Read)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _service.GetAllAsync(ct);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific expense category with stats.
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
        /// Creates a new expense category.
        /// </summary>
        [HttpPost]
        [Authorize(Policy =Permissions.Expenses.Items.Create)]
        public async Task<IActionResult> Create([FromBody] CreateExpenseCategoryDto dto, CancellationToken ct)
        {
            var result = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Updates an existing expense category.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy =Permissions.Expenses.Items.Update)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseCategoryDto dto, CancellationToken ct)
        {
            var result = await _service.UpdateAsync(id, dto, ct);
            return Ok(result);
        }

        /// <summary>
        /// Soft-deletes an expense category.
        /// Only categories without expenses can be deleted.
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