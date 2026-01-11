using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Inventory;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Inventory
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly AppDbContext _context;

        public WarehouseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Warehouse>> GetAllAsync(int? companyId = null, int? branchId = null)
        {
            IQueryable<Warehouse> query = _context.Warehouses;

            if (companyId.HasValue)
                query = query.Where(w => w.CompanyId == companyId.Value);

            if (branchId.HasValue)
                query = query.Where(w => w.BranchId == branchId.Value);

            return await query
                .Where(w => !w.IsDeleted)
                .ToListAsync();
        }

        public async Task<Warehouse?> GetByIdAsync(int id)
        {
            return await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
        }

        public async Task<bool> CodeExistsAsync(string code, int? companyId = null, int? excludeId = null)
        {
            IQueryable<Warehouse> query = _context.Warehouses;

            if (companyId.HasValue)
                query = query.Where(w => w.CompanyId == companyId.Value);

            if (excludeId.HasValue)
                query = query.Where(w => w.Id != excludeId.Value);

            return await query.AnyAsync(w => w.Code == code && !w.IsDeleted);
        }

        public async Task AddAsync(Warehouse warehouse)
        {
            await _context.Warehouses.AddAsync(warehouse);
        }

        public async Task<bool> HasInventoryActivityAsync(int warehouseId)
        {
            var hasDocuments = await _context.InventoryDocuments
                .AnyAsync(d => d.DefaultWarehouseId == warehouseId && !d.IsDeleted);

            var hasLines = await _context.InventoryDocumentLines
                .AnyAsync(l => l.WarehouseId == warehouseId && !l.IsDeleted);

            var hasStock = await _context.StockItems
                .AnyAsync(s => s.WarehouseId == warehouseId && !s.IsDeleted);

            return hasDocuments || hasLines || hasStock;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
