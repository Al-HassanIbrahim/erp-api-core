using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Entities.HR
{
    public class Attendance: ICompanyEntity
    {
        public int CompanyId { get; set; }
        public Guid Id { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        public TimeOnly? CheckInTime { get; set; }
        public TimeOnly? CheckOutTime { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal WorkedHours { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal OvertimeHours { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsManualEntry { get; set; } = false;

        // For manual entries
        [MaxLength(500)]
        public string? ManualEntryReason { get; set; }

        // Relation
        [ForeignKey("Employee")]
        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }
    }
}
