using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Enums
{
    public enum PurchaseInvoiceStatus
    {
        Draft = 0,
        Posted = 1,
        Cancelled = 2,
    }

    public enum PurchasePaymentStatus
    {
        Unpaid = 0,
        PartiallyPaid = 1,
        Paid = 2,
    }

    public enum PurchaseReturnStatus
    {
        Draft = 0,
        Posted = 1,
        Cancelled = 2,
    }

    public enum SupplierPaymentStatus
    {
        Draft = 0,
        Posted = 1,
        Cancelled = 2,
    }
}
