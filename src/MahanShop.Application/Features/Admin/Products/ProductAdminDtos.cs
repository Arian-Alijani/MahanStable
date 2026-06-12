namespace MahanShop.Application.Features.Admin.Products;

/// <summary>سطر لیست محصول در پنل ادمین.</summary>
public class ProductListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string BrandName { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public long Price { get; set; }
    public long? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public bool HasVariants { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public string? PrimaryImageUrl { get; set; }
}

/// <summary>نتیجهٔ صفحه‌بندی‌شدهٔ لیست محصولات ادمین.</summary>
public class ProductListResult
{
    public List<ProductListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}

/// <summary>یک تصویر گالری در فرم ویرایش.</summary>
public class ProductImageDto
{
    public int Id { get; set; }
    public string Url { get; set; } = null!;
    public string? Alt { get; set; }
    public bool IsMain { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>داده محصول برای فرم ویرایش/ایجاد.</summary>
public class ProductEditDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Slug { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public long Price { get; set; }
    public long? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public bool HasVariants { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public int BrandId { get; set; }
    public int CategoryId { get; set; }
    public List<ProductImageDto> Images { get; set; } = new();
}
