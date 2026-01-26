using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Entities.Inventory;

namespace ERPSystem.Domain.Abstractions
{
    public interface IWarehouseRepository
    {
        Task<List<Warehouse>> GetAllAsync(int companyId, int? branchId = null);
        Task<Warehouse?> GetByIdAsync(int id);
        Task<Warehouse?> GetByIdAsync(int id, int companyId);
        Task<bool> ExistsAsync(int id, int companyId);
        Task<bool> CodeExistsAsync(string code, int companyId, int? excludeId = null);
        Task AddAsync(Warehouse warehouse);

        // This does not remove from database, we will just mark inactive in the service
        Task<bool> HasInventoryActivityAsync(int warehouseId);

        Task SaveChangesAsync();
    }
}
