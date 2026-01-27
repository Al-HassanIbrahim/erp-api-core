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
    public class LeaveBalance: ICompanyEntity
    {
        public Guid Id { get; set; }
        public int CompanyId{ get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public LeaveType LeaveType { get; set; }

        [Required, Column(TypeName = "decimal(5,2)")]
        public decimal TotalEntitlement { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Used { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Pending { get; set; }

        //Relation
        [ForeignKey("Employee")]
        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        [NotMapped]
        public decimal Available => TotalEntitlement - Used - Pending;
    }
}
