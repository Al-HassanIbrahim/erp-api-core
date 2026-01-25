using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.Core
{
    public class CompanyMeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? CommercialName { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateCompanyMeDto
    {
        public string Name { get; set; } = default!;
        public string? CommercialName { get; set; }
        public string? Address { get; set; }
    }
}