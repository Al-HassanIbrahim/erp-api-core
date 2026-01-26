using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Products;

namespace ERPSystem.Domain.Entities.Sales
{
    public class SalesReturnLine : BaseEntity
    {
        public int SalesReturnId { get; set; }
        public SalesReturn SalesReturn { get; set; } = default!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = default!;

        public int UnitId { get; set; }
        public UnitOfMeasure Unit { get; set; } = default!;

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }

        public string? Notes { get; set; }
    }
}
