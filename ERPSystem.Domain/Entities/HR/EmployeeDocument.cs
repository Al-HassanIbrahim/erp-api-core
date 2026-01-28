using ERPSystem.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Entities.HR
{
    public class EmployeeDocument:ICompanyEntity
    {
        public Guid Id { get; set; }
        public int CompanyId { get; set; }
        [Required, MaxLength(200)]
        public string DocumentName { get; set; } = null!;

        [Required, MaxLength(50)]
        public string DocumentType { get; set; } = null!;

        [Required, MaxLength(500)]
        public string FilePath { get; set; } = null!;

        [Required]
        public long FileSizeBytes { get; set; }

        [Required, MaxLength(50)]
        public string FileExtension { get; set; } = null!;
        //Relation
        [ForeignKey("Employee")]
        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? UploadedBy { get; set; }
    }
}
