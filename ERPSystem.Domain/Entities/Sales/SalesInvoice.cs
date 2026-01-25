using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Enums;
namespace ERPSystem.Domain.Entities.Sales
{
    public class SalesInvoice : AuditableEntity, ICompanyEntity
    {
        public int CompanyId { get; set; }
        public int? BranchId { get; set; }

        public string InvoiceNumber { get; set; } = default!;
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;

        public SalesInvoiceStatus Status { get; set; } = SalesInvoiceStatus.Draft;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceDue => GrandTotal - PaidAmount;

        public string? Notes { get; set; }

        public Guid? PostedByUserId { get; set; }
        public DateTime? PostedAt { get; set; }

        public ICollection<SalesInvoiceLine> Lines { get; set; } = new List<SalesInvoiceLine>();
        public ICollection<SalesDelivery> Deliveries { get; set; } = new List<SalesDelivery>();
        public ICollection<SalesReceiptAllocation> ReceiptAllocations { get; set; } = new List<SalesReceiptAllocation>();
    }
}
