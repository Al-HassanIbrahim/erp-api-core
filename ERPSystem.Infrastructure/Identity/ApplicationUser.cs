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
        public string FullName { get; set; } = default!;
        public string? ProfileImageUrl { get; set; }

        // Soft delete properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedByUserId { get; set; }
    }
}
