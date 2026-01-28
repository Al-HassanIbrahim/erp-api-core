using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Leave
{
    public class RejectLeaveDto
    {
        [Required, MaxLength(500)]
        public string Reason { get; set; } = null!;
    }
}
