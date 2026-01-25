using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;

namespace ERPSystem.Domain.Entities.Core
{
    public class Module : BaseEntity
    {
        public string Key { get; set; } = default!;     // ex: "Inventory"
        public string Name { get; set; } = default!;    // "Inventory Management"
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<CompanyModule> CompanyModules { get; set; } = new List<CompanyModule>();
    }

}
