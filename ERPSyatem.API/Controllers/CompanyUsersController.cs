using ERPSystem.Application.DTOs.Core;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [ApiController]
    [Route("api/company-users")]
    [Authorize] // TODO: Add Roles = "CompanyOwner,CompanyAdmin" when role system is complete
    public class CompanyUsersController : ControllerBase
    {
        private readonly ICompanyUserService _service;

        public CompanyUsersController(ICompanyUserService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all users in current company
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _service.GetAllAsync(ct);
            return Ok(result);
        }

        /// <summary>
        /// Create a new user in current company
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCompanyUserDto dto, CancellationToken ct)
        {
            var result = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
        }

        /// <summary>
        /// Update user roles (CompanyOwner only for assigning CompanyOwner)
        /// </summary>
        [HttpPut("{userId}/roles")]
        [Authorize(Roles = "CompanyOwner")] // Restrict to owner for role changes
        public async Task<IActionResult> UpdateRoles(Guid userId, [FromBody] UpdateUserRolesDto dto, CancellationToken ct)
        {
            var result = await _service.UpdateRolesAsync(userId, dto, ct);
            return Ok(result);
        }

        /// <summary>
        /// Enable/disable (lock/unlock) a user in current company
        /// </summary>
        [HttpPut("{userId}/status")]
        public async Task<IActionResult> UpdateStatus(Guid userId, [FromBody] UpdateUserStatusDto dto, CancellationToken ct)
        {
            var result = await _service.UpdateStatusAsync(userId, dto, ct);
            return Ok(result);
        }
    }
}