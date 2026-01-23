namespace ERPSystem.Application.DTOs.Products
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
    }

    public class CreateCategoryRequest
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
    }

    public class UpdateCategoryRequest
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
    }
}