namespace MahanShop.Application.Features.Catalog.Queries.GetCatalog;

/// <summary>ورودی فیلتر/مرور کاتالوگ — از query-string صفحه bind می‌شود (server-side filtering).</summary>
public class CatalogFilter
{
    public string? CategorySlug { get; set; }
    public string? BrandSlug { get; set; }
    public int? ColorId { get; set; }
    public string? Search { get; set; }
    public long? PriceMin { get; set; }
    public long? PriceMax { get; set; }
    public bool InStockOnly { get; set; }

    /// <summary>newest | oldest | price-asc | price-desc | popular</summary>
    public string Sort { get; set; } = "newest";

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
