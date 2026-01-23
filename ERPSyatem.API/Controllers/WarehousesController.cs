using ERPSystem.Application.DTOs.Inventory;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehousesController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? companyId, [FromQuery] int? branchId)
        {
            var result = await _warehouseService.GetAllAsync(companyId, branchId);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var warehouse = await _warehouseService.GetByIdAsync(id);
            return warehouse == null ? NotFound() : Ok(warehouse);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateWarehouseDto dto)
        {
            var id = await _warehouseService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateWarehouseDto dto)
        {
            var updated = await _warehouseService.UpdateAsync(id, dto);
            if (!updated) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _warehouseService.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
