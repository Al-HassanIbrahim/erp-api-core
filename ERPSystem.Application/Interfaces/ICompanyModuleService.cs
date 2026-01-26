using ERPSystem.Application.DTOs.Core;

namespace ERPSystem.Application.Interfaces
{
    public interface ICompanyModuleService
    {
        Task<IReadOnlyList<CompanyModuleDto>> GetMyCompanyModulesAsync(CancellationToken ct = default);
        Task<CompanyModuleDto> ToggleModuleAsync(int moduleId, bool isEnabled, CancellationToken ct = default);
    }
}