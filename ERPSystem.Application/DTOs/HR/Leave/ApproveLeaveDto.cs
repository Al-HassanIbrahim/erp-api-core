using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Leave
{
    public class ApproveLeaveDto
    {
        [MaxLength(500)]
        public string? Notes { get; set; }
    }


}
