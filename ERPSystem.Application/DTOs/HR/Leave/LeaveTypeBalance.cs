using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Leave
{
    public class LeaveTypeBalance
    {
        public string LeaveType { get; set; } = null!;
        public decimal TotalEntitlement { get; set; }
        public decimal Used { get; set; }
        public decimal Pending { get; set; }
        public decimal Available { get; set; }
    }
}
