using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ERPSystem.Application.DTOs.Inventory
{
    public class CreateWarehouseDto
    {
        public int? BranchId { get; set; }

        [Required, StringLength(50)]
        public string Code { get; set; } = default!;

        [Required, StringLength(150)]
        public string Name { get; set; } = default!;

        [StringLength(250)]
        public string? Address { get; set; }
    }
}
