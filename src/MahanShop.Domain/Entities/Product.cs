using MahanShop.Domain.Common;

namespace MahanShop.Domain.Entities;

/// <summary>محصول (گوشی/لوازم). قیمت‌ها long (تومان) برای دقت.</summary>
public class Product : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }

    public long Price { get; set; }
    public long? DiscountPrice { get; set; }
    public int Stock { get; set; }

    /// <summary>اگر true → قیمت/موجودی از ProductVariantها؛ اگر false → محصول ساده (Price/Stock خودش).</summary>
    public bool HasVariants { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int ViewCount { get; set; }

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    // روابط
    public int BrandId { get; set; }
    public Brand Brand { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductFeature> Features { get; set; } = new List<ProductFeature>();
    public ICollection<ProductTag> Tags { get; set; } = new List<ProductTag>();
    public ICollection<ProductComment> Comments { get; set; } = new List<ProductComment>();
}
