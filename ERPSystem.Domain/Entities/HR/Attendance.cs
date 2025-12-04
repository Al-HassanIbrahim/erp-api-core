using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Entities.HR
{
    public class Attendance
    {
        public int Id { get; set; }

        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public DateOnly Date { get; set; }

        public TimeOnly? CheckIn { get; set; }
        public TimeOnly? CheckOut { get; set; }

        public TimeSpan? TotalHours =>
            CheckIn != null && CheckOut != null ? CheckOut.Value - CheckIn.Value : null;

        
        public bool IsPaid { get; set; } = true;   
    }
}
