using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Abstractions
{
    public abstract class AuditableEntity : BaseEntity
    {
        public Guid CreatedByUserId { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public Guid? DeletedByUserId { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
