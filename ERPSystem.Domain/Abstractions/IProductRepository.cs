using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Entities.Products;

namespace ERPSystem.Domain.Abstractions
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<Product?> GetByIdAsync(int id, int companyId);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetAllByCompanyAsync(int companyId);
        Task<bool> ExistsAsync(int id, int companyId);
        Task<bool> CodeExistsAsync(string code, int companyId, int? excludeId = null);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task SaveChangesAsync();
    }
}
