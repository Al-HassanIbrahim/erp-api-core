namespace ERPSystem.Application.DTOs.Core
{
    public class CompanyUserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string? FullName { get; set; }
        public bool IsActive { get; set; }
        public bool IsLockedOut { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class CreateCompanyUserDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string? FullName { get; set; }
        public List<string>? Roles { get; set; }
    }

    public class UpdateUserRolesDto
    {
        public List<string> Roles { get; set; } = new();
    }

    public class UpdateUserStatusDto
    {
        public bool IsActive { get; set; }
    }
}