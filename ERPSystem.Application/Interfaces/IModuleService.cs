using ERPSystem.Application.DTOs.Core;

namespace ERPSystem.Application.Interfaces
{
    public interface IModuleService
    {
        Task<IReadOnlyList<ModuleDto>> GetAllAsync(CancellationToken ct = default);
        Task<ModuleDto> CreateAsync(CreateModuleDto dto, CancellationToken ct = default);
    }
}