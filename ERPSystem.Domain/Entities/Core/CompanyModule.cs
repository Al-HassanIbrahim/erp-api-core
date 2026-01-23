using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;

namespace ERPSystem.Domain.Entities.Core
{
   public class CompanyModule : AuditableEntity
    {
        public int CompanyId { get; set; }
        public Company Company { get; set; } = default!;

        public int ModuleId { get; set; }
        public Module Module { get; set; } = default!;

        public bool IsEnabled { get; set; } = true;
        public DateTime EnabledAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
    }

}
