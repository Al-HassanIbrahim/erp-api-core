using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Products;

namespace ERPSystem.Domain.Entities.Sales
{
    public class SalesInvoiceLine : BaseEntity
    {
        public int SalesInvoiceId { get; set; }
        public SalesInvoice SalesInvoice { get; set; } = default!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = default!;

        public int UnitId { get; set; }
        public UnitOfMeasure Unit { get; set; } = default!;

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }

        // Track delivery
        public decimal DeliveredQuantity { get; set; }
        public decimal RemainingQuantity => Quantity - DeliveredQuantity;

        public string? Notes { get; set; }
    }
}
