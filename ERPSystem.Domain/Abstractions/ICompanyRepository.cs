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
        Task<List<Company>> GetAllAsync(CancellationToken ct = default);

        Task AddAsync(Company company, CancellationToken ct = default);
        void Update(Company company);

        /// <summary>
        /// Soft delete + audit.
        /// </summary>
        Task SoftDeleteAsync(int id, Guid deletedByUserId, CancellationToken ct = default);

        Task<bool> ExistsAsync(int id, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
