using System.Reflection;
using System.Security.Claims;
using ERPSystem.Application.Authorization;
using ERPSystem.Application.DTOs.Authorization;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers;

[ApiController]
[Route("api/access/roles")]
[Authorize(Policy =Permissions.Security.Roles.Manage)]
public class RolesController : ControllerBase
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ICurrentUserService _currentUser;
    private static readonly Lazy<HashSet<string>> _allPermissions = new(LoadAllPermissions);

    public RolesController(
        RoleManager<IdentityRole<Guid>> roleManager,
        ICurrentUserService currentUser)
    {
        _roleManager = roleManager;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Lists all roles for the current company with their permissions.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleDto>), 200)]
    // TODO: Restrict to Owner/Admin
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles(CancellationToken ct)
    {
        var companyId = _currentUser.CompanyId;

        var companyRoles = _roleManager.Roles
            .AsEnumerable()
            .Where(r => RoleKey.BelongsToCompany(r.Name, companyId))
            .ToList();

        var result = new List<RoleDto>();
        foreach (var role in companyRoles)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            var permissions = claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            result.Add(new RoleDto
            {
                Name = RoleKey.GetDisplayName(role.Name),
                Permissions = permissions
            });
        }

        return Ok(result.OrderBy(r => r.Name));
    }

    /// <summary>
    /// Creates a new role with the specified permissions.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    // TODO: Restrict to Owner/Admin
    public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        var companyId = _currentUser.CompanyId;
        var displayName = (request.Name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(displayName))
            throw BusinessErrors.RoleNameRequired();

        ValidatePermissionsOrThrow(request.Permissions);

        var scopedName = RoleKey.ForCompany(companyId, displayName);

        // Check if role already exists
        var existing = await _roleManager.FindByNameAsync(scopedName);
        if (existing != null)
            throw BusinessErrors.RoleAlreadyExists(displayName);

        // Create the role
        var createResult = await _roleManager.CreateAsync(new IdentityRole<Guid>(scopedName));
        if (!createResult.Succeeded)
            throw BusinessErrors.RoleOperationFailed("create", FormatErrors(createResult.Errors));

        // Fetch the created role
        var role = await _roleManager.FindByNameAsync(scopedName);
        if (role == null)
            throw BusinessErrors.RoleOperationFailed("create", "Role not found after creation");

        // Add permission claims
        var uniquePermissions = request.Permissions
            .Select(p => p.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var permission in uniquePermissions)
        {
            var addResult = await _roleManager.AddClaimAsync(role, new Claim("permission", permission));
            if (!addResult.Succeeded)
                throw BusinessErrors.RoleOperationFailed("add permission", FormatErrors(addResult.Errors));
        }

        return CreatedAtAction(nameof(GetRoles), null, new RoleDto
        {
            Name = displayName,
            Permissions = uniquePermissions.OrderBy(x => x).ToList()
        });
    }

    /// <summary>
    /// Replaces all permissions for a role with the provided set.
    /// </summary>
    [HttpPut("{roleName}")]
    [ProducesResponseType(typeof(RoleDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    // TODO: Restrict to Owner/Admin
    public async Task<ActionResult<RoleDto>> UpdateRolePermissions(
        string roleName,
        [FromBody] UpdateRolePermissionsRequest request,
        CancellationToken ct)
    {
        var companyId = _currentUser.CompanyId;
        var displayName = (roleName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(displayName))
            throw BusinessErrors.RoleNameRequired();

        ValidatePermissionsOrThrow(request.Permissions);

        var scopedName = RoleKey.ForCompany(companyId, displayName);
        var role = await _roleManager.FindByNameAsync(scopedName);
        if (role == null)
            throw BusinessErrors.RoleNotFound(displayName);

        // Remove all existing permission claims
        var existingClaims = await _roleManager.GetClaimsAsync(role);
        var permissionClaims = existingClaims.Where(c => c.Type == "permission").ToList();

        foreach (var claim in permissionClaims)
        {
            var removeResult = await _roleManager.RemoveClaimAsync(role, claim);
            if (!removeResult.Succeeded)
                throw BusinessErrors.RoleOperationFailed("remove permission", FormatErrors(removeResult.Errors));
        }

        // Add new permission claims
        var uniquePermissions = request.Permissions
            .Select(p => p.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var permission in uniquePermissions)
        {
            var addResult = await _roleManager.AddClaimAsync(role, new Claim("permission", permission));
            if (!addResult.Succeeded)
                throw BusinessErrors.RoleOperationFailed("add permission", FormatErrors(addResult.Errors));
        }

        return Ok(new RoleDto
        {
            Name = displayName,
            Permissions = uniquePermissions.OrderBy(x => x).ToList()
        });
    }

    /// <summary>
    /// Deletes a role from the current company.
    /// </summary>
    [HttpDelete("{roleName}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    // TODO: Restrict to Owner/Admin
    public async Task<IActionResult> DeleteRole(string roleName, CancellationToken ct)
    {
        var companyId = _currentUser.CompanyId;
        var displayName = (roleName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(displayName))
            throw BusinessErrors.RoleNameRequired();

        var scopedName = RoleKey.ForCompany(companyId, displayName);
        var role = await _roleManager.FindByNameAsync(scopedName);
        if (role == null)
            throw BusinessErrors.RoleNotFound(displayName);

        var deleteResult = await _roleManager.DeleteAsync(role);
        if (!deleteResult.Succeeded)
            throw BusinessErrors.RoleOperationFailed("delete", FormatErrors(deleteResult.Errors));

        return NoContent();
    }

    #region Helpers

    private static HashSet<string> LoadAllPermissions()
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        CollectPermissions(typeof(Permissions), set);
        return set;
    }

    private static void CollectPermissions(Type type, HashSet<string> set)
    {
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string));

        foreach (var field in fields)
        {
            if (field.GetValue(null) is string value && !string.IsNullOrWhiteSpace(value))
                set.Add(value);
        }

        foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
        {
            CollectPermissions(nestedType, set);
        }
    }

    private static void ValidatePermissionsOrThrow(IEnumerable<string>? permissions)
    {
        if (permissions == null)
            return;

        foreach (var p in permissions)
        {
            var key = (p ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(key))
                throw BusinessErrors.EmptyPermissionKey();

            if (!_allPermissions.Value.Contains(key))
                throw BusinessErrors.UnknownPermission(key);
        }
    }

    private static string FormatErrors(IEnumerable<IdentityError> errors)
    {
        return string.Join("; ", errors.Select(e => e.Description));
    }

    #endregion
}
