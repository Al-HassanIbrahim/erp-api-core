using ERPSystem.Domain.Entities.CRM;
using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Abstractions
{
    public interface ILeadRepository
    {
        Task<Lead?> GetByIdAsync(int id, int companyId, CancellationToken ct = default);

        Task<List<Lead>> ListAsync(
            int companyId,
            LeadStatus? stage = null,
            LeadSource? source = null,
            Guid? assignedToId = null,
            string? search = null,
            CancellationToken ct = default);

        Task AddAsync(Lead lead, int companyId, CancellationToken ct = default);
        Task UpdateAsync(Lead lead, int companyId);
        Task DeleteAsync(int id, int companyId, CancellationToken ct = default);

        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
