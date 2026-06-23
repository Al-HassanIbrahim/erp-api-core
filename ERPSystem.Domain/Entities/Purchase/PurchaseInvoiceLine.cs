using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Entities.Purchase
{
    public sealed class PurchaseInvoiceLine : BaseEntity
    {
        public int PurchaseInvoiceId { get; set; }
        public int ProductId { get; set; }
        /// <summary>The unit used on the purchase (e.g. Box).</summary>
        public int UnitId { get; set; }
        /// <summary>
        ///   How many Base Units fit inside this unit.
        ///   Defaults to 1 (unit IS the base unit).
        ///   Inventory Qty  = Quantity × ConversionRate
        ///   Inventory Cost = UnitPrice / ConversionRate
        /// </summary>
        public decimal ConversionRate { get; set; } = 1.0000m;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public Product Product { get; set; } = null!;
        public UnitOfMeasure Unit { get; set; } = null!;
        public PurchaseInvoice PurchaseInvoice { get; set; } = null!;
    }
}
