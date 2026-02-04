using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Entities.CRM
{
    public class Pipeline: BaseEntity, ICompanyEntity
    {
        public int CompanyId { get; set; }

        public string DealName { get; set; } = string.Empty;

        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        [ForeignKey(nameof(SourceLead))]
        public int? LeadId { get; set; }
        public Lead? SourceLead { get; set; }
        public decimal Amount { get; set; }
        public DealStatus Stage { get; set; }                      
        public DateOnly? ExpectedCloseDate { get; set; }        

        [ForeignKey(nameof(Owner))]
        public Guid? OwnerId { get; set; }
        public Employee? Owner { get; set; }

    }
}
