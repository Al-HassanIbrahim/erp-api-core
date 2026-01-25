using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Entities.Core;

namespace ERPSystem.Domain.Abstractions
{
    public interface ICompanyRepository
    {
        Task<Company?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Company?> GetByIdTrackingAsync(int id, CancellationToken ct = default); // New: for updates
        Task<List<Company>> GetAllAsync(CancellationToken ct = default);
        Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default); // New
        Task AddAsync(Company company, CancellationToken ct = default);
        void Update(Company company);
        Task SoftDeleteAsync(int id, Guid deletedByUserId, CancellationToken ct = default);
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
