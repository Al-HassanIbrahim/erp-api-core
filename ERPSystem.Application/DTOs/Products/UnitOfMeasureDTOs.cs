namespace ERPSystem.Application.DTOs.Products
{
    public class UnitOfMeasureDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Symbol { get; set; } = default!;
    }

    public class CreateUnitOfMeasureRequest
    {
        public string Name { get; set; } = default!;
        public string Symbol { get; set; } = default!;
    }

    public class UpdateUnitOfMeasureRequest
    {
        public string Name { get; set; } = default!;
        public string Symbol { get; set; } = default!;
    }
}