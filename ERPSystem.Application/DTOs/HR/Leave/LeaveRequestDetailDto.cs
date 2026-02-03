using ERPSystem.Application.DTOs.HR.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR.Leave
{
    public class LeaveRequestDetailDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string EmployeeName { get; set; } = null!;
        public string LeaveType { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int TotalDays { get; set; }
        public string Status { get; set; } = null!;
        public DateTime RequestDate { get; set; }
        public string Reason { get; set; } = null!;
        public EmployeeListDto? Employee { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? RejectedBy { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string? CancelledBy { get; set; }
        public DateTime? CancelledDate { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal BalanceAfter { get; set; }
        public List<DocumentDto> Attachments { get; set; } = new();
    }
}
