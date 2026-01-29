using ERPSystem.Application.DTOs.Expenses;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [ApiController]
    [Route("api/expenses/stats")]
    [Authorize]
    public class ExpenseStatsController : ControllerBase
    {
        private readonly IExpenseStatsService _service;

        public ExpenseStatsController(IExpenseStatsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves expense summary (total, highest, lowest, count, average).
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] StatsQuery query, CancellationToken ct)
        {
            var result = await _service.GetSummaryAsync(query, ct);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves expense time series data grouped by day/week/month.
        /// </summary>
        [HttpGet("over-time")]
        public async Task<IActionResult> GetOverTime([FromQuery] StatsQuery query, CancellationToken ct)
        {
            var result = await _service.GetOverTimeAsync(query, ct);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves expense breakdown by category.
        /// </summary>
        [HttpGet("by-category")]
        public async Task<IActionResult> GetByCategory([FromQuery] StatsQuery query, CancellationToken ct)
        {
            var result = await _service.GetByCategoryAsync(query, ct);
            return Ok(result);
        }
    }
}