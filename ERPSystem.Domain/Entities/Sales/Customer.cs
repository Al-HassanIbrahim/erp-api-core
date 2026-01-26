using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ERPSystem.Domain.Abstractions;

namespace ERPSystem.Domain.Entities.Sales
{
    public class Customer : AuditableEntity, ICompanyEntity
    {
        public int CompanyId { get; set; }

        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? TaxNumber { get; set; }

        public decimal CreditLimit { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<SalesInvoice> Invoices { get; set; } = new List<SalesInvoice>();
        public ICollection<SalesReceipt> Receipts { get; set; } = new List<SalesReceipt>();
    }
}
