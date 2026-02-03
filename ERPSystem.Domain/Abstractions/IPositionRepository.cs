using ERPSystem.Domain.Entities.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Abstractions
{
    public interface IPositionRepository
    {
        Task<JobPosition?> GetByIdAsync(Guid id, int companyId, CancellationToken ct = default);
        Task<IEnumerable<JobPosition>> GetAllAsync(int companyId, CancellationToken ct = default);
        Task<bool> ExistsByCodeAsync(string code, int companyId, CancellationToken ct = default);
        Task AddAsync(JobPosition position);
        Task UpdateAsync(JobPosition position);
        Task DeleteAsync(Guid id, int companyId, CancellationToken ct = default);
    }
}
