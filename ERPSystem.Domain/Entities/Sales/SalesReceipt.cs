using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Domain.Entities.Sales
{
    public class SalesReceipt : AuditableEntity, ICompanyEntity
    {
        public int CompanyId { get; set; }
        public int? BranchId { get; set; }

        public string ReceiptNumber { get; set; } = default!;
        public DateTime ReceiptDate { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;

        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }

        public SalesReceiptStatus Status { get; set; } = SalesReceiptStatus.Draft;

        public string? Notes { get; set; }

        public Guid? PostedByUserId { get; set; }
        public DateTime? PostedAt { get; set; }

        // Navigation
        public ICollection<SalesReceiptAllocation> Allocations { get; set; } = new List<SalesReceiptAllocation>();
    }
}
