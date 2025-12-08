using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Products;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Domain.Entities.Inventory
{
    public class InventoryDocumentLine : BaseEntity
    {
        public int InventoryDocumentId { get; set; }
        public int ProductId { get; set; }

        public int WarehouseId { get; set; }     
        public decimal Quantity { get; set; }    
        public int UnitId { get; set; }

        public InventoryLineType LineType { get; set; }  // In | Out

        public decimal? UnitCost { get; set; }
        public string? Notes { get; set; }

        public InventoryDocument Document { get; set; } = default!;
        public Product Product { get; set; } = default!;
        public Warehouse Warehouse { get; set; } = default!;
    }
}