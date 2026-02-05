using ERPSystem.Domain.Entities.CRM;
using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Abstractions
{
    public interface IPipelineRepository
    {
        Task<Pipeline?> GetByIdAsync(int id, int companyId, CancellationToken ct = default);

        Task<List<Pipeline>> ListAsync(int companyId,CancellationToken ct = default);

        Task AddAsync(Pipeline pipeline, int companyId, CancellationToken ct = default);
        Task UpdateAsync(Pipeline pipeline, int companyId, CancellationToken ct = default);
        Task DeleteAsync(int id, int companyId, CancellationToken ct = default);

        Task SaveChangesAsync(CancellationToken ct = default);

    }
}
