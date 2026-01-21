using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Domain.Entities.Inventory
{
    public class InventoryDocument : AuditableEntity, ICompanyEntity
    {
        public int CompanyId { get; set; }
        public int? BranchId { get; set; }

        public string DocNumber { get; set; } = default!;
        public DateTime DocDate { get; set; }

        public InventoryDocType DocType { get; set; }
        public InventoryDocumentStatus Status { get; set; } = InventoryDocumentStatus.Draft;

        public int? DefaultWarehouseId { get; set; }
        public string? Notes { get; set; }
        public Guid? PostedByUserId { get; set; }
        public DateTime? PostedAt { get; set; }

        public string? SourceType { get; set; }
        public int? SourceId { get; set; }

        public Warehouse? DefaultWarehouse { get; set; }
        public ICollection<InventoryDocumentLine> Lines { get; set; } = new List<InventoryDocumentLine>();
    }
}