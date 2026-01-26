using ERPSystem.Application.DTOs.Core;

namespace ERPSystem.Application.Interfaces
{
    public interface ICompanyUserService
    {
        Task<IReadOnlyList<CompanyUserDto>> GetAllAsync(CancellationToken ct = default);
        Task<CompanyUserDto> CreateAsync(CreateCompanyUserDto dto, CancellationToken ct = default);
        Task<CompanyUserDto> UpdateRolesAsync(Guid userId, UpdateUserRolesDto dto, CancellationToken ct = default);
        Task<CompanyUserDto> UpdateStatusAsync(Guid userId, UpdateUserStatusDto dto, CancellationToken ct = default);
    }
}