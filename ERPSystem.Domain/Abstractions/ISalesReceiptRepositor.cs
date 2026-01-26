using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Domain.Abstractions
{
    public interface ISalesReceiptRepository
    {
        Task<SalesReceipt?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<SalesReceipt?> GetByIdWithAllocationsAsync(int id, CancellationToken cancellationToken = default);
        Task<List<SalesReceipt>> GetAllByCompanyAsync(
            int companyId,
            int? customerId = null,
            SalesReceiptStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default);
        Task<string> GenerateReceiptNumberAsync(int companyId, CancellationToken cancellationToken = default);
        Task AddAsync(SalesReceipt receipt, CancellationToken cancellationToken = default);
        void Update(SalesReceipt receipt);
        void Delete(SalesReceipt receipt);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
