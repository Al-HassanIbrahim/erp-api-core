using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Entities.Purchase
{
    public sealed class SupplierPayment : AuditableEntity, ICompanyEntity
    {
        public int CompanyId { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public DateOnly PaymentDate { get; set; }
        public int SupplierId { get; set; }
        public decimal Amount { get; set; }
        public string? Reference { get; set; }
        public string? Notes { get; set; }

        public SupplierPaymentStatus Status { get; set; } = SupplierPaymentStatus.Draft;

        // Navigation
        public Supplier Supplier { get; set; } = null!;
        public ICollection<SupplierPaymentAllocation> Allocations { get; set; } = [];
    }
}
