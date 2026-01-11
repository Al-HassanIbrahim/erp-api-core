using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.Inventory
{
    public class StockInRequest
    {
        public int CompanyId { get; set; }
        public int? BranchId { get; set; }
        public DateTime DocDate { get; set; }

        public string SourceType { get; set; } = "Manual";
        public int? SourceId { get; set; }
        public string? Notes { get; set; }

        public int CreatedByUserId { get; set; }

        public List<StockInLineRequest> Lines { get; set; } = new();
    }

    public class StockInLineRequest
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public decimal UnitCost { get; set; }
        public string? Notes { get; set; }
    }

    // ---------- Stock Out ----------
    public class StockOutRequest
    {
        public int CompanyId { get; set; }
        public int? BranchId { get; set; }
        public DateTime DocDate { get; set; }

        public string SourceType { get; set; } = "Manual";
        public int? SourceId { get; set; }
        public string? Notes { get; set; }

        public int CreatedByUserId { get; set; }

        public List<StockOutLineRequest> Lines { get; set; } = new();
    }

    public class StockOutLineRequest
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public string? Notes { get; set; }
    }

    // ---------- Transfer ----------
    public class StockTransferRequest
    {
        public int CompanyId { get; set; }
        public int? BranchId { get; set; }
        public DateTime DocDate { get; set; }

        public string SourceType { get; set; } = "Manual";
        public string? Notes { get; set; }

        public int CreatedByUserId { get; set; }

        public List<StockTransferLineRequest> Lines { get; set; } = new();
    }

    public class StockTransferLineRequest
    {
        public int ProductId { get; set; }
        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public string? Notes { get; set; }
    }

    // ---------- Opening Balance ----------
    public class OpeningBalanceRequest
    {
        public int CompanyId { get; set; }
        public int? BranchId { get; set; }
        public DateTime DocDate { get; set; }
        public string? Notes { get; set; }

        public int CreatedByUserId { get; set; }

        public List<OpeningBalanceLineRequest> Lines { get; set; } = new();
    }

    public class OpeningBalanceLineRequest
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public decimal UnitCost { get; set; }
        public string? Notes { get; set; }
    }

    // ---------- Adjustment ----------
    public class StockAdjustmentRequest
    {
        public int CompanyId { get; set; }
        public int? BranchId { get; set; }
        public DateTime DocDate { get; set; }
        public string? Notes { get; set; }

        public int CreatedByUserId { get; set; }

        public List<StockAdjustmentLineRequest> Lines { get; set; } = new();
    }

    public class StockAdjustmentLineRequest
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public decimal ActualQuantity { get; set; }
        public int UnitId { get; set; }
        // Optional: if null, we will use current AverageUnitCost for diff > 0
        public decimal? UnitCost { get; set; }
        public string? Notes { get; set; }
    }

    // ---------- Response ----------
    public class InventoryDocumentResponse
    {
        public int DocumentId { get; set; }
        public string DocNumber { get; set; } = string.Empty;
    }
}
