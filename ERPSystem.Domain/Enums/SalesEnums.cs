using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Enums
{
    public enum SalesInvoiceStatus
    {
        Draft = 0,
        Posted = 1,
        PartiallyDelivered = 2,
        FullyDelivered = 3,
        Cancelled = 4
    }

    public enum PaymentStatus
    {
        Unpaid = 0,
        PartiallyPaid = 1,
        Paid = 2
    }

    public enum SalesDeliveryStatus
    {
        Draft = 0,
        Posted = 1,
        Cancelled = 2
    }

    public enum SalesReturnStatus
    {
        Draft = 0,
        Posted = 1,
        Cancelled = 2
    }

    public enum SalesReceiptStatus
    {
        Draft = 0,
        Posted = 1,
        Cancelled = 2
    }
}
