namespace MahanShop.Application.Features.Admin.Products;

/// <summary>سطر لیست محصول در پنل ادمین (نمای جدید: موجودی واقعی + تعداد گزینه‌ها).</summary>
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

    /// <summary>تعداد گزینه‌های فروش (واریانت) محصول. برای محصول ساده = 0.</summary>
    public int VariantCount { get; set; }

    /// <summary>موجودی واقعی برای نمایش: محصول ساده = Stock خودش؛ محصول دارای گزینه = جمع موجودی گزینه‌ها.</summary>
    public int EffectiveStock { get; set; }

    /// <summary>کمترین قیمت قابل‌نمایش (با تخفیف) برای محصول. ساده = DiscountPrice ?? Price.</summary>
    public long DisplayPrice { get; set; }
}

/// <summary>کارت‌های خلاصهٔ بالای صفحهٔ محصولات.</summary>
public class ProductStatsDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    public int LowStock { get; set; }   // 1..5
    public int OutOfStock { get; set; } // 0
}

/// <summary>نتیجهٔ صفحه‌بندی‌شدهٔ لیست محصولات ادمین + آمار + گزینه‌های فیلتر.</summary>
public class ProductListResult
{
    public List<ProductListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>آمار کلی (مستقل از فیلتر صفحه) برای کارت‌های بالای صفحه.</summary>
    public ProductStatsDto Stats { get; set; } = new();
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

/// <summary>یک ردیف مشخصهٔ فنی روی محصول (برای نمایش در فرم ویرایش).</summary>
public class ProductFeatureItemDto
{
    public int Id { get; set; }           // ProductFeature.Id (برای حذف)
    public int FeatureId { get; set; }
    public string FeatureName { get; set; } = null!;
    public string Value { get; set; } = null!;
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
    /// <summary>مشخصات فنی ثبت‌شده برای این محصول.</summary>
    public List<ProductFeatureItemDto> Features { get; set; } = new();
}
