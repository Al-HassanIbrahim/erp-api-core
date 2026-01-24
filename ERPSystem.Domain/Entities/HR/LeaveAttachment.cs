using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Entities.HR
{
    public class LeaveAttachment
    {
        public Guid Id { get; set; }

        [Required, MaxLength(200)]
        public string FileName { get; set; } = null!;

        [Required, MaxLength(500)]
        public string FilePath { get; set; } = null!;
        // Relation
        [ForeignKey("LeaveRequest")]
        public Guid LeaveRequestId { get; set; }
        public LeaveRequest LeaveRequest { get; set; } = null!;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
