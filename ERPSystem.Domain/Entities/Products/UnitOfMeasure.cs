using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;

namespace ERPSystem.Domain.Entities.Products
{
    public class UnitOfMeasure : BaseEntity
    {
        public string Name { get; set; } = default!;

        public string Symbol { get; set; } = default!;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
