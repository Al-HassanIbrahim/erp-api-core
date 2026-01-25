using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;

namespace ERPSystem.Domain.Entities.Sales
{
    public class SalesReceiptAllocation : BaseEntity
    {
        public int SalesReceiptId { get; set; }
        public SalesReceipt SalesReceipt { get; set; } = default!;

        public int SalesInvoiceId { get; set; }
        public SalesInvoice SalesInvoice { get; set; } = default!;

        public decimal AllocatedAmount { get; set; }
    }
}
