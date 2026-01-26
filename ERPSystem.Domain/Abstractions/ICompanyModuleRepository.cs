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
        Task<CompanyModule?> GetAsync(int companyId, int moduleId, CancellationToken ct = default);
        Task<List<CompanyModule>> GetByCompanyAsync(int companyId, CancellationToken ct = default);
        Task<bool> IsModuleEnabledAsync(int companyId, string moduleCode, CancellationToken ct = default);
        Task EnableAsync(int companyId, int moduleId, Guid actorUserId, CancellationToken ct = default);
        Task DisableAsync(int companyId, int moduleId, Guid actorUserId, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
