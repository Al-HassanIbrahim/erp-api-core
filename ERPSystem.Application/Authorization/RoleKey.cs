namespace ERPSystem.Application.Authorization;

/// <summary>
/// Helper for company-scoped role names.
/// Format: "c:{companyId}:{roleDisplayName}"
/// </summary>
public static class RoleKey
{
    private const string Prefix = "c";

    /// <summary>
    /// Creates a scoped role name for the given company and display name.
    /// </summary>
    public static string ForCompany(int companyId, string roleDisplayName)
    {
        if (string.IsNullOrWhiteSpace(roleDisplayName))
            throw new ArgumentException("Role display name is required.", nameof(roleDisplayName));

        var trimmed = roleDisplayName.Trim();
        return $"{Prefix}:{companyId}:{trimmed}";
    }

    /// <summary>
    /// Attempts to parse a scoped role name into company ID and display name.
    /// </summary>
    public static bool TryParse(string? scopedName, out int companyId, out string roleDisplayName)
    {
        companyId = default;
        roleDisplayName = string.Empty;

        if (string.IsNullOrWhiteSpace(scopedName))
            return false;

        // Expected format: c:{companyId}:{roleDisplayName}
        var parts = scopedName.Split(':', 3, StringSplitOptions.None);
        if (parts.Length != 3)
            return false;

        if (!string.Equals(parts[0], Prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!int.TryParse(parts[1], out companyId))
            return false;

        roleDisplayName = parts[2];
        return !string.IsNullOrWhiteSpace(roleDisplayName);
    }

    /// <summary>
    /// Checks if the scoped name belongs to the specified company.
    /// </summary>
    public static bool BelongsToCompany(string? scopedName, int companyId)
    {
        return TryParse(scopedName, out var cid, out _) && cid == companyId;
    }

    /// <summary>
    /// Extracts the display name from a scoped role name, or returns the original if not parseable.
    /// </summary>
    public static string GetDisplayName(string? scopedName)
    {
        if (TryParse(scopedName, out _, out var displayName))
            return displayName;
        return scopedName ?? string.Empty;
    }
}
