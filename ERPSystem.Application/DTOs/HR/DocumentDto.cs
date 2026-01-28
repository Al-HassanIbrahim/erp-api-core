using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR
{
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string DocumentName { get; set; } = null!;
        public string DocumentType { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public long FileSizeBytes { get; set; }
        public string FileExtension { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
        public string? UploadedBy { get; set; }
    }
}
