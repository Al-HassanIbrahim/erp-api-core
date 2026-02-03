using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Leave
{
    public class LeaveBalanceDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public int Year { get; set; }
        public List<LeaveTypeBalance> Balances { get; set; } = new();
    }
}
