using ERPSystem.Application.DTOs.Core;

namespace ERPSystem.Application.Interfaces
{
    public interface ICompanyProfileService
    {
        Task<CompanyMeDto?> GetMyCompanyAsync(CancellationToken ct = default);
        Task<CompanyMeDto> UpdateMyCompanyAsync(UpdateCompanyMeDto dto, CancellationToken ct = default);
    }
}