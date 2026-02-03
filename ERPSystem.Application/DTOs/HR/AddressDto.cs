using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.HR
{
    public class AddressDto
    {

        [Required, MaxLength(100)]
        public string City { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Country { get; set; } = null!;

        [MaxLength(20)]
        public string? PostalCode { get; set; }
    }
}
