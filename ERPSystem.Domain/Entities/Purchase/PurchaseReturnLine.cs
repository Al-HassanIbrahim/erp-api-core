using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Entities.Purchase
{
    public sealed class PurchaseReturnLine : BaseEntity
    {
        public int PurchaseReturnId { get; set; }
        public int ProductId { get; set; }
        public int UnitId { get; set; }
        /// <summary>
        ///   Structural conversion rate (Base Units per this unit).
        ///   Inventory Qty sent to StockOut = Quantity × ConversionRate.
        ///   UnitCost is NOT sent — the Inventory subsystem costs via MAC.
        /// </summary>
        public decimal ConversionRate { get; set; } = 1.0000m;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public Product Product { get; set; } = null!;
        public UnitOfMeasure Unit { get; set; } = null!;
        public PurchaseReturn PurchaseReturn { get; set; } = null!;
    }
}
