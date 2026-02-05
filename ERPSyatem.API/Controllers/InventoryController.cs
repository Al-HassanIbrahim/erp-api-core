using ERPSystem.Application.Authorization;
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
        [Authorize(Policy = Permissions.Inventory.Stock.StockIn)]
        public async Task<IActionResult> StockIn([FromBody] StockInRequest request, CancellationToken cancellationToken)
        {
            var result = await _inventoryService.StockInAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("out")]
        [Authorize(Policy = Permissions.Inventory.Stock.StockOut)]
        public async Task<IActionResult> StockOut([FromBody] StockOutRequest request, CancellationToken cancellationToken)
        {
            var result = await _inventoryService.StockOutAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("transfer")]
        [Authorize(Policy = Permissions.Inventory.Stock.Transfer)]
        public async Task<IActionResult> Transfer([FromBody] StockTransferRequest request, CancellationToken cancellationToken)
        {
            var result = await _inventoryService.TransferAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("opening-balance")]
        [Authorize(Policy = Permissions.Inventory.Stock.Opening)]
        public async Task<IActionResult> OpeningBalance([FromBody] OpeningBalanceRequest request, CancellationToken cancellationToken)
        {
            var result = await _inventoryService.OpeningBalanceAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("adjustment")]
        [Authorize(Policy = Permissions.Inventory.Stock.Adjust)]
        public async Task<IActionResult> Adjustment([FromBody] StockAdjustmentRequest request, CancellationToken cancellationToken)
        {
            var result = await _inventoryService.AdjustmentAsync(request, cancellationToken);
            return Ok(result);
        }
    }
}