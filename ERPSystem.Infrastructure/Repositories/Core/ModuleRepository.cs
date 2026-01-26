using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Core;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Core
{
    public class ModuleRepository : IModuleRepository
    {
        private readonly AppDbContext _context;

        public ModuleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Module?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Modules
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, ct);
        }

        public async Task<Module?> GetByKeyAsync(string key, CancellationToken ct = default)
        {
            return await _context.Modules
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Key == key.Trim() && !m.IsDeleted, ct);
        }

        public async Task<List<Module>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Modules
                .AsNoTracking()
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.Name)
                .ToListAsync(ct);
        }

        public async Task<bool> KeyExistsAsync(string key, int? excludeId = null, CancellationToken ct = default)
        {
            var query = _context.Modules
                .Where(m => m.Key == key.Trim() && !m.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(m => m.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }

        public async Task AddAsync(Module module, CancellationToken ct = default)
        {
            await _context.Modules.AddAsync(module, ct);
        }

        public void Update(Module module)
        {
            module.UpdatedAt = DateTime.UtcNow;
            _context.Modules.Update(module);
        }

        public async Task SoftDeleteAsync(int id, CancellationToken ct = default)
        {
            var module = await _context.Modules
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, ct);

            if (module is null) return;

            module.IsDeleted = true;
            module.UpdatedAt = DateTime.UtcNow;

            _context.Modules.Update(module);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}