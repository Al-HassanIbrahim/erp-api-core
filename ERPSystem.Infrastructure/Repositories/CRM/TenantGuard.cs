using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Infrastructure.Repositories.CRM
{
    public static class TenantGuard
    {
        public static void EnsureCompany(int requestedCompanyId, int currentCompanyId)
        {
            if (requestedCompanyId != currentCompanyId)
                throw new UnauthorizedAccessException("Cross-company access is not allowed.");
        }
    }
}
