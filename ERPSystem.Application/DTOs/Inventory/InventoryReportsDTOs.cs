using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.Inventory
{
    // Used for stock balance and valuation endpoints
    public class StockBalanceDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = default!;
        public string ProductName { get; set; } = default!;
        public string? CategoryName { get; set; }

        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = default!;

        public string UnitName { get; set; } = default!;
        public string UnitSymbol { get; set; } = default!;

        public decimal QuantityOnHand { get; set; }
        public decimal AverageUnitCost { get; set; }

        public decimal TotalValue => QuantityOnHand * AverageUnitCost;
    }

    // Used for movement (cardex) reports
    public class InventoryMovementDto
    {
        public int DocumentId { get; set; }
        public string DocNumber { get; set; } = default!;
        public DateTime DocDate { get; set; }
        public string DocType { get; set; } = default!;   // can map from enum name

        public int ProductId { get; set; }
        public string ProductCode { get; set; } = default!;
        public string ProductName { get; set; } = default!;

        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = default!;

        public string LineType { get; set; } = default!;  // In / Out
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost => Quantity * UnitCost;
    }

    // Used for low stock reports
    public class LowStockItemDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = default!;
        public string ProductName { get; set; } = default!;

        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = default!;

        public decimal QuantityOnHand { get; set; }
        public decimal? MinQuantity { get; set; }
    }
}
