using ERPSystem.Application.Authorization;
using ERPSystem.Application.DTOs.Core;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [ApiController]
    [Route("api/companies")]
    [Authorize]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyProfileService _service;

        public CompaniesController(ICompanyProfileService service)
        {
            _service = service;
        }

        [HttpGet("me")]
        [Authorize(Policy =Permissions.Core.Companie.Read)]
        // Get current user's company details
        public async Task<IActionResult> GetMyCompany(CancellationToken ct)
        {
            var result = await _service.GetMyCompanyAsync(ct);
            return result == null ? NotFound() : Ok(result);
        }

        // Update current user's company details
        [HttpPut("me")]
        [Authorize(Policy =Permissions.Core.Companie.Update)]
        public async Task<IActionResult> UpdateMyCompany([FromBody] UpdateCompanyMeDto dto, CancellationToken ct)
        {
            var result = await _service.UpdateMyCompanyAsync(dto, ct);
            return Ok(result);
        }
    }
}