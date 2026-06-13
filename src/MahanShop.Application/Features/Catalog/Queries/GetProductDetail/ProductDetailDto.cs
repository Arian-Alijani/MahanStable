using MahanShop.Domain.Enums;

namespace MahanShop.Application.Features.Catalog.Queries.GetProductDetail;

/// <summary>چینش UI صفحهٔ محصول: ساده (swatch/dropdown) یا مبتنی بر دستگاه (برند→مدل[→رنگ]).</summary>
public enum ProductVariantLayout
{
    Simple = 0,
    Device = 1
}

public class ProductImageDto
{
    public string Url { get; set; } = null!;
    public string? Alt { get; set; }
    public bool IsMain { get; set; }
}

/// <summary>یک ویژگی متغیر محصول + مقادیر قابل‌انتخاب آن (برای ساخت selector).</summary>
public class ProductAttributeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsColor { get; set; }
    public VariantAttributeKind Kind { get; set; }
    public List<ProductAttributeValueDto> Values { get; set; } = new();
}

public class ProductAttributeValueDto
{
    public int Id { get; set; }   // VariantAttributeValue.Id
    public string Value { get; set; } = null!;
    public string? ColorHex { get; set; }
    public string? LogoUrl { get; set; }
}

/// <summary>یک گزینه فروش (variant) با قیمت/موجودی مستقل + idهای مقادیر ویژگیِ سازنده‌اش.</summary>
public class ProductVariantDto
{
    public int Id { get; set; }
    public long Price { get; set; }
    public long? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public List<int> ValueIds { get; set; } = new();

    public long FinalPrice => DiscountPrice is > 0 && DiscountPrice < Price ? DiscountPrice.Value : Price;
    public bool InStock => Stock > 0;
}

public class ProductFeatureDto
{
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
}

public class ProductTagDto
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
}

/// <summary>مدل کامل صفحه جزییات محصول (Phase 4B). جدا از entity.</summary>
public class ProductDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }

    public bool HasVariants { get; set; }
    public long Price { get; set; }
    public long? DiscountPrice { get; set; }
    public int DiscountPercent { get; set; }
    public int Stock { get; set; }
    public bool InStock => HasVariants ? Variants.Any(v => v.InStock) : Stock > 0;

    public string? BrandName { get; set; }
    public string? BrandSlug { get; set; }
    public string CategoryName { get; set; } = null!;
    public string CategorySlug { get; set; } = null!;

    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    public List<ProductImageDto> Images { get; set; } = new();
    public List<ProductAttributeDto> Attributes { get; set; } = new();
    public List<ProductVariantDto> Variants { get; set; } = new();
    public List<ProductFeatureDto> Features { get; set; } = new();
    public List<ProductTagDto> Tags { get; set; } = new();

    public long FinalPrice => DiscountPrice is > 0 && DiscountPrice < Price ? DiscountPrice.Value : Price;
    public bool HasDiscount => DiscountPrice is > 0 && DiscountPrice < Price;

    /// <summary>برند در میان ویژگی‌ها؟</summary>
    public bool HasBrandAttr => Attributes.Any(a => a.Kind == VariantAttributeKind.Brand);
    public bool HasModelAttr => Attributes.Any(a => a.Kind == VariantAttributeKind.Model);

    /// <summary>اگر هم برند هم مدل داشت → جریان دستگاه؛ وگرنه ساده.</summary>
    public ProductVariantLayout Layout =>
        HasVariants && HasBrandAttr && HasModelAttr ? ProductVariantLayout.Device : ProductVariantLayout.Simple;
}
