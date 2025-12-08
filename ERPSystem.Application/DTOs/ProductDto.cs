namespace ERPSystem.Application.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public decimal DefaultPrice { get; set; }

        public string? CategoryName { get; set; }
        public string UnitOfMeasureName { get; set; } = default!;

        public bool IsActive { get; set; }
    }
}
