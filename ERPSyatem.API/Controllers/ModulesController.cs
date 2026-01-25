using ERPSystem.Application.DTOs.Core;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [ApiController]
    [Route("api/modules")]
    public class ModulesController : ControllerBase
    {
        private readonly IModuleService _service;

        public ModulesController(IModuleService service)
        {
            _service = service;
        }

        // Get all available modules
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _service.GetAllAsync(ct);
            return Ok(result);
        }

        // Create a new module (restricted to SystemAdmin)
        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] CreateModuleDto dto, CancellationToken ct)
        //{
        //    var result = await _service.CreateAsync(dto, ct);
        //    return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
        //}
    }
}