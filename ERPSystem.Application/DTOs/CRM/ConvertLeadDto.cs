using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.CRM
{
    public class ConvertLeadDto
    {
        public int CustomerId { get; set; }
        public bool CreateDeal { get; set; } = false;
        public string DealName { get; set; } = string.Empty;
        public decimal? DealAmount { get; set; }

        public DateOnly? ExpectedCloseDate { get; set; }
        public DealStatus DealStage { get; set; }
        public Guid? OwnerId { get; set; }

    }
}
