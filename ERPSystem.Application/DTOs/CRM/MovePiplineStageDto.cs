using ERPSystem.Domain.Entities.CRM;
using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.CRM
{
    public class MovePiplineStageDto
    {
        public DealStatus Stage { get; set; }

        public void ApplyTo(Pipeline pipeline)
        {
            pipeline.Stage = Stage;
        }
    }
}
