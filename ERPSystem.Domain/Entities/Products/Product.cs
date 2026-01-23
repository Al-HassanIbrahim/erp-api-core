using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;

namespace ERPSystem.Domain.Entities.Products
{
    public class Product : BaseEntity, ICompanyEntity
    {
        // Internal product code (SKU). Used for quick search and unique identification.
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public int CompanyId { get; set; }
        public string? Description { get; set; }

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }


        public int UnitOfMeasureId { get; set; }
        public UnitOfMeasure UnitOfMeasure { get; set; } = default!;

        public decimal DefaultPrice { get; set; }       
        public string? Barcode { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
