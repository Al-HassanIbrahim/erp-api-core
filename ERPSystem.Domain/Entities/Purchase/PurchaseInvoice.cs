using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Inventory;
using ERPSystem.Domain.Entities.Purchase;
using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Entities.Purchase
{
    public sealed class PurchaseInvoice : AuditableEntity, ICompanyEntity
    {
        public int CompanyId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateOnly InvoiceDate { get; set; }
        public DateOnly DueDate { get; set; }
        public int SupplierId { get; set; }
        /// <summary>Warehouse where goods are received into stock.</summary>
        public int WarehouseId { get; set; }

        public PurchaseInvoiceStatus Status { get; set; } = PurchaseInvoiceStatus.Draft;
        public PurchasePaymentStatus PaymentStatus { get; set; } = PurchasePaymentStatus.Unpaid;

        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public int? InventoryDocumentId { get; set; }

        /// <summary>Computed; not persisted to the database.</summary>
        [NotMapped]
        public decimal BalanceDue => GrandTotal - PaidAmount;

        // Navigation
        public Warehouse Warehouse { get; set; } = null!;
        public Supplier Supplier { get; set; } = null!;
        public ICollection<PurchaseInvoiceLine> Lines { get; set; } = [];
        public ICollection<SupplierPaymentAllocation> PaymentAllocations { get; set; } = [];
    }
}
