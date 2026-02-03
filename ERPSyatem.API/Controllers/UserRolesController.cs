using ERPSystem.Application.Authorization;
using ERPSystem.Application.DTOs.Authorization;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers;

[ApiController]
[Route("api/access/user-roles")]
[Authorize]
public class UserRolesController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ICurrentUserService _currentUser;

    public UserRolesController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ICurrentUserService currentUser)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Assigns a role to a user within the current company.
    /// </summary>
    [HttpPost("assign")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    // TODO: Restrict to Owner/Admin
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request, CancellationToken ct)
    {
        var companyId = _currentUser.CompanyId;
        var displayName = (request.RoleName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(displayName))
            throw BusinessErrors.RoleNameRequired();

        // Find the user
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw BusinessErrors.UserNotFound();

        // Verify user belongs to the same company
        if (user.CompanyId != companyId)
            throw BusinessErrors.CannotAssignRoleAcrossCompanies();

        // Find the role (must exist in this company)
        var scopedName = RoleKey.ForCompany(companyId, displayName);
        var role = await _roleManager.FindByNameAsync(scopedName);
        if (role == null)
            throw BusinessErrors.RoleNotFound(displayName);

        // Assign the role
        var result = await _userManager.AddToRoleAsync(user, scopedName);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw BusinessErrors.RoleOperationFailed("assign", errors);
        }

        return NoContent();
    }

    /// <summary>
    /// Removes a role from a user within the current company.
    /// </summary>
    [HttpPost("remove")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    // TODO: Restrict to Owner/Admin
    public async Task<IActionResult> RemoveRole([FromBody] AssignRoleRequest request, CancellationToken ct)
    {
        var companyId = _currentUser.CompanyId;
        var displayName = (request.RoleName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(displayName))
            throw BusinessErrors.RoleNameRequired();

        // Find the user
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw BusinessErrors.UserNotFound();

        // Verify user belongs to the same company
        if (user.CompanyId != companyId)
            throw BusinessErrors.CannotAssignRoleAcrossCompanies();

        // Find the role (must exist in this company)
        var scopedName = RoleKey.ForCompany(companyId, displayName);
        var role = await _roleManager.FindByNameAsync(scopedName);
        if (role == null)
            throw BusinessErrors.RoleNotFound(displayName);

        // Remove the role
        var result = await _userManager.RemoveFromRoleAsync(user, scopedName);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw BusinessErrors.RoleOperationFailed("remove", errors);
        }

        return NoContent();
    }

    /// <summary>
    /// Gets all roles for a user within the current company.
    /// </summary>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(UserRolesDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    // TODO: Restrict to Owner/Admin or self
    public async Task<ActionResult<UserRolesDto>> GetUserRoles(Guid userId, CancellationToken ct)
    {
        var companyId = _currentUser.CompanyId;

        // Find the user
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw BusinessErrors.UserNotFound();

        // Verify user belongs to the same company
        if (user.CompanyId != companyId)
            throw BusinessErrors.Unauthorized("Cannot view roles for users in other companies.");

        // Get all roles for the user
        var allRoles = await _userManager.GetRolesAsync(user);

        // Filter to only company-scoped roles and extract display names
        var companyRoles = allRoles
            .Where(r => RoleKey.BelongsToCompany(r, companyId))
            .Select(RoleKey.GetDisplayName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        return Ok(new UserRolesDto
        {
            UserId = userId,
            Roles = companyRoles
        });
    }
}
