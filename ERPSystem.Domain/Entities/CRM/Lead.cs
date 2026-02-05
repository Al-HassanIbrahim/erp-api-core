using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ERPSystem.Domain.Entities.CRM
{

    public class Lead : BaseEntity, ICompanyEntity
    {
        public int CompanyId { get; set; }

        // Info
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string CompanyName { get; set; } = string.Empty;

        public LeadSource Source { get; set; }
        public LeadStatus Stage { get; set; }

        public decimal? DealValue { get; set; }
        public DateTime? LastContactDate { get; set; }

        // HR module
        [ForeignKey(nameof(AssignedTo))]
        public Guid? AssignedToId { get; set; }
        public Employee? AssignedTo { get; set; }

        // Conversion (to existing Customer master)
        [ForeignKey(nameof(ConvertedCustomer))]
        public int? ConvertedCustomerId { get; set; }
        public Customer? ConvertedCustomer { get; set; }
        public DateTime? ConvertedDate { get; set; }


    }
}
