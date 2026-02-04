using System.ComponentModel.DataAnnotations;

namespace ERPSystem.Application.DTOs.Core
{
    public class CompanyUserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public bool IsLockedOut { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class CreateCompanyUserDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required, MinLength(6)]
        public string Password { get; set; } = default!;

        [Required]
        public string FullName { get; set; } = default!;

        /// <summary>
        /// Optional phone number (admin-managed).
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Optional role display names to assign on creation.
        /// </summary>
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

    /// <summary>
    /// Request to assign a role to a user (used in company-users endpoints).
    /// </summary>
    public class UserRoleAssignmentRequest
    {
        [Required]
        public string RoleName { get; set; } = default!;
    }

    /// <summary>
    /// Request to remove a role from a user (used in company-users endpoints).
    /// </summary>
    public class UserRoleRemovalRequest
    {
        [Required]
        public string RoleName { get; set; } = default!;
    }

    /// <summary>
    /// Admin update user profile request.
    /// </summary>
    public class AdminUpdateUserProfileDto      
    {
        [Required]
        public string FullName { get; set; } = default!;

        /// <summary>
        /// Optional phone number (admin-managed).
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Optional: only if email change is allowed.
        /// </summary>
        public string? Email { get; set; }
    }
}