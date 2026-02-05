using ERPSystem.Application.Authorization;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy =Permissions.Inventory.Reports.Read)]
    public class InventoryReportsController : ControllerBase
    {
        private readonly IInventoryReportsService _reportsService;

        public InventoryReportsController(IInventoryReportsService reportsService)
        {
            _reportsService = reportsService;
        }

        /// <summary>
        /// Generic stock balance endpoint.
        /// - If both productId and warehouseId are provided, returns balance for that pair.
        /// - If only productId is provided, returns stock of that product across all warehouses.
        /// - If only warehouseId is provided, returns all products in that warehouse.
        /// - If both are null, returns all stock items.
        /// </summary>
        [HttpGet("stock-balance")]
        public async Task<IActionResult> GetStockBalance(
            [FromQuery] int? productId,
            [FromQuery] int? warehouseId,
            CancellationToken cancellationToken = default)
        {
            var result = await _reportsService.GetStockBalanceAsync(productId, warehouseId, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Returns stock for all products in a specific warehouse.
        /// </summary>
        [HttpGet("warehouse/{warehouseId}/stock")]
        public async Task<IActionResult> GetWarehouseStock(
            int warehouseId,
            CancellationToken cancellationToken = default)
        {
            var result = await _reportsService.GetWarehouseStockAsync(warehouseId, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Returns stock for a specific product across all warehouses.
        /// </summary>
        [HttpGet("product/{productId}/stock")]
        public async Task<IActionResult> GetProductStock(
            int productId,
            CancellationToken cancellationToken = default)
        {
            var result = await _reportsService.GetProductStockAsync(productId, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Returns inventory movements (In/Out/Transfer/Adjustment) for a product
        /// within a given date range, optionally filtered by warehouse.
        /// </summary>
        [HttpGet("movements")]
        public async Task<IActionResult> GetMovements(
            [FromQuery] int productId,
            [FromQuery] int? warehouseId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            CancellationToken cancellationToken = default)
        {
            var result = await _reportsService.GetMovementsAsync(productId, warehouseId, from, to, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Returns products that are low in stock based on MinQuantity,
        /// optionally filtered by warehouse.
        /// </summary>
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock(
            [FromQuery] int? warehouseId,
            CancellationToken cancellationToken = default)
        {
            var result = await _reportsService.GetLowStockAsync(warehouseId, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Returns inventory valuation (quantity * average cost),
        /// optionally filtered by warehouse.
        /// </summary>
        [HttpGet("valuation")]
        public async Task<IActionResult> GetValuation(
            [FromQuery] int? warehouseId,
            CancellationToken cancellationToken = default)
        {
            var result = await _reportsService.GetInventoryValuationAsync(warehouseId, cancellationToken);
            return Ok(result);
        }
    }
}


