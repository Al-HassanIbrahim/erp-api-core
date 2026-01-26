using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Products;

namespace ERPSystem.Domain.Entities.Sales
{
    public class SalesDeliveryLine : BaseEntity
    {
        public int SalesDeliveryId { get; set; }
        public SalesDelivery SalesDelivery { get; set; } = default!;

        public int SalesInvoiceLineId { get; set; }
        public SalesInvoiceLine SalesInvoiceLine { get; set; } = default!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = default!;

        public int UnitId { get; set; }
        public UnitOfMeasure Unit { get; set; } = default!;

        public decimal Quantity { get; set; }

        public string? Notes { get; set; }
    }
}
