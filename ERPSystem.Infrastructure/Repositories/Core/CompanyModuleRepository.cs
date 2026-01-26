using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Core;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Core
{
    public class CompanyModuleRepository : ICompanyModuleRepository
    {
        private readonly AppDbContext _context;

        public CompanyModuleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CompanyModule>> GetByCompanyAsync(int companyId, CancellationToken ct = default)
        {
            return await _context.CompanyModules
                .AsNoTracking()
                .Include(x => x.Module) // Include Module for mapping
                .Where(x => x.CompanyId == companyId && !x.IsDeleted && !x.Module.IsDeleted)
                .OrderBy(x => x.Module.Name)
                .ToListAsync(ct);
        }

        public async Task<CompanyModule?> GetAsync(int companyId, int moduleId, CancellationToken ct = default)
        {
            return await _context.CompanyModules
                .Include(x => x.Module)
                .FirstOrDefaultAsync(x =>
                    x.CompanyId == companyId &&
                    x.ModuleId == moduleId &&
                    !x.IsDeleted, ct);
        }

        public async Task<bool> IsModuleEnabledAsync(int companyId, string moduleKey, CancellationToken ct = default)
        {
            return await _context.CompanyModules
                .AsNoTracking()
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    !x.IsDeleted &&
                    x.IsEnabled &&
                    x.Module != null && !x.Module.IsDeleted &&
                    x.Module.Key == moduleKey.Trim(), ct);
        }

        public async Task EnableAsync(int companyId, int moduleId, Guid actorUserId, CancellationToken ct = default)
        {
            var cm = await _context.CompanyModules
                .FirstOrDefaultAsync(x =>
                    x.CompanyId == companyId &&
                    x.ModuleId == moduleId &&
                    !x.IsDeleted, ct);

            if (cm is null)
            {
                cm = new CompanyModule
                {
                    CompanyId = companyId,
                    ModuleId = moduleId,
                    IsEnabled = true,
                    EnabledAt = DateTime.UtcNow,
                    CreatedByUserId = actorUserId,
                    UpdatedByUserId = actorUserId,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.CompanyModules.AddAsync(cm, ct);
                return;
            }

            cm.IsEnabled = true;
            cm.EnabledAt = DateTime.UtcNow;
            cm.ExpiresAt = null;
            cm.UpdatedAt = DateTime.UtcNow;
            cm.UpdatedByUserId = actorUserId;

            _context.CompanyModules.Update(cm);
        }

        public async Task DisableAsync(int companyId, int moduleId, Guid actorUserId, CancellationToken ct = default)
        {
            var cm = await _context.CompanyModules
                .FirstOrDefaultAsync(x =>
                    x.CompanyId == companyId &&
                    x.ModuleId == moduleId &&
                    !x.IsDeleted, ct);

            if (cm is null) return;

            cm.IsEnabled = false;
            cm.UpdatedAt = DateTime.UtcNow;
            cm.UpdatedByUserId = actorUserId;

            _context.CompanyModules.Update(cm);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}