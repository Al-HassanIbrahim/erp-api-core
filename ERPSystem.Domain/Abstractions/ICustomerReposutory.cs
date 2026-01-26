using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Entities.Sales;

namespace ERPSystem.Domain.Abstractions
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Customer>> GetAllByCompanyAsync(int companyId, bool? isActive = null, CancellationToken cancellationToken = default);
        Task<Customer?> GetByCodeAsync(int companyId, string code, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int companyId, string code, int? excludeId = null, CancellationToken cancellationToken = default);
        Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
        void Update(Customer customer);
        void Delete(Customer customer);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
