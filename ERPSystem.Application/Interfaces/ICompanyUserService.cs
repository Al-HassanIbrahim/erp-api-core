using ERPSystem.Application.DTOs.Core;

namespace ERPSystem.Application.Interfaces
{
    public interface ICompanyUserService
    {
        Task<IReadOnlyList<CompanyUserDto>> GetAllAsync(CancellationToken ct = default);
        Task<CompanyUserDto> CreateAsync(CreateCompanyUserDto dto, CancellationToken ct = default);
        Task<CompanyUserDto> UpdateStatusAsync(Guid userId, UpdateUserStatusDto dto, CancellationToken ct = default);

        /// <summary>
        /// Assigns a role (by display name) to a user in the current company.
        /// </summary>
        Task<CompanyUserDto> AssignRoleAsync(Guid userId, string roleDisplayName, CancellationToken ct = default);

        /// <summary>
        /// Removes a role (by display name) from a user in the current company.
        /// </summary>
        Task<CompanyUserDto> RemoveRoleAsync(Guid userId, string roleDisplayName, CancellationToken ct = default);

        /// <summary>
        /// Updates user profile (FullName, PhoneNumber, optionally Email) by admin.
        /// </summary>
        Task<CompanyUserDto> UpdateProfileAsync(Guid userId, AdminUpdateUserProfileDto dto, CancellationToken ct = default);
    }
}