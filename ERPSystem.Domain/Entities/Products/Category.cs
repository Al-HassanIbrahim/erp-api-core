using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;

namespace ERPSystem.Domain.Entities.Products
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = default!;

        public string? Description { get; set; }

        // Parent category (optional). 
        // This allows building hierarchical categories. 
        // Example: "Beverages" → parent of "Soft Drinks", "Juices", "Water".
        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }

        // (if this category is the parent) Child categories that belong under it.
        public ICollection<Category> Children { get; set; } = new List<Category>();

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
