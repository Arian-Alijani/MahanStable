using MahanShop.Application.Features.Home.Queries.GetHomePage;

namespace MahanShop.Application.Features.Catalog.Queries.GetCatalog;

/// <summary>دسته در sidebar فیلتر (درخت یک‌سطحی منو).</summary>
public class CategoryFilterDto
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public bool Active { get; set; }
    public List<CategoryFilterDto> Children { get; set; } = new();
}

/// <summary>رنگ قابل فیلتر.</summary>
public class ColorFilterDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Hex { get; set; } = "#000000";
    public bool Active { get; set; }
}

/// <summary>برند قابل فیلتر.</summary>
public class BrandFilterDto
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public bool Active { get; set; }
}

/// <summary>ViewModel کامل صفحه کاتالوگ — محصولات صفحه فعلی + facetها + وضعیت فیلتر.</summary>
public class CatalogViewModel
{
    public List<ProductCardDto> Products { get; set; } = new();

    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPrev => Page > 1;
    public bool HasNext => Page < TotalPages;

    // facetها برای sidebar
    public List<CategoryFilterDto> Categories { get; set; } = new();
    public List<ColorFilterDto> Colors { get; set; } = new();
    public List<BrandFilterDto> Brands { get; set; } = new();
    public long PriceFloor { get; set; }
    public long PriceCeil { get; set; }

    // وضعیت اعمال‌شده (برای فرم + chipها + canonical)
    public CatalogFilter Applied { get; set; } = new();
    public string? ActiveCategoryName { get; set; }
}
