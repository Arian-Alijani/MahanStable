namespace MahanShop.Application.Features.Home.Queries.GetMenuCategories;

/// <summary>گره دسته‌بندی منوی هدر (مگامنو). تو سطح (والد + فرزندان).</summary>
public class MenuCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public List<MenuCategoryDto> Children { get; set; } = new();
}
