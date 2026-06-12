namespace MahanShop.Application.Features.Home.Queries.GetHomePage;

/// <summary>کارت محصول برای نوارهای صفحه اصلی (و بعداً لیست/جستجو). جدا از entity.</summary>
public class ProductCardDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? BrandName { get; set; }
    public long Price { get; set; }
    public long? DiscountPrice { get; set; }
    public int DiscountPercent { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public List<string> ColorHexes { get; set; } = new();
    public string? CategorySlug { get; set; }

    public bool HasDiscount => DiscountPrice is > 0 && DiscountPrice < Price;
}
