using ERPSystem.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Entities.Purchase
{
    // ─────────────────────────────────────────────────────────────
    //  DocumentSequence
    //  Provides thread-safe, gap-free sequential document numbers
    //  per company × document-type × calendar-month.
    // ─────────────────────────────────────────────────────────────
    public sealed class DocumentSequence : BaseEntity, ICompanyEntity
    {
        public int CompanyId { get; set; }
        /// <summary>"Invoice" | "Return" | "Payment"</summary>
        public string DocumentType { get; set; } = string.Empty;
        /// <summary>Format: "yyyyMM" — e.g. "202606"</summary>
        public string YearMonth { get; set; } = string.Empty;
        public int LastSequence { get; set; }
    }
}
