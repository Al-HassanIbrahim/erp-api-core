using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Inventory;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Domain.Entities.Sales
{
    public class SalesReturn : AuditableEntity, ICompanyEntity
    {
        public int CompanyId { get; set; }
        public int? BranchId { get; set; }

        public string ReturnNumber { get; set; } = default!;
        public DateTime ReturnDate { get; set; }

        public int? SalesInvoiceId { get; set; }
        public SalesInvoice? SalesInvoice { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = default!;

        public SalesReturnStatus Status { get; set; } = SalesReturnStatus.Draft;

        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }

        public string? Reason { get; set; }
        public string? Notes { get; set; }

        public Guid? PostedByUserId { get; set; }
        public DateTime? PostedAt { get; set; }

        // Link to Inventory document created on posting
        public int? InventoryDocumentId { get; set; }

        // Navigation
        public ICollection<SalesReturnLine> Lines { get; set; } = new List<SalesReturnLine>();
    }
}
