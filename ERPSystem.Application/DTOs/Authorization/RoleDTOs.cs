namespace ERPSystem.Application.DTOs.Authorization;

/// <summary>
/// Request to create a new role with permissions.
/// </summary>
public class CreateRoleRequest
{
    /// <summary>
    /// Display name for the role (not scoped).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// List of permission keys to assign to the role.
    /// </summary>
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// Request to update a role's permissions (full replacement).
/// </summary>
public class UpdateRolePermissionsRequest
{
    /// <summary>
    /// List of permission keys to assign to the role (replaces existing).
    /// </summary>
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// Request to assign or remove a role from a user.
/// </summary>
public class AssignRoleRequest
{
    /// <summary>
    /// The user ID to assign/remove the role.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Role display name (not scoped).
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
}

/// <summary>
/// Represents a role with its permissions.
/// </summary>
public class RoleDto
{
    /// <summary>
    /// Role display name (not scoped).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// List of permission keys assigned to the role.
    /// </summary>
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// Represents a user's roles within a company.
/// </summary>
public class UserRolesDto
{
    /// <summary>
    /// The user ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// List of role display names (not scoped).
    /// </summary>
    public List<string> Roles { get; set; } = new();
}
