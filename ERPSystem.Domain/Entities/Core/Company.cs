using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;

namespace ERPSystem.Domain.Entities.Core
{
    public class Company : AuditableEntity
    {
        public string Name { get; set; } = default!;
        public string? TaxNumber { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<CompanyModule> CompanyModules { get; set; } = new List<CompanyModule>();
    }
}
