using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Entities.Core;
using Microsoft.AspNetCore.Identity;

namespace ERPSystem.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser<Guid> 
    {
        public int CompanyId { get; set; }
        public Company Company { get; set; } = default!;
    }
}
