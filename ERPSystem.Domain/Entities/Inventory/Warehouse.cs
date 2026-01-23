using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;

namespace ERPSystem.Domain.Entities.Inventory
{
    public class Warehouse : BaseEntity, ICompanyEntity
    {
        public int CompanyId { get; set; }        
        public int? BranchId { get; set; } 

        public string Code { get; set; } = default!; 
        public string Name { get; set; } = default!;
        public string? Address { get; set; }         
        public bool IsActive { get; set; } = true;
   
        public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
        public ICollection<InventoryDocument> InventoryDocuments { get; set; } = new List<InventoryDocument>();
        public ICollection<InventoryDocumentLine> InventoryDocumentLines { get; set; } = new List<InventoryDocumentLine>();
    }
}
