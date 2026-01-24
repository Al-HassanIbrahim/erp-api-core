using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Enums;
namespace ERPSystem.Domain.Entities.HR
{
    public class LeaveRequest
    {
        public Guid Id { get; set; }

        [Required]
        public LeaveType LeaveType { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        [Required]
        public int TotalDays { get; set; }

        [Required]
        public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;

        [Required]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(500)]
        public string Reason { get; set; } = null!;

        // Approval/Rejection Details
        [MaxLength(100)]
        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

        [MaxLength(100)]
        public string? RejectedBy { get; set; }

        public DateTime? RejectedDate { get; set; }

        // Cancellation Details
        [MaxLength(100)]
        public string? CancelledBy { get; set; }

        public DateTime? CancelledDate { get; set; }

        // Balance tracking
        [Column(TypeName = "decimal(5,2)")]
        public decimal CurrentBalance { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal BalanceAfter { get; set; }

        // Relation 
        [ForeignKey("Employee")]
        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        // Collections
        public ICollection<LeaveAttachment> Attachments { get; set; } = new List<LeaveAttachment>();
    }
}
