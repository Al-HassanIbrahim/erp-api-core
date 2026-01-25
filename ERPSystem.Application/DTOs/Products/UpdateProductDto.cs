namespace ERPSystem.Application.DTOs.Products
{
    public class UpdateProductDto
    {
        public int Id { get; set; }

        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }

        public int? CategoryId { get; set; }
        public int UnitOfMeasureId { get; set; }

        public decimal DefaultPrice { get; set; }
        public string? Barcode { get; set; }

        public bool IsActive { get; set; }
    }
}
