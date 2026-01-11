using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Products;

namespace ERPSystem.Domain.Entities.Inventory
{
    public class StockItem : BaseEntity
    {
        public int WarehouseId { get; set; }
        public int ProductId { get; set; }

        public decimal QuantityOnHand { get; set; }
        public decimal? MinQuantity { get; set; }   
        public decimal? MaxQuantity { get; set; }  

        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        // Average cost per unit for this stock item (moving average)
        public decimal AverageUnitCost { get; set; }

        public Warehouse Warehouse { get; set; } = default!;
        public Product Product { get; set; } = default!;
    }
}