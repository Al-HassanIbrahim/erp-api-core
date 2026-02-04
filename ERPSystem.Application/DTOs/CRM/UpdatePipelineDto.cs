using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.CRM
{
    public class UpdatePipelineDto
    {
        public int CompanyId { get; set; }
        public int CustomerId { get; set; }
        public string DealName { get; set; } = string.Empty;
        public decimal DealAmount { get; set; }
        public int? LeadId { get; set; }

        public DateOnly? ExpectedCloseDate { get; set; }
        public DealStatus DealStage { get; set; }
        public Guid? OwnerId { get; set; }
    }
}
