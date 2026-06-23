using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.Interfaces
{
    public interface IDocumentSequenceService
    {
        Task<string> GenerateNextNumberAsync(
            int companyId,
            string documentType,
            string prefix,
            CancellationToken ct = default);
    }
}
