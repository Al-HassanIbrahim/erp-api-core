using ERPSystem.Application.Authorization;
using ERPSystem.Application.DTOs.Core;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [ApiController]
    [Route("api/company-users")]
    [Authorize]
    public class CompanyUsersController : ControllerBase
    {
        private readonly ICompanyUserService _service;

        public CompanyUsersController(ICompanyUserService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all users in current company.
        /// </summary>
        [HttpGet]
        [Authorize(Policy = Permissions.Core.Users.Read)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _service.GetAllAsync(ct);
            return Ok(result);
        }

        /// <summary>
        /// Create a new user in current company.
        /// Admin provides Email, FullName, Password, optional PhoneNumber, and optional roles.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = Permissions.Core.Users.Create)]
        public async Task<IActionResult> Create([FromBody] CreateCompanyUserDto dto, CancellationToken ct)
        {
            var result = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
        }

        /// <summary>
        /// Enable/disable (lock/unlock) a user in current company.
        /// </summary>
        [HttpPut("{userId:guid}/status")]
        [Authorize(Policy = Permissions.Core.Users.Update)]
        public async Task<IActionResult> UpdateStatus(Guid userId, [FromBody] UpdateUserStatusDto dto, CancellationToken ct)
        {
            var result = await _service.UpdateStatusAsync(userId, dto, ct);
            return Ok(result);
        }

        /// <summary>
        /// Assigns a role (by display name) to a user in the current company.
        /// </summary>
        [HttpPost("{userId:guid}/roles/assign")]
        [Authorize(Policy = Permissions.Core.Users.Update)]
        public async Task<IActionResult> AssignRole(Guid userId, [FromBody] UserRoleAssignmentRequest request, CancellationToken ct)
        {
            var result = await _service.AssignRoleAsync(userId, request.RoleName, ct);
            return Ok(result);
        }

        /// <summary>
        /// Removes a role (by display name) from a user in the current company.
        /// </summary>
        [HttpPost("{userId:guid}/roles/remove")]
        [Authorize(Policy = Permissions.Core.Users.Update)]
        public async Task<IActionResult> RemoveRole(Guid userId, [FromBody] UserRoleRemovalRequest request, CancellationToken ct)
        {
            var result = await _service.RemoveRoleAsync(userId, request.RoleName, ct);
            return Ok(result);
        }

        /// <summary>
        /// Updates a user's profile (FullName, PhoneNumber, optionally Email) by admin.
        /// </summary>
        [HttpPut("{userId:guid}/profile")]
        [Authorize(Policy = Permissions.Core.Users.Update)]
        public async Task<IActionResult> UpdateProfile(Guid userId, [FromBody] AdminUpdateUserProfileDto dto, CancellationToken ct)
        {
            var result = await _service.UpdateProfileAsync(userId, dto, ct);
            return Ok(result);
        }
    }
}