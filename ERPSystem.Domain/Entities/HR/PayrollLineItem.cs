using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Domain.Entities.HR
{
    public class PayrollLineItem: ICompanyEntity
    {
        public Guid Id { get; set; }
        public int CompanyId { get; set; }

        [ForeignKey("Payroll")]
        public Guid PayrollId { get; set; }
        public Payroll Payroll { get; set; } = null!;

        [MaxLength(200)]
        public string Description { get; set; } = null!;

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public PayrollLineItemType Type { get; set; }
    }
}
