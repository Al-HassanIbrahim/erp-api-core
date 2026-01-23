using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Entities.Core;

namespace ERPSystem.Domain.Abstractions
{
    public interface ICompanyModuleRepository
    {
        Task<List<CompanyModule>> GetCompanyModulesAsync(int companyId, CancellationToken ct = default);

        Task<CompanyModule?> GetAsync(int companyId, int moduleId, CancellationToken ct = default);

        Task<bool> IsEnabledAsync(int companyId, string moduleKey, CancellationToken ct = default);

        Task AddAsync(CompanyModule companyModule, CancellationToken ct = default);

        Task EnableAsync(int companyId, int moduleId, Guid actorUserId, CancellationToken ct = default);
        Task DisableAsync(int companyId, int moduleId, Guid actorUserId, CancellationToken ct = default);

        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
