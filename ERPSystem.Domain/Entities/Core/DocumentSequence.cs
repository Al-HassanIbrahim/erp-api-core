using ERPSystem.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Entities.Core
{
    public sealed class DocumentSequence : ICompanyEntity
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string DocumentType { get; set; } = string.Empty; // "SalesInvoice", "SalesDelivery", "InventoryDocument", etc.
        public string YearMonth { get; set; } = string.Empty;    // "202606"
        public int LastSequence { get; set; }
    }
}
