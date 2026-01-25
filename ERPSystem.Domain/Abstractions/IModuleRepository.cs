using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Entities.Core;

namespace ERPSystem.Domain.Abstractions
{
    public interface IModuleRepository
    {
        Task<Module?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Module?> GetByKeyAsync(string key, CancellationToken ct = default);
        Task<List<Module>> GetAllAsync(CancellationToken ct = default);
        Task<bool> KeyExistsAsync(string key, int? excludeId = null, CancellationToken ct = default); // New

        Task AddAsync(Module module, CancellationToken ct = default);
        void Update(Module module);

        Task SoftDeleteAsync(int id, CancellationToken ct = default);

        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
