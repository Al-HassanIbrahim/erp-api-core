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
    public class SalesDelivery : AuditableEntity, ICompanyEntity
    {
        public int CompanyId { get; set; }
        public int? BranchId { get; set; }

        public string DeliveryNumber { get; set; } = default!;
        public DateTime DeliveryDate { get; set; }

        public int SalesInvoiceId { get; set; }
        public SalesInvoice SalesInvoice { get; set; } = default!;

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = default!;

        public SalesDeliveryStatus Status { get; set; } = SalesDeliveryStatus.Draft;

        public string? Notes { get; set; }

        public Guid? PostedByUserId { get; set; }
        public DateTime? PostedAt { get; set; }

        // Link to Inventory document created on posting
        public int? InventoryDocumentId { get; set; }

        // Navigation
        public ICollection<SalesDeliveryLine> Lines { get; set; } = new List<SalesDeliveryLine>();
    }
}
