using ERPSystem.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Entities.Purchase
{
    public sealed class Supplier : AuditableEntity, ICompanyEntity
    {
        public int CompanyId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? TaxNumber { get; set; }
        public decimal CreditLimit { get; set; }
        public int PaymentTermsDays { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = [];
        public ICollection<PurchaseReturn> PurchaseReturns { get; set; } = [];
        public ICollection<SupplierPayment> SupplierPayments { get; set; } = [];
    }
}
