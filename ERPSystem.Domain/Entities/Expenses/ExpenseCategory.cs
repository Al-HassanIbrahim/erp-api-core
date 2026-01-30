using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;

namespace ERPSystem.Domain.Entities.Expenses
{
    public class ExpenseCategory : AuditableEntity, ICompanyEntity
    {
        public int CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
