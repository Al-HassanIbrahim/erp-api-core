using System.Reflection;
using ERPSystem.Application.Authorization;
using ERPSystem.Application.DTOs.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers;

[Route("api/access/permissions")]
[ApiController]
[Authorize]
public class PermissionsController : ControllerBase
{
    /// <summary>
    /// Returns all available permissions grouped by module and resource for UI rendering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PermissionGroupDto>), 200)]
    public ActionResult<IEnumerable<PermissionGroupDto>> GetAllPermissions()
    {
        var permissions = ExtractPermissions();
        var grouped = permissions
            .GroupBy(p => new { p.Module, p.Resource })
            .Select(g => new PermissionGroupDto
            {
                Module = g.Key.Module,
                Resource = g.Key.Resource,
                Permissions = g.Select(p => new PermissionItemDto
                {
                    Key = p.Key,
                    Action = p.Action,
                    Description = GenerateDescription(g.Key.Module, g.Key.Resource, p.Action)
                }).ToList()
            })
            .OrderBy(g => g.Module)
            .ThenBy(g => g.Resource)
            .ToList();

        return Ok(grouped);
    }

    private static List<(string Key, string Module, string Resource, string Action)> ExtractPermissions()
    {
        var result = new List<(string Key, string Module, string Resource, string Action)>();
        ExtractFromType(typeof(Permissions), [], result);
        return result;
    }

    private static void ExtractFromType(
        Type type,
        List<string> path,
        List<(string Key, string Module, string Resource, string Action)> result)
    {
        // Get const string fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string));

        foreach (var field in fields)
        {
            var value = field.GetValue(null) as string;
            if (!string.IsNullOrEmpty(value))
            {
                var parts = value.Split('.');
                if (parts.Length >= 3)
                {
                    result.Add((value, parts[0], parts[1], parts[2]));
                }
            }
        }

        // Process nested types
        foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
        {
            var newPath = new List<string>(path) { nestedType.Name.ToLowerInvariant() };
            ExtractFromType(nestedType, newPath, result);
        }
    }

    private static string GenerateDescription(string module, string resource, string action)
    {
        var actionDescriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["read"] = "View",
            ["create"] = "Create new",
            ["update"] = "Edit existing",
            ["delete"] = "Remove",
            ["post"] = "Post/finalize",
            ["void"] = "Void/cancel",
            ["approve"] = "Approve",
            ["reject"] = "Reject",
            ["adjust"] = "Adjust",
            ["transfer"] = "Transfer",
            ["opening"] = "Set opening balances for",
            ["manage"] = "Manage"
        };

        var actionText = actionDescriptions.TryGetValue(action, out var desc) ? desc : Capitalize(action);
        var resourceText = Capitalize(resource);
        var moduleText = Capitalize(module);

        return $"{actionText} {resourceText} ({moduleText})";
    }

    private static string Capitalize(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpperInvariant(input[0]) + input[1..];
    }
}