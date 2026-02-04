using Microsoft.AspNetCore.Authorization;

namespace ERPSystem.Application.Authorization;

/// <summary>
/// Authorization requirement that checks for a specific permission claim.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }
}

/// <summary>
/// Handles permission-based authorization by checking the "permission" claim.
/// </summary>
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private const string PermissionClaimType = "permission";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User is null)
        {
            return Task.CompletedTask;
        }

        var hasPermission = context.User.Claims
            .Where(c => c.Type == PermissionClaimType)
            .Any(c => c.Value.Equals(requirement.Permission, StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}