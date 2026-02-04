using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.CRM
{
    public class CreateLeadDto
    {
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public LeadSource Source { get; set; }
        public LeadStatus Stage { get; set; }
        public decimal? DealValue { get; set; }
        public DateTime? LastContactDate { get; set; }
        public Guid? AssignedToId { get; set; }
    }
}
