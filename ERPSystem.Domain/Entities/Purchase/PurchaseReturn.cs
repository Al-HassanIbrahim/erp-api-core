using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Inventory;
using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Entities.Purchase
{
    public sealed class PurchaseReturn : AuditableEntity, ICompanyEntity
    {
        public int CompanyId { get; set; }
        public string ReturnNumber { get; set; } = string.Empty;
        public DateOnly ReturnDate { get; set; }
        public int SupplierId { get; set; }
        /// <summary>Warehouse where returned stock is drawn out from.</summary>
        public int WarehouseId { get; set; }
        /// <summary>Optional link to the originating purchase invoice.</summary>
        public int? PurchaseInvoiceId { get; set; }

        public PurchaseReturnStatus Status { get; set; } = PurchaseReturnStatus.Draft;

        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string? Reason { get; set; }
        public int? InventoryDocumentId { get; set; }

        // Navigation
        public Warehouse Warehouse { get; set; } = null!;
        public Supplier Supplier { get; set; } = null!;
        public PurchaseInvoice? PurchaseInvoice { get; set; }
        public ICollection<PurchaseReturnLine> Lines { get; set; } = [];
    }
}
