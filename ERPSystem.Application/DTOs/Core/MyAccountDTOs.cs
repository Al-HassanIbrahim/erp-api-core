using System.ComponentModel.DataAnnotations;

namespace ERPSystem.Application.DTOs.Core
{
    /// <summary>
    /// Current user's account profile (read-only fields like PhoneNumber).
    /// </summary>
    public class MyAccountDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public int CompanyId { get; set; }
        public string? ProfileImageUrl { get; set; }

        /// <summary>
        /// Read-only: managed by admin only.
        /// </summary>
        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// Request to change current user's password.
    /// </summary>
    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = default!;

        [Required, MinLength(6)]
        public string NewPassword { get; set; } = default!;
    }

    /// <summary>
    /// Response after uploading profile image.
    /// </summary>
    public class ProfileImageUploadResult
    {
        public string ProfileImageUrl { get; set; } = default!;
    }
}