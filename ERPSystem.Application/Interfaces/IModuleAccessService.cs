using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.Interfaces
{
    namespace ERPSystem.Application.Interfaces
    {
        public interface IModuleAccessService
        {
            Task<bool> IsModuleEnabledAsync(int companyId, string moduleCode, CancellationToken cancellationToken = default);
            Task<bool> IsSalesEnabledAsync(CancellationToken cancellationToken = default);
            Task<bool> IsInventoryEnabledAsync(CancellationToken cancellationToken = default);
            Task EnsureSalesEnabledAsync(CancellationToken cancellationToken = default);
            Task EnsureInventoryEnabledAsync(CancellationToken cancellationToken = default);
        }
    }
}
