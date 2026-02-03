using System.Reflection;
using ERPSystem.Application.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace ERPSyatem.API.Extensions;

public static class PermissionPolicyExtensions
{
    /// <summary>
    /// Registers authorization policies for all permissions defined in the Permissions class.
    /// Each policy name matches the permission string exactly.
    /// </summary>
    public static IServiceCollection AddPermissionPolicies(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

        services.AddAuthorizationBuilder()
            .AddPoliciesFromPermissions();

        return services;
    }

    private static AuthorizationBuilder AddPoliciesFromPermissions(this AuthorizationBuilder builder)
    {
        var permissionValues = GetAllPermissionValues();

        foreach (var permission in permissionValues)
        {
            builder.AddPolicy(permission, policy =>
                policy.Requirements.Add(new PermissionRequirement(permission)));
        }

        return builder;
    }

    /// <summary>
    /// Reflects over the Permissions class to extract all permission string constants.
    /// </summary>
    public static IEnumerable<string> GetAllPermissionValues()
    {
        return GetPermissionValuesFromType(typeof(Permissions));
    }

    private static IEnumerable<string> GetPermissionValuesFromType(Type type)
    {
        // Get all public const string fields from this type
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string));

        foreach (var field in fields)
        {
            var value = field.GetValue(null) as string;
            if (!string.IsNullOrEmpty(value))
            {
                yield return value;
            }
        }

        // Recursively process nested classes
        foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
        {
            foreach (var permission in GetPermissionValuesFromType(nestedType))
            {
                yield return permission;
            }
        }
    }
}