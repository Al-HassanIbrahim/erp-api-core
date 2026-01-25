using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.Core
{
    public class CompanyModuleDto
    {
        public int ModuleId { get; set; }
        public string ModuleKey { get; set; } = default!;
        public string ModuleName { get; set; } = default!;
        public bool IsEnabled { get; set; }
        public DateTime? EnabledAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class ToggleCompanyModuleDto
    {
        public bool IsEnabled { get; set; }
    }
}
