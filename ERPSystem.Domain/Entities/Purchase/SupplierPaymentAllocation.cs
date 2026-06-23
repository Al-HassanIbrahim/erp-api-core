using ERPSystem.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Entities.Purchase
{
    //  Maps a payment to one or more invoices with exact amounts.
    // ─────────────────────────────────────────────────────────────
    public sealed class SupplierPaymentAllocation : BaseEntity
    {
        public int SupplierPaymentId { get; set; }
        public int PurchaseInvoiceId { get; set; }
        public decimal AllocatedAmount { get; set; }

        // Navigation
        public SupplierPayment SupplierPayment { get; set; } = null!;
        public PurchaseInvoice PurchaseInvoice { get; set; } = null!;
    }
}
