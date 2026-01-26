using ERPSystem.Application.DTOs.Core;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;

namespace ERPSystem.Application.Services.Core
{
    public class CompanyModuleService : ICompanyModuleService
    {
        private readonly ICompanyModuleRepository _companyModuleRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ICurrentUserService _currentUser;

        public CompanyModuleService(
            ICompanyModuleRepository companyModuleRepository,
            IModuleRepository moduleRepository,
            ICurrentUserService currentUser)
        {
            _companyModuleRepository = companyModuleRepository;
            _moduleRepository = moduleRepository;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyList<CompanyModuleDto>> GetMyCompanyModulesAsync(CancellationToken ct = default)
        {
            var allModules = await _moduleRepository.GetAllAsync(ct);
            var companyModules = await _companyModuleRepository.GetByCompanyAsync(_currentUser.CompanyId, ct);
            var enabledDict = companyModules.ToDictionary(cm => cm.ModuleId);

            return allModules.Select(m => new CompanyModuleDto
            {
                ModuleId = m.Id,
                ModuleKey = m.Key,
                ModuleName = m.Name,
                IsEnabled = enabledDict.TryGetValue(m.Id, out var cm) && cm.IsEnabled,
                EnabledAt = enabledDict.TryGetValue(m.Id, out var cm2) ? cm2.EnabledAt : null,
                ExpiresAt = enabledDict.TryGetValue(m.Id, out var cm3) ? cm3.ExpiresAt : null
            }).ToList();
        }

        public async Task<CompanyModuleDto> ToggleModuleAsync(int moduleId, bool isEnabled, CancellationToken ct = default)
        {
            var module = await _moduleRepository.GetByIdAsync(moduleId, ct)
                ?? throw new BusinessException("MODULE_NOT_FOUND", "Module not found.", 404);

            if (isEnabled && !module.IsActive)
                throw new BusinessException("MODULE_INACTIVE", "Cannot enable an inactive module.", 400);

            if (isEnabled)
                await _companyModuleRepository.EnableAsync(_currentUser.CompanyId, moduleId, _currentUser.UserId, ct);
            else
                await _companyModuleRepository.DisableAsync(_currentUser.CompanyId, moduleId, _currentUser.UserId, ct);

            await _companyModuleRepository.SaveChangesAsync(ct);

            var cm = await _companyModuleRepository.GetAsync(_currentUser.CompanyId, moduleId, ct);

            return new CompanyModuleDto
            {
                ModuleId = module.Id,
                ModuleKey = module.Key,
                ModuleName = module.Name,
                IsEnabled = cm?.IsEnabled ?? false,
                EnabledAt = cm?.EnabledAt,
                ExpiresAt = cm?.ExpiresAt
            };
        }
    }
}