namespace ERPSystem.Application.DTOs.Authorization;

public class PermissionGroupDto
{
    public string Module { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public List<PermissionItemDto> Permissions { get; set; } = [];
}

public class PermissionItemDto
{
    public string Key { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}