using ERPSystem.Application.Authorization;
using ERPSystem.Application.DTOs.Core;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [ApiController]
    [Route("api/company-modules")]
    [Authorize]
    public class CompanyModulesController : ControllerBase
    {
        private readonly ICompanyModuleService _service;

        public CompanyModulesController(ICompanyModuleService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all modules with enabled status for current company
        /// </summary>
        [HttpGet]
        [Authorize(Policy = Permissions.Core.Modules.Read)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _service.GetMyCompanyModulesAsync(ct);
            return Ok(result);
        }

        /// <summary>
        /// Toggle module enabled/disabled for current company
        /// </summary>
        [HttpPut("{moduleId}")]
        [Authorize(Policy = Permissions.Core.Modules.Manage)]
        public async Task<IActionResult> Toggle(int moduleId, [FromBody] ToggleCompanyModuleDto dto, CancellationToken ct)
        {
            var result = await _service.ToggleModuleAsync(moduleId, dto.IsEnabled, ct);
            return Ok(result);
        }
    }
}