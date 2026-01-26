using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Domain.Abstractions
{
    public interface ISalesInvoiceRepository
    {
        Task<SalesInvoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<SalesInvoice?> GetByIdWithLinesAsync(int id, CancellationToken cancellationToken = default);
        Task<List<SalesInvoice>> GetAllByCompanyAsync(
            int companyId,
            int? customerId = null,
            SalesInvoiceStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default);
        Task<string> GenerateInvoiceNumberAsync(int companyId, CancellationToken cancellationToken = default);
        Task AddAsync(SalesInvoice invoice, CancellationToken cancellationToken = default);
        void Update(SalesInvoice invoice);
        void Delete(SalesInvoice invoice);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
