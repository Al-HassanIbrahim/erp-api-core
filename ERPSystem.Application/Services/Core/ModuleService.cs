using ERPSystem.Application.DTOs.Core;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Core;

namespace ERPSystem.Application.Services.Core
{
    public class ModuleService : IModuleService
    {
        private readonly IModuleRepository _repository;

        public ModuleService(IModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<ModuleDto>> GetAllAsync(CancellationToken ct = default)
        {
            var modules = await _repository.GetAllAsync(ct);
            return modules.Select(m => new ModuleDto
            {
                Id = m.Id,
                Key = m.Key,
                Name = m.Name,
                Description = m.Description,
                IsActive = m.IsActive
            }).ToList();
        }

        public async Task<ModuleDto> CreateAsync(CreateModuleDto dto, CancellationToken ct = default)
        {
            if (await _repository.KeyExistsAsync(dto.Key.Trim(), null, ct))
                throw new BusinessException("KEY_EXISTS", "Module key already exists.", 409);

            var module = new Module
            {
                Key = dto.Key.Trim().ToUpperInvariant(),
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                IsActive = dto.IsActive
            };

            await _repository.AddAsync(module, ct);
            await _repository.SaveChangesAsync(ct);

            return new ModuleDto
            {
                Id = module.Id,
                Key = module.Key,
                Name = module.Name,
                Description = module.Description,
                IsActive = module.IsActive
            };
        }
    }
}