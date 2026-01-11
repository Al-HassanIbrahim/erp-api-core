using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.Inventory
{
    public class CreateWarehouseDto
    {
        public int CompanyId { get; set; }      
        public int? BranchId { get; set; }

        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Address { get; set; }
    }
}
