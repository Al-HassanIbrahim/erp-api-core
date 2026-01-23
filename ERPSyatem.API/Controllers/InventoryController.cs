using ERPSystem.Application.DTOs.Inventory;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpPost("in")]
        public async Task<IActionResult> StockIn([FromBody] StockInRequest request, CancellationToken cancellationToken)
        {
            var result = await _inventoryService.StockInAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("out")]
        public async Task<IActionResult> StockOut([FromBody] StockOutRequest request, CancellationToken cancellationToken)
        {
            var result = await _inventoryService.StockOutAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] StockTransferRequest request, CancellationToken cancellationToken)
        {
            var result = await _inventoryService.TransferAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("opening-balance")]
        public async Task<IActionResult> OpeningBalance([FromBody] OpeningBalanceRequest request, CancellationToken cancellationToken)
        {
            var result = await _inventoryService.OpeningBalanceAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("adjustment")]
        public async Task<IActionResult> Adjustment([FromBody] StockAdjustmentRequest request, CancellationToken cancellationToken)
        {
            var result = await _inventoryService.AdjustmentAsync(request, cancellationToken);
            return Ok(result);
        }
    }
}