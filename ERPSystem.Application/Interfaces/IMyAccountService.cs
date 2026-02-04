using ERPSystem.Application.DTOs.Core;

namespace ERPSystem.Application.Interfaces
{
    public interface IMyAccountService
    {
        /// <summary>
        /// Gets the current user's account profile.
        /// </summary>
        Task<MyAccountDto> GetProfileAsync(CancellationToken ct = default);

        /// <summary>
        /// Changes the current user's password.
        /// </summary>
        Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default);

        /// <summary>
        /// TODO Uploads a profile image for the current user.
        /// </summary>
       // Task<ProfileImageUploadResult> UploadProfileImageAsync(IFormFile file, CancellationToken ct = default);
    }
}